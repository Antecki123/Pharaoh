using UnityEngine;
using Zenject;

namespace Controllers.Calendar
{
    public class CalendarController : ITickable
    {
        public int Hour { get; private set; } = 6;
        public int Day { get; private set; } = 1;
        public int Month { get; private set; } = 1;
        public int Year { get; private set; } = 1;

        private float gameSeconds;
        private float timeScale = 72f;

        public void Tick()
        {
            gameSeconds += Time.deltaTime * timeScale;

            if (gameSeconds >= 86400f)
                gameSeconds -= 86400f;

            Hour = (int)(gameSeconds / 3600f);
        }
    }

    public enum MonthName
    {
        Thoth = 1,
        Paopi = 2,
        Hathor = 3,
        Khoiak = 4,
        Tybi = 5,
        Mekhir = 6,
        Phamenoth = 7,
        Pharmuthi = 8,
        Pakhons = 9,
        Payni = 10,
        Epiphi = 11,
        Mesore = 12
    }
}