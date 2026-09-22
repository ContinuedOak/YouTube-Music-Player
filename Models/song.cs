using System;

namespace OakMusic.Models
{
    public class Song
    {
        public string VideoId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string ChannelName { get; set; } = "";
        public string Album { get; set; } = "";
        public string ThumbnailUrl { get; set; } = "";
        public TimeSpan Duration { get; set; }

        public string DurationText =>
            Duration.TotalHours >= 1
                ? Duration.ToString(@"h\:mm\:ss")
                : Duration.ToString(@"m\:ss");
    }
}