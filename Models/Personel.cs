using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MroPlan.Models.Enums;

namespace MroPlan.Models
{
#nullable enable
    [Table("Personeller")]
    public class Personel
    {
        [Key]
        public int Id { get; set; }
        public string SicilNo { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string? AtolyeKodu { get; set; }

        // FK — AtolyeKodu string ile senkronize tutulur
        public int? BakimGrubuId { get; set; }
        [ForeignKey("BakimGrubuId")]
        public virtual BakimGrubu? BakimGrubu { get; set; }

        public string? CalistigiYer { get; set; }
        public string? Unvan { get; set; }

        public string Durum { get; set; } = "Aktif";

        [NotMapped]
        public PersonelDurumu DurumEnum
        {
            get => Enum.TryParse<PersonelDurumu>(Durum, out var result) ? result : PersonelDurumu.Aktif;
            set => Durum = value.ToString();
        }

        public DateTime? IzinBaslangic { get; set; }
        public DateTime? IzinBitis { get; set; }

        [NotMapped]
        public string AdSoyad => $"{Ad} {Soyad}";
    }
}
