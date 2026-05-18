using System.Text.Json;

namespace MroPlan.Services
{
    /// <summary>
    /// Resmi tatiller, hafta sonları ve mola sürelerini dikkate alarak
    /// iş emirleri için tahmini bitiş tarihi hesaplar.
    /// </summary>
    public class ZamanYonetimServisi(
        HttpClient http,
        ILogger<ZamanYonetimServisi> logger)
    {
        // ── Mesai tanımı ──────────────────────────────────────────────────
        private static readonly TimeOnly MesaiBas = new(8, 0);
        private static readonly TimeOnly MesaiBit = new(18, 0);

        // Mola blokları: (başlangıç, bitiş)
        private static readonly (TimeOnly Bas, TimeOnly Bit)[] Molalar =
        [
            (new(10,  0), new(10, 15)),   // sabah çay
            (new(12,  0), new(13,  0)),   // öğle
            (new(15,  0), new(15, 15)),   // ikindi çay
        ];

        // Toplam mola = 90 dk → etkin çalışma = 510 dk/gün
        private const int EtkinCalismaDkGun = 510;

        // ── Tatil cache ───────────────────────────────────────────────────
        private readonly Dictionary<int, HashSet<DateOnly>> _tatilCache = new();
        private readonly SemaphoreSlim _lock = new(1, 1);

        // Hardcoded fallback — API erişilemezse kullanılır
        private static readonly HashSet<string> FallbackTatiller =
        [
            // 2025
            "2025-01-01","2025-04-23","2025-05-01","2025-05-19",
            "2025-06-06","2025-06-07","2025-06-08","2025-06-09",
            "2025-07-15","2025-08-30","2025-10-29",
            // 2026
            "2026-01-01","2026-04-23","2026-05-01","2026-05-19",
            "2026-05-26","2026-05-27","2026-05-28","2026-05-29",
            "2026-07-15","2026-08-30","2026-10-29",
            // 2027
            "2027-01-01","2027-04-23","2027-05-01","2027-05-19",
            "2027-07-15","2027-08-30","2027-10-29",
        ];

        // ── Ana metot ────────────────────────────────────────────────────
        /// <summary>
        /// Verilen başlangıç tarihinden itibaren toplamDakika çalışma süresi
        /// geçince hangi tarih/saatte biteceğini hesaplar.
        /// Hafta sonları, resmi tatiller ve mola blokları atlanır.
        /// </summary>
        public async Task<DateTime> TahminiBitisHesaplaAsync(
            DateTime baslangicUtc, int toplamDakika)
        {
            var an = baslangicUtc.Kind == DateTimeKind.Utc
                ? baslangicUtc.ToLocalTime()
                : baslangicUtc;

            // Başlangıcı mesai saatine normalize et
            an = await NormalizeEtAsync(an);

            int kalan = toplamDakika;
            int guvenlik = 0;

            while (kalan > 0 && guvenlik++ < 365)
            {
                var gun = DateOnly.FromDateTime(an);

                // İş günü değilse bir sonraki iş günü 08:00'e atla
                if (!await IsGunuMuAsync(gun))
                {
                    an = await SonrakiIsGunuAsync(an.Date.AddDays(1));
                    continue;
                }

                // O andaki saatten başlayarak o gün içindeki çalışma bloklarını bul
                var bloklar = GunCalismaBloklari(an.TimeOfDay);

                foreach (var (blosBas, blokBit) in bloklar)
                {
                    int blokDk = (int)(blokBit - blosBas).TotalMinutes;
                    if (kalan <= blokDk)
                    {
                        // Bu blok içinde bitiyor
                        var bitisZaman = an.Date.Add(blosBas).AddMinutes(kalan);
                        return DateTime.SpecifyKind(bitisZaman.ToUniversalTime(), DateTimeKind.Utc);
                    }
                    kalan -= blokDk;
                }

                // O gün tükendi, ertesi iş günü 08:00
                an = await SonrakiIsGunuAsync(an.Date.AddDays(1));
            }

            return DateTime.SpecifyKind(an.ToUniversalTime(), DateTimeKind.Utc);
        }

        /// <summary>
        /// İki tarih arasındaki toplam çalışılabilir dakika (tatil/mola çıkarılmış).
        /// Raporlama ve Gantt için kullanılır.
        /// </summary>
        public async Task<int> ToplamCalismaDkAsync(DateTime basUtc, DateTime bitUtc)
        {
            var bas = basUtc.ToLocalTime();
            var bit = bitUtc.ToLocalTime();
            int toplam = 0;

            var gun = DateOnly.FromDateTime(bas);
            var sonGun = DateOnly.FromDateTime(bit);

            while (gun <= sonGun)
            {
                if (await IsGunuMuAsync(gun))
                {
                    var gunBas = gun == DateOnly.FromDateTime(bas)
                        ? bas.TimeOfDay > MesaiBas.ToTimeSpan() ? bas.TimeOfDay : MesaiBas.ToTimeSpan()
                        : MesaiBas.ToTimeSpan();
                    var gunBit = gun == sonGun
                        ? bit.TimeOfDay < MesaiBit.ToTimeSpan() ? bit.TimeOfDay : MesaiBit.ToTimeSpan()
                        : MesaiBit.ToTimeSpan();

                    var bloklar = GunCalismaBloklari(gunBas, gunBit);
                    toplam += bloklar.Sum(b => (int)(b.Bit - b.Bas).TotalMinutes);
                }
                gun = gun.AddDays(1);
            }
            return toplam;
        }

        /// <summary>
        /// Verilen günün iş günü olup olmadığını döner.
        /// </summary>
        public async Task<bool> IsGunuMuAsync(DateOnly gun)
        {
            if (gun.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                return false;

            var tatiller = await GetTatillerAsync(gun.Year);
            return !tatiller.Contains(gun);
        }

        // ── Yardımcılar ──────────────────────────────────────────────────

        private async Task<DateTime> NormalizeEtAsync(DateTime an)
        {
            var gun = DateOnly.FromDateTime(an);

            // İş günü değilse ileri sar
            if (!await IsGunuMuAsync(gun))
                return await SonrakiIsGunuAsync(an.Date.AddDays(1));

            var saat = TimeOnly.FromDateTime(an);

            if (saat >= MesaiBit)
                return await SonrakiIsGunuAsync(an.Date.AddDays(1));

            if (saat < MesaiBas)
                return an.Date.Add(MesaiBas.ToTimeSpan());

            // Mola içindeyse molanın bitişine atla
            foreach (var (mbas, mbit) in Molalar)
                if (saat >= mbas && saat < mbit)
                    return an.Date.Add(mbit.ToTimeSpan());

            return an;
        }

        private async Task<DateTime> SonrakiIsGunuAsync(DateTime gun)
        {
            int guvenlik = 0;
            while (guvenlik++ < 14)
            {
                if (await IsGunuMuAsync(DateOnly.FromDateTime(gun)))
                    return gun.Date.Add(MesaiBas.ToTimeSpan());
                gun = gun.AddDays(1);
            }
            return gun.Date.Add(MesaiBas.ToTimeSpan());
        }

        /// <summary>
        /// Verilen başlangıç saatinden (ve opsiyonel bitiş saatinden) o gün
        /// içindeki çalışma bloklarını döner — molalar çıkarılmış.
        /// </summary>
        private static List<(TimeSpan Bas, TimeSpan Bit)> GunCalismaBloklari(
            TimeSpan basSaat,
            TimeSpan? bitSaat = null)
        {
            var bit = bitSaat ?? MesaiBit.ToTimeSpan();
            var result = new List<(TimeSpan, TimeSpan)>();

            // Mesai blokları (molalar arası)
            var sinirlar = new List<TimeSpan> { MesaiBas.ToTimeSpan() };
            foreach (var (mb, mbit) in Molalar)
            {
                sinirlar.Add(mb.ToTimeSpan());
                sinirlar.Add(mbit.ToTimeSpan());
            }
            sinirlar.Add(MesaiBit.ToTimeSpan());

            for (int i = 0; i < sinirlar.Count - 1; i += 2)
            {
                var blokBas = sinirlar[i];
                var blokBit = sinirlar[i + 1];

                // basSaat ve bitSaat ile kesişim
                var etkinBas = basSaat > blokBas ? basSaat : blokBas;
                var etkinBit = bit < blokBit ? bit : blokBit;

                if (etkinBit > etkinBas)
                    result.Add((etkinBas, etkinBit));
            }

            return result;
        }

        // ── Tatil API ─────────────────────────────────────────────────────

        private async Task<HashSet<DateOnly>> GetTatillerAsync(int yil)
        {
            if (_tatilCache.TryGetValue(yil, out var cache))
                return cache;

            await _lock.WaitAsync();
            try
            {
                if (_tatilCache.TryGetValue(yil, out cache)) return cache;

                var tatiller = await ApidenCekAsync(yil)
                    ?? FallbackTatillerGetir(yil);

                _tatilCache[yil] = tatiller;
                return tatiller;
            }
            finally { _lock.Release(); }
        }

        private async Task<HashSet<DateOnly>?> ApidenCekAsync(int yil)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                var json = await http.GetStringAsync(
                    $"PublicHolidays/{yil}/TR", cts.Token);

                var items = JsonSerializer.Deserialize<List<NagerHoliday>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (items == null) return null;

                logger.LogInformation("Nager.Date: {yil} için {sayi} tatil yüklendi", yil, items.Count);
                return items
                    .Select(h => DateOnly.Parse(h.Date))
                    .ToHashSet();
            }
            catch (Exception ex)
            {
                logger.LogWarning("Nager.Date API erişilemedi ({msg}), fallback kullanılıyor", ex.Message);
                return null;
            }
        }

        private static HashSet<DateOnly> FallbackTatillerGetir(int yil)
        {
            return FallbackTatiller
                .Where(s => s.StartsWith($"{yil}-"))
                .Select(DateOnly.Parse)
                .ToHashSet();
        }

        private record NagerHoliday(string Date, string Name);
    }
}
