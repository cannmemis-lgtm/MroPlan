using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MroPlan.Models
{
    [Table("BakimGruplari")]
    public class BakimGrubu
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string AtolyeKodu { get; set; } = string.Empty; // Kurumsal Kod (216501)
        public string GrupAdi { get; set; } = string.Empty;   // Atölye İsmi
    }
}
