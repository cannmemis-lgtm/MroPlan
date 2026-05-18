using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MroPlan.Models
{
#nullable enable
    [Table("ParcaSablonlari")]
    public class ParcaSablonu
    {
        [Key]
        public int Id { get; set; }
        public string HeliTipi { get; set; } = string.Empty;
        public string ParcaAdi { get; set; } = string.Empty;
        public string ParcaPN { get; set; } = string.Empty;
        public string IslemTuru { get; set; } = string.Empty;
        public int IslemSure { get; set; }
        public int HazirlikSuresi { get; set; } = 15;
        public int BakimGrubuId { get; set; }
        [ForeignKey("BakimGrubuId")]
        public virtual BakimGrubu? BakimGrubu { get; set; }
    }
}
