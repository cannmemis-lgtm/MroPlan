#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MroPlan.Models
{
    public class EgitimModulu
    {
        [Key]
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Kategori { get; set; } = "Genel";
        public string Aciklama { get; set; } = string.Empty;
        public int HedefYetkinlikSeviyesi { get; set; } = 1; // Bu eğitimi bitirince ulaşabileceği max seviye (veya artış)
        public int? ParcaSablonuId { get; set; } // Hangi parça ile ilgili eğitim?
        [ForeignKey("ParcaSablonuId")]
        public virtual ParcaSablonu? ParcaSablonu { get; set; }

        public string Ikon { get; set; } = "School";
        public string CyberColor { get; set; } = "#FACC15"; // Neon Yellow
    }

    public class PersonelEgitim
    {
        [Key]
        public int Id { get; set; }
        public int PersonelId { get; set; }
        public int EgitimModuluId { get; set; }
        public DateTime TamamlanmaTarihi { get; set; } = DateTime.UtcNow;
        public bool Tamamlandi { get; set; } = false;
        public int IlerlemeYuzdesi { get; set; } = 0;
        public DateTime? PlanlananBaslangic { get; set; }
        public DateTime? PlanlananBitis     { get; set; }

        [ForeignKey("PersonelId")]
        public virtual Personel? Personel { get; set; }
        [ForeignKey("EgitimModuluId")]
        public virtual EgitimModulu? EgitimModulu { get; set; }
    }
}
