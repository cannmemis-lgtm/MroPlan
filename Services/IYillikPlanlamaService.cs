using MroPlan.Models;

namespace MroPlan.Services
{
    public interface IYillikPlanlamaService
    {
        Task<List<AylikTahminSonuc>> TahminHesaplaAsync(int yil);
        Task TahminKaydetAsync(int yil, List<AylikTahminSonuc> tahminler);
        Task<OptimizasyonSonuc> PersonelAtamaOptimizeEtAsync(int bakimPlaniId, string? atolyeKodu = null);
        Task<SenaryoSonuc> SenaryoAnalizeEtAsync(SenaryoParametreleri parametreler);
    }

    public record AylikTahminSonuc(
        int Ay,
        string AyAdi,
        int TahminiIsGucuIhtiyaci,
        int MevcutKapasite,
        bool AsimdaVar,
        double DolulukYuzdesi
    );

    // ─── Optimizasyon çıktıları ───────────────────────────────────────────────

    public record OptimizasyonSonuc(
        // Boyutlar (|I|, |J|)
        int AktifPersonelSayisi,
        int BekleyenGorevSayisi,
        // Çözüm özeti
        int AtananGorev,
        int AtanamayaGorev,
        double OptimallikSkoru,
        double HedefFonksiyonZ,         // Z = M·Σy_j + Σ(5-SV)·x_ij
        // Detay listeleri
        List<AtamaDetay> AtamaDetaylari,
        List<AtanamayaDetay> Atanamayalar,
        List<PersonelYukDetay> PersonelYukleri,
        List<KisitDogrulama> KisitDogrulamalari
    )
    {
        // Geriye dönük uyumluluk için
        int ToplamGorev => BekleyenGorevSayisi;
        List<AtamaOneri> Oneriler => AtamaDetaylari
            .Select(d => new AtamaOneri(d.PersonelId, d.PersonelAdi, d.BakimKontrolKaydiId, d.ParcaAdi, d.ZKatkisi))
            .ToList();
    }

    public record AtamaDetay(
        int PersonelId,
        string PersonelAdi,
        string SicilNo,
        int BakimKontrolKaydiId,
        string ParcaAdi,
        string HeliTipi,
        int AtananSv,
        int GerekliSv,
        int IslemSureDk,
        int ToplamYukDk,       // bu personelin toplam yükü
        int Kapasite,          // günlük kapasite dk
        bool K2Saglandi,       // SV kısıtı
        bool K3Saglandi,       // kapasite kısıtı
        int ZKatkisi,          // bu atamanın Z'ye katkısı: 5 - AtananSv
        int SiraNo,            // bakım rotasındaki operasyon sırası
        string? RotaGrubu      // mantıksal grup etiketi (ör. "Motor", "Şanzıman")
    );

    public record AtanamayaDetay(
        int BakimKontrolKaydiId,
        string ParcaAdi,
        string HeliTipi,
        int GerekliSv,
        int MevcutMaxSv,       // DB'deki en yüksek uygun SV
        string Neden           // "Yeterli SV yok" / "Kapasite aşımı"
    );

    public record PersonelYukDetay(
        int PersonelId,
        string PersonelAdi,
        string SicilNo,
        int AtananGorevSayisi,
        int ToplamYukDk,
        int KapasiteDk,
        double KullanımYuzdesi
    );

    public record KisitDogrulama(
        string KisitAdi,       // "K1", "K2", "K3", "K4"
        string Aciklama,
        bool Saglandi,
        string Detay           // örn: "8/8 görev kapsandı"
    );

    // ─── Tek atama önerisi (geriye uyumluluk) ────────────────────────────────
    public record AtamaOneri(
        int PersonelId,
        string PersonelAdi,
        int BakimKontrolKaydiId,
        string ParcaAdi,
        int Skor
    );

    // ─── Senaryo ─────────────────────────────────────────────────────────────
    public record SenaryoParametreleri(
        int PersonelDegisimi,
        int YeniUcakSayisi,
        int EgitimButcesiGun,
        int HedefYil,
        int HedefAy
    );

    public record SenaryoSonuc(
        int MevcutPersonel,
        int SenaryoPersonel,
        int TahminiIhtiyac,
        int GapAcigi,
        string RiskSeviyesi,
        string Aciklama,
        List<string> Oneriler
    );
}
