namespace MroPlan.Models
{
    public enum BildirimTuru { Atama, Gecikme, KapasiteAsimi, SistemBilgisi }
    public enum BildirimSeviye { Bilgi, Uyari, Kritik }

    public class Bildirim
    {
        private static int _counter = 0;

        public int Id { get; } = Interlocked.Increment(ref _counter);
        public string Baslik { get; init; } = "";
        public string Mesaj { get; init; } = "";
        public BildirimTuru Turu { get; init; }
        public BildirimSeviye Seviye { get; init; }
        public DateTime Tarih { get; init; } = DateTime.UtcNow;
        public bool Okundu { get; set; } = false;
    }
}
