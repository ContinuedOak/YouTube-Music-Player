namespace OakMusic.Models
{
    public class AppSettings
    {
        public bool DarkMode { get; set; } = true;

        public bool AlwaysOnTop { get; set; } = false;

        public double Volume { get; set; } = 100;

        public double Playtime { get; set; } = 0;
    }
}