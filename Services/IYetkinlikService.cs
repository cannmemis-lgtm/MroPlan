using MroPlan.Models;

namespace MroPlan.Services
{
    public interface IYetkinlikService
    {
        Task<List<Yetkinlik>> GetYetkinliklerAsync();
        Task KartTamamlandiAsync(int personelId, int parcaSablonuId, int kartId);
        Task GelistirmeModunaAlAsync(BakimKontrolKaydi kayit, int gelistirmePersonelId);

        Task<List<BakimKontrolKaydi>> GetAktifGelistirmelerAsync();
        Task<List<YetkinlikAcigi>> GetAtolyelYetkinlikAciklariAsync();
        Task<List<BakimGrubu>> GetBakimGruplariAsync();

        // Yetkinlik CRUD + Audit
        Task UpsertAsync(Yetkinlik yetkinlik, string guncelleyenSicil = "sistem");
        Task DeleteAsync(int id, string guncelleyenSicil = "sistem");
        Task<List<YetkinlikGecmisi>> GetYetkinlikGecmisiAsync(string sicilNo);

        // Eğitim Metotları
        Task<List<EgitimModulu>> GetEgitimModulleriAsync();
        Task<List<PersonelEgitim>> GetPersonelEgitimleriAsync(int personelId);
        Task EgitimiTamamlaAsync(int personelId, int egitimId);
        Task<List<EgitimOnerisi>> GetOnerilenEgitimlerAsync(int personelId);

        // GA - Yol Haritası Planlama
        Task<List<GelisimYolHaritasi>> GenerateRoadmapAsync(int atolyeId, List<YetkinlikHedefi> hedefler);

        // Gap-based AI Planning
        Task<List<GelisimYolHaritasi>> GeneratePlanForGapsAsync(List<YetkinlikAcigi> kritikAciklar, string userVoiceCommand = "");
        Task<int> PersonelEgitimlerOnayla(
            List<(int PersonelId, int EgitimModuluId)> atamalar,
            Dictionary<(int, int), DateTime>? tahminiTarihler = null);

        // Modül yönetimi
        Task<EgitimModulu> EnsureModulAsync(int parcaId, int hedefSv);

        // Kapsama & Çizelge
        Task<List<KapsamaAnalizi>> GetKapsamaAnaliziAsync();
        Task<List<EgitimCizelgesiItem>> GetEgitimCizelgesiAsync();
        Task<HashSet<(int PersonelId, int ParcaSablonuId)>> GetAktifEgitimParcalariAsync();
        Task AtamaKaldirAsync(int personelId, int egitimModuluId);
    }

    public record EgitimOnerisi(
        EgitimModulu Modul,
        string Neden,
        int AcikIsSayisi,
        bool Kritik
    );

    public record YetkinlikAcigi(
        string AtolyeAdi,
        string ParcaAdi,
        string ParcaPN,
        int MevcutSv5Sayisi,
        int ToplamPersonel,
        bool Kritik
    );

    public record KapsamaAnalizi(
        string AtolyeAdi,
        string ParcaAdi,
        string ParcaPN,
        string HeliTipi,
        int ToplamPersonel,
        int Sv1Sayisi,
        int Sv2Sayisi,
        int Sv3PlusSayisi,
        int Sv4PlusSayisi,
        bool PipelineVar,      // SV1 veya SV2'de en az 1 kişi var mı
        string RiskSeviyesi    // "ACİL" | "KRİTİK" | "YÜKSEK" | "ORTA" | "İYİ" | "TAMAM"
    );

    public record EgitimCizelgesiItem(
        int PersonelId,
        string PersonelAd,
        string AtolyeAdi,
        string EgitimAdi,
        string ParcaAdi,
        int HedefSv,
        int EgitimModuluId,
        long BasMs,
        long BitMs,
        bool Tamamlandi,
        int IlerlemeYuzdesi,
        string Renk,
        DateTime? TamamlanmaTarihi = null,
        int MevcutSv = 0
    );

    // SV eşik tablosu — sabit kural
    public static class SvEsikler
    {
        private static readonly int[] Esikler = [0, 3, 5, 8, 12];

        // SV1 için sonraki eşik: 3, SV2: 5, SV3: 8, SV4: 12
        public static int SonrakiEsik(int mevcutSv) =>
            mevcutSv >= 5 ? int.MaxValue : Esikler[mevcutSv];

        // Mevcut SV için o seviyeye kadar toplam gereken kart
        public static int KumulatifEsik(int mevcutSv) =>
            mevcutSv <= 1 ? 0 : Esikler[1..(mevcutSv)].Sum();

        public static bool SvYukseltilebilir(int mevcutSv, int tamamlananKart) =>
            mevcutSv < 5 && tamamlananKart >= KumulatifEsik(mevcutSv) + SonrakiEsik(mevcutSv);

        // Eğitim süresi: SV0→1 = 3 gün, diğerleri = 7 gün
        public static int EgitimSuresiGun(int mevcutSv) => mevcutSv == 0 ? 3 : 7;
    }
}
