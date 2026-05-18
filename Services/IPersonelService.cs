using MroPlan.Models;
using MroPlan.Models.Enums;

namespace MroPlan.Services
{
#nullable enable
    public interface IPersonelService
    {
        Task<List<Personel>> GetPersonellerAsync();
        Task<Personel?> GetPersonelByIdAsync(int id);
        Task<bool> AddPersonelAsync(Personel personel);
        Task<bool> UpdatePersonelAsync(Personel personel, string? changeLog = null);
        Task<bool> DeletePersonelAsync(int id);

        Task<Dictionary<string, int>> GetPersonelStatsAsync();
        Task<List<string>> GetDistinctUnitsAsync();
        Task<List<string>> GetDistinctTitlesAsync();
    }
}
