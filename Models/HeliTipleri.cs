namespace MroPlan.Models
{
    public static class HeliTipleri
    {
        public const string Cougar    = "AS532 Cougar";
        public const string Chinook   = "CH-47 Chinook";
        public const string Blackhawk = "S-70 Blackhawk";
        public const string Seahawk   = "S-70B Seahawk";
        public const string Atak      = "T-129 ATAK";
        public const string Iroquois  = "UH-1H";

        public static readonly IReadOnlyList<string> Hepsi = new[]
        {
            Cougar, Chinook, Blackhawk, Seahawk, Atak, Iroquois
        };
    }
}
