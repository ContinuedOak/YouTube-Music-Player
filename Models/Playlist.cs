using System.Collections.Generic;

namespace OakMusic.Models
{
    public class Playlist
    {
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";

        public string IconPath { get; set; } = "";

        public List<Song> Songs { get; set; } = new();
    }
}