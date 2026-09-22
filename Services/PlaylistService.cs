using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using OakMusic.Models;

namespace OakMusic
{
    public class PlaylistService
    {
        private readonly string playlistFolder;

        private readonly string playlistIconFolder;

        private readonly JsonSerializerOptions jsonOptions =
            new JsonSerializerOptions
            {
                WriteIndented = true
            };

        public PlaylistService()
        {
            string oakMusicFolder =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData),
                    "OakMusic");

            playlistFolder =
                Path.Combine(
                    oakMusicFolder,
                    "Playlists");

            playlistIconFolder =
                Path.Combine(
                    oakMusicFolder,
                    "PlaylistIcons");

            Directory.CreateDirectory(
                playlistFolder);

            Directory.CreateDirectory(
                playlistIconFolder);
        }

        public List<Playlist> LoadPlaylists()
        {
            List<Playlist> playlists =
                new();

            try
            {
                foreach (string file in
                    Directory.GetFiles(
                        playlistFolder,
                        "*.json"))
                {
                    try
                    {
                        string json =
                            File.ReadAllText(
                                file);

                        Playlist? playlist =
                            JsonSerializer.Deserialize<Playlist>(
                                json,
                                jsonOptions);

                        if (playlist == null)
                            continue;

                        if (string.IsNullOrWhiteSpace(
                                playlist.Id))
                        {
                            playlist.Id =
                                Guid.NewGuid().ToString();
                        }

                        if (string.IsNullOrWhiteSpace(
                                playlist.Name))
                        {
                            continue;
                        }

                        playlist.Songs ??=
                            new List<Song>();

                        if (!string.IsNullOrWhiteSpace(
                                playlist.IconPath) &&
                            !File.Exists(
                                playlist.IconPath))
                        {
                            playlist.IconPath =
                                "";
                        }

                        playlists.Add(
                            playlist);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            return playlists
                .OrderBy(
                    playlist =>
                        playlist.Name,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public void SavePlaylist(
            Playlist playlist)
        {
            if (string.IsNullOrWhiteSpace(
                    playlist.Id))
            {
                playlist.Id =
                    Guid.NewGuid().ToString();
            }

            string fileName =
                SanitizeFileName(
                    playlist.Id) +
                ".json";

            string path =
                Path.Combine(
                    playlistFolder,
                    fileName);

            string json =
                JsonSerializer.Serialize(
                    playlist,
                    jsonOptions);

            File.WriteAllText(
                path,
                json);
        }

        public string CopyPlaylistIcon(
            string sourcePath,
            string playlistId)
        {
            if (string.IsNullOrWhiteSpace(
                    sourcePath) ||
                !File.Exists(
                    sourcePath))
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(
                    playlistId))
            {
                return "";
            }

            try
            {
                string extension =
                    Path.GetExtension(
                        sourcePath);

                if (string.IsNullOrWhiteSpace(
                        extension))
                {
                    extension =
                        ".png";
                }

                string safeId =
                    SanitizeFileName(
                        playlistId);

                string destination =
                    Path.Combine(
                        playlistIconFolder,
                        safeId +
                        extension);

                foreach (string existingFile in
                    Directory.GetFiles(
                        playlistIconFolder,
                        safeId + ".*"))
                {
                    if (!string.Equals(
                            existingFile,
                            destination,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            File.Delete(
                                existingFile);
                        }
                        catch
                        {
                        }
                    }
                }

                File.Copy(
                    sourcePath,
                    destination,
                    true);

                return destination;
            }
            catch
            {
                return "";
            }
        }

        public void DeletePlaylistIcon(
            Playlist playlist)
        {
            if (string.IsNullOrWhiteSpace(
                    playlist.Id))
            {
                return;
            }

            try
            {
                string pattern =
                    SanitizeFileName(
                        playlist.Id) +
                    ".*";

                foreach (string file in
                    Directory.GetFiles(
                        playlistIconFolder,
                        pattern))
                {
                    try
                    {
                        File.Delete(
                            file);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        public void DeletePlaylist(
            Playlist playlist)
        {
            string fileName =
                SanitizeFileName(
                    playlist.Id) +
                ".json";

            string path =
                Path.Combine(
                    playlistFolder,
                    fileName);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            DeletePlaylistIcon(
                playlist);
        }

        private string SanitizeFileName(
            string name)
        {
            foreach (
                char character in
                Path.GetInvalidFileNameChars())
            {
                name =
                    name.Replace(
                        character,
                        '_');
            }

            return name;
        }
    }
}