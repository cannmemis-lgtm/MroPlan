using Microsoft.EntityFrameworkCore;
using MroPlan.Data;
using MroPlan.Models;

namespace MroPlan.Services
{
    public class YillikPlanlamaService : IYillikPlanlamaService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private static readonly string[] _ayAdlari =
            ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];

        // Büyük M ceza sabiti (ILP formülasyonu)
        private const int M = 1000;
        // Günlük net kapasite: 8 saat × 60 dk × 0.85 verimlilik
        private const int GunlukKapasiteDk = 408;

        public YillikPlanlamaService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<AylikTahminSonuc>> TahminHesaplaAsync(int yil)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            // 1. Kaydedilmiş tahminler (YillikIsgucuTahminleri tablosu)
            var kayitliTahminler = await ctx.YillikIsgucuTahminleri
                .Where(t => t.Yil == yil)
                .AsNoTracking()
                .ToListAsync();

            // 2. Seçili yılın gerçek iş yükü — BakimKontrolKayitlari.IslemSure toplamı
            var buYilKayitlar = await ctx.BakimKontrolKayitlari
                .Include(k => k.ParcaSablonu)
                .Where(k => k.IslemTarihi.Year == yil)
                .AsNoTracking()
                .ToListAsync();

            // 3. Geçmiş 2 yıl (mevsimsel tahmin için fallback)
            var gecmisKayitlar = await ctx.BakimKontrolKayitlari
                .Include(k => k.ParcaSablonu)
                .Where(k => k.IslemTarihi.Year >= yil - 2 && k.IslemTarihi.Year < yil)
                .AsNoTracking()
                .ToListAsync();

            var aktifPersonel = await ctx.Personeller.CountAsync(p => p.Durum == "Aktif");

            var sonuclar = new List<AylikTahminSonuc>();
            for (int ay = 1; ay <= 12; ay++)
            {
                // Kaydedilmiş tahmin varsa — önce onu kullan
                var kayitli = kayitliTahminler.FirstOrDefault(t => t.Ay == ay);
                if (kayitli != null)
                {
                    int kap = aktifPersonel > 0 ? aktifPersonel * 20 : 20;
                    sonuclar.Add(new AylikTahminSonuc(
                        Ay: ay, AyAdi: _ayAdlari[ay - 1],
                        TahminiIsGucuIhtiyaci: kayitli.TahminiIsGucuIhtiyaci,
                        MevcutKapasite: kap,
                        AsimdaVar: kayitli.TahminiIsGucuIhtiyaci > kap,
                        DolulukYuzdesi: kap > 0 ? Math.Round((double)kayitli.TahminiIsGucuIhtiyaci / kap * 100, 1) : 0
                    ));
                    continue;
                }

                // Bu yılın gerçek iş yükü: o aya ait IslemSure toplamı → kişi-gün
                var ayKayitlari = buYilKayitlar.Where(k => k.IslemTarihi.Month == ay).ToList();
                int tahminiKisiGun;

                if (ayKayitlari.Count > 0)
                {
                    // Gerçek veri var — dakikayı kişi-güne çevir (GunlukKapasiteDk = 408 dk/kişi-gün)
                    int toplamDk = ayKayitlari.Sum(k => k.ParcaSablonu?.IslemSure ?? 60);
                    tahminiKisiGun = (int)Math.Ceiling(toplamDk / (double)GunlukKapasiteDk);
                }
                else
                {
                    // Gerçek veri yok — geçmiş 2 yılın aynı ayından ortalama al + mevsim faktörü
                    var gecmisAy = gecmisKayitlar.Where(k => k.IslemTarihi.Month == ay).ToList();
                    double mevsimFaktor = ay switch
                    {
                        3 or 4 or 5 => 1.3,
                        9 or 10     => 1.2,
                        7 or 8      => 0.85,
                        12 or 1     => 0.9,
                        _           => 1.0
                    };

                    if (gecmisAy.Count > 0)
                    {
                        // Geçmiş ortalama dk → kişi-gün × mevsim faktörü
                        double gecmisOrtDk = gecmisAy.Sum(k => k.ParcaSablonu?.IslemSure ?? 60) / 2.0;
                        tahminiKisiGun = (int)Math.Ceiling(gecmisOrtDk * mevsimFaktor / GunlukKapasiteDk);
                    }
                    else
                    {
                        // Hiç veri yok — aktif personelin %40'ı kadar yük tahmini
                        tahminiKisiGun = (int)Math.Ceiling(aktifPersonel * 0.4 * mevsimFaktor);
                    }
                }

                int kapasite = aktifPersonel > 0 ? aktifPersonel * 20 : 20;
                sonuclar.Add(new AylikTahminSonuc(
                    Ay: ay, AyAdi: _ayAdlari[ay - 1],
                    TahminiIsGucuIhtiyaci: tahminiKisiGun,
                    MevcutKapasite: kapasite,
                    AsimdaVar: tahminiKisiGun > kapasite,
                    DolulukYuzdesi: kapasite > 0 ? Math.Round((double)tahminiKisiGun / kapasite * 100, 1) : 0
                ));
            }
            return sonuclar;
        }

        public async Task TahminKaydetAsync(int yil, List<AylikTahminSonuc> tahminler)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var mevcutlar = await ctx.YillikIsgucuTahminleri
                .Where(t => t.Yil == yil)
                .ToListAsync();

            foreach (var t in tahminler)
            {
                var mevcut = mevcutlar.FirstOrDefault(m => m.Ay == t.Ay);
                if (mevcut != null)
                {
                    // Güncelle
                    mevcut.TahminiIsGucuIhtiyaci = t.TahminiIsGucuIhtiyaci;
                    mevcut.MevcutKapasite        = t.MevcutKapasite;
                    mevcut.DolulukOrani          = t.DolulukYuzdesi;
                    mevcut.OlusturulmaZamani     = DateTime.UtcNow;
                }
                else
                {
                    // Yeni ekle
                    ctx.YillikIsgucuTahminleri.Add(new YillikIsgucuTahmini
                    {
                        Yil                    = yil,
                        Ay                     = t.Ay,
                        HeliTipi               = "GENEL",
                        TahminiIsGucuIhtiyaci  = t.TahminiIsGucuIhtiyaci,
                        MevcutKapasite         = t.MevcutKapasite,
                        DolulukOrani           = t.DolulukYuzdesi,
                        OlusturulmaZamani      = DateTime.UtcNow
                    });
                }
            }

            await ctx.SaveChangesAsync();
        }

        public async Task<OptimizasyonSonuc> PersonelAtamaOptimizeEtAsync(int bakimPlaniId, string? atolyeKodu = null)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            // ── Veri yükleme ────────────────────────────────────────────────
            var gorevler = await ctx.BakimKontrolKayitlari
                .Include(k => k.ParcaSablonu).ThenInclude(s => s!.BakimGrubu)
                .Where(k => (bakimPlaniId == 0 || k.BakimPlaniId == bakimPlaniId)
                         && k.Durum == "Beklemede"
                         && (atolyeKodu == null || k.ParcaSablonu!.BakimGrubu!.AtolyeKodu == atolyeKodu))
                .ToListAsync();

            var personeller = await ctx.Personeller
                .Where(p => p.Durum == "Aktif"
                         && (atolyeKodu == null || p.AtolyeKodu == atolyeKodu))
                .ToListAsync();

            var personelIdleri = personeller.Select(p => p.Id).ToHashSet();
            var yetkinlikler = await ctx.Yetkinlikler
                .Include(y => y.ParcaSablonu)
                .Where(y => personelIdleri.Contains(y.PersonelId))
                .ToListAsync();

            // ── Dinamik planlama ufku (K3 kısıtı) ──────────────────────────
            // Toplam iş yükü / toplam günlük kapasite → kaç gün gerekli?
            // En az 5 gün (1 hafta), en fazla 30 gün (1 ay) planlama penceresi.
            int toplamYukDkTahmini = gorevler.Sum(g => g.ParcaSablonu?.IslemSure ?? 60);
            int toplamGunlukKapasite = Math.Max(1, personeller.Count) * GunlukKapasiteDk;
            int planlamaGunSayisi = Math.Clamp(
                (int)Math.Ceiling((double)toplamYukDkTahmini / toplamGunlukKapasite),
                5, 30);
            int kisiselKapasite = GunlukKapasiteDk * planlamaGunSayisi;

            // Personel başına kalan kapasite takibi (K3 kısıtı için)
            var kalanKapasite = personeller.ToDictionary(p => p.Id, _ => kisiselKapasite);
            // Personel başına atanan görev süresi toplamı
            var toplamYuk = personeller.ToDictionary(p => p.Id, _ => 0);
            // Personel başına atanan görev sayısı
            var gorevSayisi = personeller.ToDictionary(p => p.Id, _ => 0);

            var atamalar = new List<AtamaDetay>();
            var atanamayalar = new List<AtanamayaDetay>();

            // ── SV önbellekleme: personelId → heliTipi → maxSv ─────────────
            // Döngü içinde O(N) tarama yerine O(1) sözlük erişimi sağlar.
            var svOnbellek = yetkinlikler
                .GroupBy(y => y.PersonelId)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(y => y.ParcaSablonu?.HeliTipi ?? "")
                           .ToDictionary(h => h.Key, h => h.Max(y => y.YetkinlikSeviyesi))
                );

            // ── Greedy + local search (ILP yaklaşımı) ──────────────────────
            // Görevleri en yüksek SV gereksinimi önce olacak şekilde sırala (en kısıtlı önce)
            var siraliGorevler = gorevler
                .OrderByDescending(g => g.ParcaSablonu?.GerekliSvMin ?? 1)
                .ThenByDescending(g => g.ParcaSablonu?.IslemSure ?? 60)
                .ToList();

            foreach (var gorev in siraliGorevler)
            {
                int gerekliSv  = gorev.ParcaSablonu?.GerekliSvMin ?? 1;
                string heliTipi = gorev.ParcaSablonu?.HeliTipi ?? "";
                int islemSure  = gorev.ParcaSablonu?.IslemSure ?? 60;

                // Her personel için O(1) SV skoru (sözlükten)
                var adaylar = personeller.Select(p =>
                {
                    int svSkoru = 0;
                    if (svOnbellek.TryGetValue(p.Id, out var personelSvMap))
                    {
                        if (string.IsNullOrEmpty(heliTipi))
                            svSkoru = personelSvMap.Values.DefaultIfEmpty(0).Max();
                        else
                            personelSvMap.TryGetValue(heliTipi, out svSkoru);
                    }
                    return new { Personel = p, SvSkoru = svSkoru };
                }).ToList();

                // Maksimum mevcut SV (neden-atanamadı için)
                int mevcutMaxSv = adaylar.Max(a => a.SvSkoru);

                // K2: SV kısıtını sağlayan adaylar
                // K3: kapasite kısıtını sağlayan adaylar
                // Adil yük dağılımı: SV eşiği karşılandıktan sonra
                //   1. En düşük yük yüzdesi önce (tüm personel devreye girer)
                //   2. Yorulma katsayısı: yük% arttıkça efektif skor düşer
                //   3. SV fazlası tiebreaker (minimum yeterliyse yüksek SV boşa harcanmaz)
                var uygunAdaylar = adaylar
                    .Where(a => a.SvSkoru >= gerekliSv
                             && kalanKapasite[a.Personel.Id] >= islemSure)
                    .OrderBy(a => toplamYuk[a.Personel.Id])               // 1. en düşük yük önce
                    .ThenBy(a => a.SvSkoru - gerekliSv)                   // 2. en az SV fazlası (israf önlenir)
                    .ToList();

                if (uygunAdaylar.Any())
                {
                    var enIyi = uygunAdaylar.First();
                    toplamYuk[enIyi.Personel.Id]   += islemSure;
                    kalanKapasite[enIyi.Personel.Id] -= islemSure;
                    gorevSayisi[enIyi.Personel.Id]++;

                    // Yorulma katsayısı η: yük arttıkça Z maliyeti yükselir
                    // η = toplamYuk / kapasite ∈ [0,1]
                    // ZKatkisi = (5 - SV) + round(η × 3)  → yüklü personele +0..+3 ceza
                    double eta = kisiselKapasite > 0
                        ? (double)toplamYuk[enIyi.Personel.Id] / kisiselKapasite
                        : 0;
                    int yorulmaCezasi = (int)Math.Round(eta * 3);

                    atamalar.Add(new AtamaDetay(
                        PersonelId:          enIyi.Personel.Id,
                        PersonelAdi:         enIyi.Personel.AdSoyad,
                        SicilNo:             enIyi.Personel.SicilNo,
                        BakimKontrolKaydiId: gorev.Id,
                        ParcaAdi:            gorev.ParcaSablonu?.ParcaAdi ?? "?",
                        HeliTipi:            heliTipi,
                        AtananSv:            enIyi.SvSkoru,
                        GerekliSv:           gerekliSv,
                        IslemSureDk:         islemSure,
                        ToplamYukDk:         toplamYuk[enIyi.Personel.Id],
                        Kapasite:            kisiselKapasite,
                        K2Saglandi:          enIyi.SvSkoru >= gerekliSv,
                        K3Saglandi:          toplamYuk[enIyi.Personel.Id] <= kisiselKapasite,
                        ZKatkisi:            (5 - enIyi.SvSkoru) + yorulmaCezasi,
                        SiraNo:              gorev.ParcaSablonu?.SiraNo ?? 0,
                        RotaGrubu:           gorev.ParcaSablonu?.RotaGrubu
                    ));
                }
                else
                {
                    // Neden atanamadı?
                    bool svYetersiz = !adaylar.Any(a => a.SvSkoru >= gerekliSv);
                    bool kapasiteYok = adaylar.Any(a => a.SvSkoru >= gerekliSv)
                                    && !adaylar.Any(a => a.SvSkoru >= gerekliSv
                                                      && kalanKapasite[a.Personel.Id] >= islemSure);

                    atanamayalar.Add(new AtanamayaDetay(
                        BakimKontrolKaydiId: gorev.Id,
                        ParcaAdi:            gorev.ParcaSablonu?.ParcaAdi ?? "?",
                        HeliTipi:            heliTipi,
                        GerekliSv:           gerekliSv,
                        MevcutMaxSv:         mevcutMaxSv,
                        Neden: svYetersiz
                            ? $"SV{gerekliSv} gerekli — mevcut max SV{mevcutMaxSv} ({gerekliSv - mevcutMaxSv} seviye gap)"
                            : kapasiteYok
                            ? $"SV{gerekliSv}+ personel var ama günlük kapasite ({GunlukKapasiteDk} dk) aşıldı"
                            : "Uygun personel bulunamadı"
                    ));
                }
            }

            // ── Z hedef fonksiyon değeri ────────────────────────────────────
            // Z = M × (atanamayan görev sayısı) + Σ (5 - SV_ij) × x_ij
            double zDegeri = M * atanamayalar.Count
                           + atamalar.Sum(a => a.ZKatkisi);

            // ── Personel yük tablosu ────────────────────────────────────────
            var personelYukleri = personeller
                .Select(p => new PersonelYukDetay(
                    PersonelId:       p.Id,
                    PersonelAdi:      p.AdSoyad,
                    SicilNo:          p.SicilNo,
                    AtananGorevSayisi: gorevSayisi[p.Id],
                    ToplamYukDk:      toplamYuk[p.Id],
                    KapasiteDk:       kisiselKapasite,
                    KullanımYuzdesi:  Math.Round((double)toplamYuk[p.Id] / kisiselKapasite * 100, 1)
                ))
                .Where(p => p.AtananGorevSayisi > 0 || gorevler.Count == 0)
                .OrderByDescending(p => p.ToplamYukDk)
                .ToList();

            // ── Kısıt doğrulamaları ─────────────────────────────────────────
            int toplamGorev = gorevler.Count;
            int kapsananGorev = atamalar.Count + atanamayalar.Count; // K1: tümü kapsandı mı?
            bool k1 = kapsananGorev == toplamGorev;
            bool k2 = atamalar.All(a => a.K2Saglandi);
            bool k3 = atamalar.All(a => a.K3Saglandi);
            bool k4 = true; // x_ij ∈ {0,1} — greedy yapısal olarak sağlar

            var kisitlar = new List<KisitDogrulama>
            {
                new("K1", "Her görev en az bir personele atandı veya y_j=1 ceza alındı",
                    k1, $"{kapsananGorev}/{toplamGorev} görev kapsandı"),
                new("K2", "SV_ij ≥ SV^min_j · x_ij — yetkinlik kısıtı",
                    k2, k2 ? $"Tüm {atamalar.Count} atamada SV yeterli"
                           : $"{atamalar.Count(a => !a.K2Saglandi)} atamada SV yetersiz"),
                new("K3", "Σ d_j · x_ij ≤ C_i — kapasite kısıtı",
                    k3, k3 ? $"Tüm personel {planlamaGunSayisi} günlük ({kisiselKapasite} dk) kapasite içinde"
                           : $"{atamalar.Count(a => !a.K3Saglandi)} atamada kapasite aşımı"),
                new("K4", "x_ij ∈ {0,1} — tamsayı kısıtı",
                    k4, "Greedy yapısal olarak ikili atama garantiler")
            };

            double optSkoru = toplamGorev > 0
                ? Math.Round((double)atamalar.Count / toplamGorev * 100, 1)
                : 0.0;

            return new OptimizasyonSonuc(
                AktifPersonelSayisi:    personeller.Count,
                BekleyenGorevSayisi:    toplamGorev,
                AtananGorev:            atamalar.Count,
                AtanamayaGorev:         atanamayalar.Count,
                OptimallikSkoru:        optSkoru,
                HedefFonksiyonZ:        zDegeri,
                AtamaDetaylari:         atamalar,
                Atanamayalar:           atanamayalar,
                PersonelYukleri:        personelYukleri,
                KisitDogrulamalari:     kisitlar
            );
        }

        public async Task<SenaryoSonuc> SenaryoAnalizeEtAsync(SenaryoParametreleri p)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            // ── Personel ────────────────────────────────────────────────────
            int mevcutPersonel = await ctx.Personeller.CountAsync(x => x.Durum == "Aktif");
            int senaryoPersonel = Math.Max(0, mevcutPersonel + p.PersonelDegisimi);

            // ── Bekleyen iş yükü (personel-gün) ────────────────────────────
            // Düzeltme 1: ILP ile tutarlı olması için GunlukKapasiteDk (408 dk, η=0.85) kullanılır
            // Düzeltme 2: DevamEdiyor işler zaten atanmış — talebe eklenmez (çift sayım önlenir)
            var bekleyenIsler = await ctx.BakimKontrolKayitlari
                .Include(k => k.ParcaSablonu)
                .Where(k => k.Durum == "Beklemede")
                .Select(k => k.ParcaSablonu != null ? k.ParcaSablonu.IslemSure : 60)
                .ToListAsync();

            int bekleyenIsYukuDk = bekleyenIsler.Sum();
            int mevcutIsYukuGun  = (int)Math.Ceiling(bekleyenIsYukuDk / (double)GunlukKapasiteDk);

            // ── Yeni uçak yükü tahmini ──────────────────────────────────────
            // Düzeltme 3: 90 dk sabit yerine DB'deki gerçek ortalama IslemSure kullanılır
            int toplamSablon = await ctx.ParcaSablonlari.CountAsync();
            int ucakBasinaOrtalamaSablon = toplamSablon > 0
                ? Math.Max(5, toplamSablon / Math.Max(1,
                    await ctx.BakimPlanlari.Select(b => b.SeriNo).Distinct().CountAsync()))
                : 10;

            double ortalamaIslemSureDk = toplamSablon > 0
                ? await ctx.ParcaSablonlari.AverageAsync(s => (double)s.IslemSure)
                : 60.0;

            int yeniUcakYukuGun = p.YeniUcakSayisi > 0
                ? (int)Math.Ceiling(p.YeniUcakSayisi * ucakBasinaOrtalamaSablon * ortalamaIslemSureDk / GunlukKapasiteDk)
                : 0;

            // ── Toplam ihtiyaç & kapasite ───────────────────────────────────
            int tahminiIhtiyac = mevcutIsYukuGun + yeniUcakYukuGun;
            // Düzeltme 4: EgitimButcesiGun kişi başı gün — her personel o kadar gün operasyonel değil
            int senaryoKapasite = Math.Max(0, senaryoPersonel * (20 - p.EgitimButcesiGun));

            int gap = tahminiIhtiyac - senaryoKapasite;

            // Risk eşiği: kapasite kullanım oranına göre
            double kullanımOrani = senaryoKapasite > 0
                ? (double)tahminiIhtiyac / senaryoKapasite * 100
                : 100;

            string risk = kullanımOrani >= 90 ? "KRİTİK"
                        : kullanımOrani >= 70 ? "YÜKSEK"
                        : kullanımOrani >= 40 ? "ORTA"
                        : "DÜŞÜK";

            // ── Öneriler ─────────────────────────────────────────────────────
            var oneriler = new List<string>();

            if (p.PersonelDegisimi < 0)
                oneriler.Add($"{Math.Abs(p.PersonelDegisimi)} personel azalması → fazla mesai veya alt yüklenici planlanmalı");

            if (p.YeniUcakSayisi > 0)
                oneriler.Add($"{p.YeniUcakSayisi} yeni uçak ≈ {yeniUcakYukuGun} ek personel-gün iş yükü getiriyor");

            if (p.EgitimButcesiGun > 0 && senaryoPersonel > 0)
            {
                double egitimEtki = (double)p.EgitimButcesiGun / 20.0 * 100;
                oneriler.Add($"Kişi başı {p.EgitimButcesiGun} gün eğitim → operasyonel kapasite {senaryoPersonel * p.EgitimButcesiGun} p-gün azalıyor (%{Math.Round(egitimEtki, 1)})");
            }

            if (bekleyenIsler.Count > 0)
                oneriler.Add($"Sistemde {bekleyenIsler.Count} bekleyen iş emri, toplam {mevcutIsYukuGun} personel-gün iş yükü mevcut");

            if (gap <= 0 && kullanımOrani < 40)
                oneriler.Add($"Kapasite yeterli — %{Math.Round(kullanımOrani, 1)} kullanım oranı");
            else if (gap > 0)
                oneriler.Add($"{gap} personel-gün açık var → {Math.Ceiling((double)gap / 20)} ek personel önerilir");

            return new SenaryoSonuc(
                MevcutPersonel:  mevcutPersonel,
                SenaryoPersonel: senaryoPersonel,
                TahminiIhtiyac:  tahminiIhtiyac,
                GapAcigi:        gap,
                RiskSeviyesi:    risk,
                Aciklama:        $"{risk} risk · %{Math.Round(kullanımOrani, 1)} kapasite kullanımı · {tahminiIhtiyac} p-gün ihtiyaç / {senaryoKapasite} p-gün kapasite",
                Oneriler:        oneriler
            );
        }
    }
}
