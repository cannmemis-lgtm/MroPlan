using Microsoft.EntityFrameworkCore;
using MroPlan.Data;
using MroPlan.Models;
using MroPlan.Models.Enums;

namespace MroPlan.Services
{
    public class BildirimArkaplanServisi(
        IDbContextFactory<ApplicationDbContext> factory,
        BildirimServisi bildirimServisi,
        ILogger<BildirimArkaplanServisi> logger) : BackgroundService
    {
        private readonly HashSet<int> _oncekiAtananlar = [];
        private readonly HashSet<int> _uyarilanlar = [];
        // Atölye × parça çiftleri için SV5 eksikliği uyarısı verilmiş olanlar
        private readonly HashSet<string> _sv5UyarilanGruplar = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Uygulama başladığında mevcut durumu baseline olarak al
            await BeslemeBaseline(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try { await KontrolEt(stoppingToken); }
                catch (Exception ex) { logger.LogError(ex, "Bildirim arkaplan servisi hatası"); }
            }
        }

        private async Task BeslemeBaseline(CancellationToken ct)
        {
            try
            {
                await using var ctx = await factory.CreateDbContextAsync(ct);
                var atananlar = await ctx.BakimKontrolKayitlari
                    .Where(k => k.AtananPersonelId != null)
                    .Select(k => k.Id)
                    .ToListAsync(ct);
                foreach (var id in atananlar) _oncekiAtananlar.Add(id);
            }
            catch { /* startup hatası kritik değil */ }
        }

        private async Task KontrolEt(CancellationToken ct)
        {
            await using var ctx = await factory.CreateDbContextAsync(ct);

            var kayitlar = await ctx.BakimKontrolKayitlari
                .Include(k => k.AtananPersonel)
                .Include(k => k.ParcaSablonu)
                .AsNoTracking()
                .ToListAsync(ct);

            // 1. Yeni atamalar
            var simdikiAtananIdler = kayitlar
                .Where(k => k.AtananPersonelId != null)
                .Select(k => k.Id)
                .ToHashSet();

            var yeniAtamalar = kayitlar
                .Where(k => k.AtananPersonelId != null && !_oncekiAtananlar.Contains(k.Id))
                .ToList();

            foreach (var k in yeniAtamalar)
            {
                var personelAd = k.AtananPersonel?.AdSoyad ?? "Bilinmiyor";
                var isAd      = k.ParcaSablonu?.ParcaAdi ?? $"İş #{k.Id}";
                bildirimServisi.Ekle(new Bildirim
                {
                    Baslik  = "Yeni Atama",
                    Mesaj   = $"{personelAd} → {isAd}",
                    Turu    = BildirimTuru.Atama,
                    Seviye  = BildirimSeviye.Bilgi
                });
            }

            _oncekiAtananlar.Clear();
            foreach (var id in simdikiAtananIdler) _oncekiAtananlar.Add(id);

            // 2. 48 saat+ bekleyen atanmamış işler
            var uzunBekleyenler = kayitlar
                .Where(k => k.Durum == nameof(BakimDurumu.Beklemede)
                    && k.AtananPersonelId == null
                    && (DateTime.UtcNow - k.IslemTarihi).TotalHours >= 48
                    && !_uyarilanlar.Contains(k.Id))
                .ToList();

            foreach (var k in uzunBekleyenler)
            {
                var saat = (int)(DateTime.UtcNow - k.IslemTarihi).TotalHours;
                bildirimServisi.Ekle(new Bildirim
                {
                    Baslik  = "Uzun Bekleme Uyarısı",
                    Mesaj   = $"{k.ParcaSablonu?.ParcaAdi ?? $"İş #{k.Id}"} — {saat}s bekliyor",
                    Turu    = BildirimTuru.Gecikme,
                    Seviye  = BildirimSeviye.Uyari
                });
                _uyarilanlar.Add(k.Id);
            }

            // 3. Geliştirme modunda 7+ gün geçen kartlar
            var uzunGelistirmeler = kayitlar
                .Where(k => k.GelistirmeModu
                    && k.GelistirmeBaslangic.HasValue
                    && (DateTime.UtcNow - k.GelistirmeBaslangic.Value).TotalDays >= 7
                    && !_uyarilanlar.Contains(-(k.Id)))
                .ToList();

            foreach (var k in uzunGelistirmeler)
            {
                var gun = (int)(DateTime.UtcNow - k.GelistirmeBaslangic!.Value).TotalDays;
                bildirimServisi.Ekle(new Bildirim
                {
                    Baslik = "Geliştirme Süresi Aşıldı",
                    Mesaj  = $"{k.ParcaSablonu?.ParcaAdi ?? $"İş #{k.Id}"} — {gun} gündür geliştirme modunda",
                    Turu   = BildirimTuru.Gecikme,
                    Seviye = BildirimSeviye.Uyari
                });
                _uyarilanlar.Add(-(k.Id)); // negatif ID ile geliştirme uyarıları ayrı tutulur
            }

            // 4. Atölyede hiç SV5 personel kalmayan kritik parçalar
            var yetkinlikler = await ctx.Yetkinlikler.AsNoTracking().ToListAsync(ct);
            var personeller  = await ctx.Personeller.AsNoTracking().ToListAsync(ct);
            var parcalar     = await ctx.ParcaSablonlari.Include(p => p.BakimGrubu).AsNoTracking().ToListAsync(ct);

            foreach (var parca in parcalar)
            {
                var key = $"{parca.BakimGrubuId}:{parca.Id}";
                if (_sv5UyarilanGruplar.Contains(key)) continue;

                var grupPersonelIdler = personeller
                    .Where(p => p.BakimGrubuId == parca.BakimGrubuId && p.Durum == "Aktif")
                    .Select(p => p.Id)
                    .ToHashSet();

                if (grupPersonelIdler.Count == 0) continue;

                var sv5Var = yetkinlikler.Any(y =>
                    grupPersonelIdler.Contains(y.PersonelId) &&
                    y.ParcaSablonuId == parca.Id &&
                    y.YetkinlikSeviyesi >= 5);

                if (!sv5Var)
                {
                    bildirimServisi.Ekle(new Bildirim
                    {
                        Baslik = "Kritik Yetkinlik Açığı",
                        Mesaj  = $"{parca.BakimGrubu?.GrupAdi} atölyesinde {parca.ParcaAdi} için SV5 personel yok",
                        Turu   = BildirimTuru.KapasiteAsimi,
                        Seviye = BildirimSeviye.Kritik
                    });
                    _sv5UyarilanGruplar.Add(key);
                }
            }

            // 5. Kapasite aşımı (aynı atölyede 5+ eş zamanlı iş)
            var kapasiteAsimi = kayitlar
                .Where(k => k.Durum == nameof(BakimDurumu.DevamEdiyor))
                .GroupBy(k => k.ParcaSablonu?.BakimGrubuId ?? 0)
                .Where(g => g.Key != 0 && g.Count() >= 5);

            foreach (var g in kapasiteAsimi)
            {
                var grupAdi = g.First().ParcaSablonu?.BakimGrubu?.GrupAdi ?? $"Grup {g.Key}";
                bildirimServisi.Ekle(new Bildirim
                {
                    Baslik  = "Kapasite Aşımı",
                    Mesaj   = $"{grupAdi} — {g.Count()} iş eş zamanlı devam ediyor",
                    Turu    = BildirimTuru.KapasiteAsimi,
                    Seviye  = BildirimSeviye.Kritik
                });
            }
        }
    }
}
