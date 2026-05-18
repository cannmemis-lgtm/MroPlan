using System;
using System.Collections.Generic;

namespace MroPlan.Models
{
    public class YetkinlikHedefi
    {
        public int ParcaSablonuId { get; set; }
        public string ParcaAdi { get; set; } = string.Empty;
        public string ParcaPN { get; set; } = string.Empty;
        public int MevcutUzmanSayisi { get; set; }
        public int HedefUzmanSayisi { get; set; }
    }

    public class GelisimYolHaritasi
    {
        public int PersonelId { get; set; }
        public string PersonelAdSoyad { get; set; } = string.Empty;
        public string AtolyeAdi { get; set; } = string.Empty;
        public List<YolHaritasiAdimi> Adimlar { get; set; } = new();
        public double Skor { get; set; }
        public string AiAciklamasi { get; set; } = string.Empty;
    }

    public class YolHaritasiAdimi
    {
        public string Tur { get; set; } = "Eğitim"; // "Eğitim" veya "Kritik Görev"
        public string Baslik { get; set; } = string.Empty;
        public DateTime TahminiTamamlanma { get; set; }
        public int HedefSeviye { get; set; }
        public int? EgitimModuluId { get; set; }
    }
}
