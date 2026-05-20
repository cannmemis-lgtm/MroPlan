namespace MroPlan.Models.Enums
{
    public enum PersonelDurumu
    {
        Aktif,
        Izinli,
        Egitimde,
        Gorevli,
        Ayrildi
    }

    public enum BakimDurumu
    {
        Beklemede,
        DevamEdiyor,
        Durduruldu,
        Tamamlandi,
        IptalEdildi
    }

    public enum BakimSeviyesi
    {
        Operasyonel,   // O-Level: uçuş hattı bakımı
        Orta,          // I-Level: orta kademe bakım
        Depo           // D-Level: depo seviyesi onarım
    }
}
