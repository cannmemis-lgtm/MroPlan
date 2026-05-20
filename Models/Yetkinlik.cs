using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MroPlan.Models
{
#nullable enable
    [Table("Yetkinlikler")]
    public class Yetkinlik
    {
        [Key]
        public int Id { get; set; }

        // Personel FK
        public int PersonelId { get; set; }
        [ForeignKey("PersonelId")]
        public virtual Personel? Personel { get; set; }

        // ParcaSablonu FK
        public int ParcaSablonuId { get; set; }
        [ForeignKey("ParcaSablonuId")]
        public virtual ParcaSablonu? ParcaSablonu { get; set; }

        // Doğal/görüntü anahtarları — matris mantığı bunlara göre çalışır
        [Required]
        public string SicilNo { get; set; } = string.Empty;
        public string? ParcaPN { get; set; }

        public int YetkinlikSeviyesi { get; set; } = 1;
        public int TamamlananKartSayisi { get; set; } = 0;
        public string? Aciklama { get; set; }

        // Sertifika alanları
        public DateTime? SertifikaTarihi { get; set; }
        public string? SertifikaBelgeNo { get; set; }

        // Audit
        public string? GuncelleyenSicil { get; set; }
        public DateTime GuncellenmeTarihi { get; set; } = DateTime.UtcNow;
    }
}
