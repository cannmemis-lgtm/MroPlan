using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MroPlan.Models.Enums;

namespace MroPlan.Models
{
    [Table("BakimPlani")]
    public class BakimPlani
    {
        [Key]
        public int Id { get; set; }
        public string SeriNo { get; set; } = string.Empty;
        public string HeliTipi { get; set; } = string.Empty;
        public string GeldigiBirlik { get; set; } = string.Empty;
        public DateTime KabulTarihi { get; set; } = DateTime.UtcNow;
        public DateTime? TeslimTarihi { get; set; }
        public DateTime? HedefTeslimTarihi { get; set; }

        // --- TITANIUM FIX: Veritabanı ile doğrudan uyum için string ---
        public string GenelDurum { get; set; } = "Beklemede";

        [NotMapped]
        public BakimDurumu GenelDurumEnum
        {
            get => Enum.TryParse<BakimDurumu>(GenelDurum, out var result) ? result : BakimDurumu.Beklemede;
            set => GenelDurum = value.ToString();
        }

        public virtual ICollection<BakimKontrolKaydi> KontrolKayitlari { get; set; } = [];
    }
}
