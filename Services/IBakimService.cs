using MroPlan.Models;

namespace MroPlan.Services
{
    public interface IBakimService
    {
        Task<List<BakimPlani>> GetBakimPlanlariAsync();
        Task<bool> AddBakimPlaniAsync(BakimPlani plan);
        Task<List<BakimKontrolKaydi>> GetKontrolKayitlariAsync(int planId);
        Task<bool> UpdateKontrolKaydiAsync(BakimKontrolKaydi kayit);
        Task<bool> UpdateBakimPlaniAsync(BakimPlani plan);
        Task<bool> DeleteBakimPlaniAsync(int id);
        Task<List<BakimGrubu>> GetBakimGruplariAsync();
        Task<List<ParcaSablonu>> GetParcaSablonlariAsync();
        Task<DashboardStats> GetDashboardStatsAsync();
        Task<TeslimTahmini> GetTahminiTeslimAsync(int planId, int aktifPersonelSayisi = 1);
    }

    public record TeslimTahmini(
        DateTime? TahminiTarih,
        int ToplamIscilikDk,
        int ToplamHazirlikDk,
        int ToplamDk,
        int PersonelSayisi
    );

    public record DashboardStats(
        int AktifBakim,
        int TamamlananGorev,
        int BekleyenIsEmri,
        List<(string Gun, int Sayi)> HaftalikTrend
    );
}
