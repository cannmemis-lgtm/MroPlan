using MroPlan.Models;

namespace MroPlan.Services
{
    public class BildirimServisi
    {
        private readonly List<Bildirim> _liste = [];
        private readonly object _kilit = new();

        // Her yeni bildirimde tüm abone bileşenler tetiklenir
        public event Action<Bildirim>? YeniBildirim;

        public IReadOnlyList<Bildirim> Tumü
        {
            get { lock (_kilit) return _liste.AsReadOnly(); }
        }

        public int OkunmamisSayisi
        {
            get { lock (_kilit) return _liste.Count(b => !b.Okundu); }
        }

        public void Ekle(Bildirim bildirim)
        {
            lock (_kilit)
            {
                _liste.Insert(0, bildirim);
                // Son 100 bildirimi tut
                if (_liste.Count > 100) _liste.RemoveAt(_liste.Count - 1);
            }
            YeniBildirim?.Invoke(bildirim);
        }

        public void TumunuOkunduIsaretle()
        {
            lock (_kilit)
                foreach (var b in _liste) b.Okundu = true;
        }

        public void Temizle()
        {
            lock (_kilit) _liste.Clear();
        }
    }
}
