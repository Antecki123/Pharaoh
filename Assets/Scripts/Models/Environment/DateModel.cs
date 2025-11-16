namespace Models.Environment
{
    public class DateModel
    {
        public int CurrentMonth { get; set; } = 1;
        public int CurrentYear { get; set; } = 1;
    }

    public enum MonthName
    {
        Phamenoth = 1,
        Pharmuthi = 2,
        Pakhons = 3,
        Payni = 4,
        Epiphi = 5,
        Mesore = 6,
        Thoth = 7,
        Paopi = 8,
        Hathor = 9,
        Khoiak = 10,
        Tybi = 11,
        Mekhir = 12,

        Achet = Thoth | Paopi | Hathor | Khoiak
    }
}