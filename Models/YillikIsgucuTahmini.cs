using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MroPlan.Models
{
    [Table("YillikIsgucuTahminleri")]
    public class YillikIsgucuTahmini
    {
        [Key]
        public int Id { get; set; }
        public int Yil { get; set; }
        public int Ay { get; set; }  // 1-12
        public string HeliTipi { get; set; } = string.Empty;
        public int TahminiIsBakim { get; set; }      // Tahmini bakım iş emri sayısı
        public int TahminiIsGucuIhtiyaci { get; set; } // Tahmini gerekli personel-gün
        public int MevcutKapasite { get; set; }        // O ay aktif personel sayısı
        public double DolulukOrani { get; set; }       // TahminiIsGucuIhtiyaci / MevcutKapasite
        public DateTime OlusturulmaZamani { get; set; } = DateTime.UtcNow;
    }
}
