using Microsoft.EntityFrameworkCore;
using MroPlan.Data;
using MroPlan.Models;

namespace MroPlan.Services
{
    public class BakimService(
        IDbContextFactory<ApplicationDbContext> factory,
        ZamanYonetimServisi zamanServisi) : IBakimService
    {
        public async Task<List<BakimPlani>> GetBakimPlanlariAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.BakimPlanlari
                .Include(p => p.KontrolKayitlari)
                .AsNoTracking()
                .OrderByDescending(p => p.KabulTarihi)
                .ToListAsync();
        }

        public async Task<bool> AddBakimPlaniAsync(BakimPlani plan)
        {
            try
            {
                await using var ctx = await factory.CreateDbContextAsync();
                plan.KabulTarihi = EnsureUtc(plan.KabulTarihi);
                ctx.BakimPlanlari.Add(plan);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<List<BakimKontrolKaydi>> GetKontrolKayitlariAsync(int planId)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.BakimKontrolKayitlari
                .Include(k => k.ParcaSablonu).ThenInclude(s => s!.BakimGrubu)
                .Include(k => k.AtananPersonel)
                .Where(k => k.BakimPlaniId == planId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> UpdateKontrolKaydiAsync(BakimKontrolKaydi kayit)
        {
            try
            {
                await using var ctx = await factory.CreateDbContextAsync();
                kayit.IslemTarihi = EnsureUtc(kayit.IslemTarihi);
                ctx.Update(kayit);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateBakimPlaniAsync(BakimPlani plan)
        {
            try
            {
                await using var ctx = await factory.CreateDbContextAsync();
                plan.KabulTarihi = EnsureUtc(plan.KabulTarihi);
                ctx.Update(plan);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> DeleteBakimPlaniAsync(int id)
        {
            try
            {
                await using var ctx = await factory.CreateDbContextAsync();
                var plan = await ctx.BakimPlanlari.FindAsync(id);
                if (plan == null) return false;
                ctx.BakimPlanlari.Remove(plan);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<List<BakimGrubu>> GetBakimGruplariAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.BakimGruplari.AsNoTracking().ToListAsync();
        }

        public async Task<List<ParcaSablonu>> GetParcaSablonlariAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.ParcaSablonlari.Include(s => s.BakimGrubu).AsNoTracking().ToListAsync();
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();

            var aktifBakim = await ctx.BakimPlanlari.CountAsync(p => p.GenelDurum != "Tamamlandi");
            var tamamlanan = await ctx.BakimKontrolKayitlari.CountAsync(k => k.Durum == "Tamamlandi");
            var bekleyen   = await ctx.BakimKontrolKayitlari.CountAsync(k => k.Durum == "Beklemede");

            var bugun = DateTime.UtcNow.Date;
            var trend = new List<(string, int)>();
            for (int i = 6; i >= 0; i--)
            {
                var gun    = bugun.AddDays(-i);
                var sonraki = gun.AddDays(1);
                var sayi   = await ctx.BakimKontrolKayitlari
                    .CountAsync(k => k.IslemTarihi >= gun && k.IslemTarihi < sonraki);
                var ad = gun.DayOfWeek switch
                {
                    DayOfWeek.Monday    => "Pzt",
                    DayOfWeek.Tuesday   => "Sal",
                    DayOfWeek.Wednesday => "Çar",
                    DayOfWeek.Thursday  => "Per",
                    DayOfWeek.Friday    => "Cum",
                    DayOfWeek.Saturday  => "Cmt",
                    _                   => "Paz"
                };
                trend.Add((ad, sayi));
            }

            return new DashboardStats(aktifBakim, tamamlanan, bekleyen, trend);
        }

        public async Task<TeslimTahmini> GetTahminiTeslimAsync(int planId, int aktifPersonelSayisi = 1)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            var plan = await ctx.BakimPlanlari
                .Include(p => p.KontrolKayitlari).ThenInclude(k => k.ParcaSablonu)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null)
                return new TeslimTahmini(null, 0, 0, 0, aktifPersonelSayisi);

            var iscilikDk   = plan.KontrolKayitlari.Sum(k => k.ParcaSablonu?.IslemSure    ?? 60);
            var hazirlikDk  = plan.KontrolKayitlari.Sum(k => k.ParcaSablonu?.HazirlikSuresi ?? 15);
            var toplamDk    = iscilikDk + hazirlikDk;
            var personel    = Math.Max(aktifPersonelSayisi, 1);
            var kisiBasiDk  = (int)Math.Ceiling(toplamDk / (double)personel);

            var tahminiTarih = await zamanServisi.TahminiBitisHesaplaAsync(
                plan.KabulTarihi, kisiBasiDk);

            return new TeslimTahmini(tahminiTarih, iscilikDk, hazirlikDk, toplamDk, personel);
        }

        private static DateTime EnsureUtc(DateTime dt) =>
            dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();
    }
}
