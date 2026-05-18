using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MroPlan.Models
{
#nullable enable
    [Table("YetkinlikGecmisi")]
    public class YetkinlikGecmisi
    {
        [Key]
        public int Id { get; set; }

        public int YetkinlikId { get; set; }
        [ForeignKey("YetkinlikId")]
        public virtual Yetkinlik? Yetkinlik { get; set; }

        [Required]
        public string SicilNo { get; set; } = string.Empty;

        [Required]
        public string ParcaPN { get; set; } = string.Empty;

        public int EskiSeviye { get; set; }
        public int YeniSeviye { get; set; }

        public string IslemYapanSicil { get; set; } = string.Empty;
        public DateTime IslemTarihi { get; set; } = DateTime.UtcNow;
        public string? IslemNotu { get; set; }
    }
}
