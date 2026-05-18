using Microsoft.EntityFrameworkCore;
using MroPlan.Data;
using MroPlan.Models;
using MroPlan.Models.Enums;

namespace MroPlan.Services
{
#nullable enable
    public class PersonelService(IDbContextFactory<ApplicationDbContext> factory) : IPersonelService
    {
        public async Task<List<Personel>> GetPersonellerAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.Personeller.AsNoTracking().ToListAsync();
        }

        public async Task<Personel?> GetPersonelByIdAsync(int id)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.Personeller.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> AddPersonelAsync(Personel personel)
        {
            try
            {
                await using var ctx = await factory.CreateDbContextAsync();
                await SenkronizeEtBakimGrubuId(ctx, personel);
                ctx.Personeller.Add(personel);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> UpdatePersonelAsync(Personel personel, string? changeLog = null)
        {
            try
            {
                await using var ctx = await factory.CreateDbContextAsync();
                await SenkronizeEtBakimGrubuId(ctx, personel);
                ctx.Personeller.Update(personel);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> DeletePersonelAsync(int id)
        {
            await using var ctx = await factory.CreateDbContextAsync();
            var p = await ctx.Personeller.FindAsync(id);
            if (p == null) return false;
            ctx.Personeller.Remove(p);
            await ctx.SaveChangesAsync();
            return true;
        }


        public async Task<Dictionary<string, int>> GetPersonelStatsAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            var stats = await ctx.Personeller
                .GroupBy(p => p.Durum)
                .Select(g => new { Durum = g.Key, Sayi = g.Count() })
                .ToDictionaryAsync(x => x.Durum, x => x.Sayi);

            foreach (var status in Enum.GetNames<PersonelDurumu>())
                if (!stats.ContainsKey(status)) stats[status] = 0;

            return stats;
        }

        public async Task<List<string>> GetDistinctUnitsAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.Personeller.AsNoTracking()
                .Where(p => !string.IsNullOrEmpty(p.CalistigiYer))
                .Select(p => p.CalistigiYer!)
                .Distinct().ToListAsync();
        }

        public async Task<List<string>> GetDistinctTitlesAsync()
        {
            await using var ctx = await factory.CreateDbContextAsync();
            return await ctx.Personeller.AsNoTracking()
                .Where(p => !string.IsNullOrEmpty(p.Unvan))
                .Select(p => p.Unvan!)
                .Distinct().ToListAsync();
        }

        private static async Task SenkronizeEtBakimGrubuId(ApplicationDbContext ctx, Personel personel)
        {
            if (!string.IsNullOrEmpty(personel.AtolyeKodu))
            {
                var grup = await ctx.BakimGruplari
                    .FirstOrDefaultAsync(g => g.AtolyeKodu == personel.AtolyeKodu);
                personel.BakimGrubuId = grup?.Id;
            }
            else
            {
                personel.BakimGrubuId = null;
            }
        }
    }
}
