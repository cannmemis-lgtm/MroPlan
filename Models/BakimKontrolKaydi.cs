using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MroPlan.Models.Enums;

namespace MroPlan.Models
{
#nullable enable
    [Table("BakimKontrolKayitlari")]
    public class BakimKontrolKaydi
    {
        [Key]
        public int Id { get; set; }

        public int BakimPlaniId { get; set; }
        [ForeignKey("BakimPlaniId")]
        public virtual BakimPlani? BakimPlani { get; set; }

        public int ParcaSablonuId { get; set; }
        [ForeignKey("ParcaSablonuId")]
        public virtual ParcaSablonu? ParcaSablonu { get; set; }

        public int? AtananPersonelId { get; set; }
        [ForeignKey("AtananPersonelId")]
        public virtual Personel? AtananPersonel { get; set; }

        // Seviye 1-2 personele atanan işlerde gözetimci
        public int? SupervisorPersonelId { get; set; }
        [ForeignKey("SupervisorPersonelId")]
        public virtual Personel? SupervisorPersonel { get; set; }

        // --- TITANIUM FIX: Veritabanı ile doğrudan uyum için string ---
        public string Durum { get; set; } = "Beklemede";

        [NotMapped]
        public BakimDurumu DurumEnum
        {
            get => Enum.TryParse<BakimDurumu>(Durum, out var result) ? result : BakimDurumu.Beklemede;
            set => Durum = value.ToString();
        }

        // Atama tarihi — IsBitir() tarafından ezilmez
        public DateTime IslemTarihi { get; set; } = DateTime.UtcNow;

        // Gerçek bitiş tarihi — performans hesabı için ayrı tutulur
        public DateTime? TamamlanmaTarihi { get; set; }

        // Eğitim modunda yapılan atamalar performans hesabına dahil edilmez
        public bool EgitimModu { get; set; } = false;

        // Geliştirme modu — SV yetersiz personel bu kartla deneyim kazanır
        public bool GelistirmeModu { get; set; } = false;
        public int? GelistirmePersonelId { get; set; }
        [ForeignKey("GelistirmePersonelId")]
        public virtual Personel? GelistirmePersonel { get; set; }
        public DateTime? GelistirmeBaslangic { get; set; }

        public string TeknisyenNotu { get; set; } = string.Empty;

        // Gerçek çalışma süresi (dakika) — supervisor onayında girilir, performans katsayısı için
        public int? GercekSureDk { get; set; }

    }
}
