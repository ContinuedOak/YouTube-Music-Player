using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OakMusic.Models;

namespace OakMusic.Services
{
    public class YouTubeMusicService
    {
        private readonly HttpClient httpClient;

        private const string MusicHost = "https://music.youtube.com";
        private const string ClientName = "WEB_REMIX";
        private const string ClientVersion = "1.20260921.01.00";

        public YouTubeMusicService()
        {
            httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/153.0.0.0 Safari/537.36");

            httpClient.DefaultRequestHeaders.Add(
                "Origin",
                MusicHost);
        }

        public async Task<List<Song>> SearchAsync(string query)
        {
            var results = new List<Song>();

            if (string.IsNullOrWhiteSpace(query))
                return results;

            var requestBody = new
            {
                context = new
                {
                    client = new
                    {
                        clientName = ClientName,
                        clientVersion = ClientVersion,
                        hl = "en",
                        gl = "AU"
                    }
                },
                query = query
            };

            string json = JsonSerializer.Serialize(requestBody);

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            string url =
                $"{MusicHost}/youtubei/v1/search?prettyPrint=false";

            using HttpResponseMessage response =
                await httpClient.PostAsync(url, content);

            response.EnsureSuccessStatusCode();

            string responseJson =
                await response.Content.ReadAsStringAsync();

            using JsonDocument document =
                JsonDocument.Parse(responseJson);

            ExtractSongs(
                document.RootElement,
                results);

            foreach (Song song in results)
            {
                var metadata =
                    await GetVideoMetadataAsync(song.VideoId);

                if (!string.IsNullOrWhiteSpace(metadata.ChannelName))
                {
                    song.ChannelName =
                        metadata.ChannelName;
                }

                if (metadata.Duration != TimeSpan.Zero)
                {
                    song.Duration =
                        metadata.Duration;
                }
            }

            return results;
        }

        private void ExtractSongs(
            JsonElement element,
            List<Song> results)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty(
                    "musicResponsiveListItemRenderer",
                    out JsonElement renderer))
                {
                    Song? song = ParseSong(renderer);

                    if (song != null &&
                        !string.IsNullOrEmpty(song.VideoId))
                    {
                        bool duplicate = false;

                        foreach (Song existing in results)
                        {
                            if (existing.VideoId == song.VideoId)
                            {
                                duplicate = true;
                                break;
                            }
                        }

                        if (!duplicate)
                        {
                            results.Add(song);
                        }
                    }
                }

                foreach (JsonProperty property in element.EnumerateObject())
                {
                    ExtractSongs(
                        property.Value,
                        results);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement child in element.EnumerateArray())
                {
                    ExtractSongs(
                        child,
                        results);
                }
            }
        }

        private Song? ParseSong(JsonElement renderer)
        {
            string videoId = "";

            if (renderer.TryGetProperty(
                "playlistItemData",
                out JsonElement playlistItemData))
            {
                if (playlistItemData.TryGetProperty(
                    "videoId",
                    out JsonElement videoIdElement))
                {
                    videoId = GetTextValue(videoIdElement);
                }
            }

            if (string.IsNullOrEmpty(videoId))
            {
                videoId = FindVideoId(renderer);
            }

            if (string.IsNullOrEmpty(videoId))
                return null;

            string title = "";
            string artist = "";
            string album = "";
            string thumbnail = "";
            TimeSpan duration = TimeSpan.Zero;

            if (renderer.TryGetProperty(
                "flexColumns",
                out JsonElement flexColumns))
            {
                List<string> texts =
                    ExtractTextValues(flexColumns);

                if (texts.Count > 0)
                    title = texts[0];

                if (texts.Count > 1)
                    artist = texts[1];

                if (texts.Count > 2)
                    album = texts[2];
            }

            if (renderer.TryGetProperty(
                "thumbnail",
                out JsonElement thumbnailElement))
            {
                thumbnail =
                    FindThumbnailUrl(thumbnailElement);
            }

            if (renderer.TryGetProperty(
                "fixedColumns",
                out JsonElement fixedColumns))
            {
                string durationText =
                    FindText(fixedColumns);

                duration =
                    ParseDuration(durationText);
            }

            return new Song
            {
                VideoId = videoId,
                Title = title,
                Artist = artist,
                ChannelName = "",
                Album = album,
                ThumbnailUrl = thumbnail,
                Duration = duration
            };
        }

        private string FindChannelName(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty(
                    "navigationEndpoint",
                    out JsonElement navigationEndpoint))
                {
                    if (navigationEndpoint.TryGetProperty(
                        "browseEndpoint",
                        out JsonElement browseEndpoint))
                    {
                        if (browseEndpoint.TryGetProperty(
                            "browseId",
                            out JsonElement browseId))
                        {
                            string id = GetTextValue(browseId);

                            if (id.StartsWith("UC"))
                            {
                                if (element.TryGetProperty(
                                    "text",
                                    out JsonElement text))
                                {
                                    string channel =
                                        GetTextValue(text);

                                    if (!string.IsNullOrWhiteSpace(channel))
                                        return channel;
                                }
                            }
                        }
                    }
                }

                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string result =
                        FindChannelName(property.Value);

                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement child in element.EnumerateArray())
                {
                    string result =
                        FindChannelName(child);

                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
            }

            return "";
        }
        private async Task<(string ChannelName, TimeSpan Duration)> GetVideoMetadataAsync(
    string videoId)
        {
            try
            {
                var requestBody = new
                {
                    context = new
                    {
                        client = new
                        {
                            clientName = ClientName,
                            clientVersion = ClientVersion,
                            hl = "en",
                            gl = "AU"
                        }
                    },
                    videoId = videoId
                };

                string json =
                    JsonSerializer.Serialize(requestBody);

                using var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                string url =
                    $"{MusicHost}/youtubei/v1/player?prettyPrint=false";

                using HttpResponseMessage response =
                    await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                    return ("", TimeSpan.Zero);

                string responseJson =
                    await response.Content.ReadAsStringAsync();

                using JsonDocument document =
                    JsonDocument.Parse(responseJson);

                JsonElement root =
                    document.RootElement;

                string channelName = "";
                TimeSpan duration = TimeSpan.Zero;

                if (root.TryGetProperty(
                    "videoDetails",
                    out JsonElement videoDetails))
                {
                    if (videoDetails.TryGetProperty(
                        "author",
                        out JsonElement author))
                    {
                        channelName =
                            GetTextValue(author);
                    }

                    if (videoDetails.TryGetProperty(
                        "lengthSeconds",
                        out JsonElement lengthSeconds))
                    {
                        string length =
                            GetTextValue(lengthSeconds);

                        if (int.TryParse(
                            length,
                            out int seconds))
                        {
                            duration =
                                TimeSpan.FromSeconds(seconds);
                        }
                    }
                }

                return (channelName, duration);
            }
            catch
            {
                return ("", TimeSpan.Zero);
            }
        }

        private string GetTextValue(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString() ?? "";
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty(
                    "simpleText",
                    out JsonElement simpleText))
                {
                    return GetTextValue(simpleText);
                }

                if (element.TryGetProperty(
                    "text",
                    out JsonElement text))
                {
                    return GetTextValue(text);
                }

                if (element.TryGetProperty(
                    "runs",
                    out JsonElement runs) &&
                    runs.ValueKind == JsonValueKind.Array)
                {
                    string result = "";

                    foreach (JsonElement run in runs.EnumerateArray())
                    {
                        if (run.TryGetProperty(
                            "text",
                            out JsonElement runText))
                        {
                            result += GetTextValue(runText);
                        }
                    }

                    return result;
                }
            }

            return "";
        }

        private string FindVideoId(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty(
                    "watchEndpoint",
                    out JsonElement watchEndpoint))
                {
                    if (watchEndpoint.TryGetProperty(
                        "videoId",
                        out JsonElement videoId))
                    {
                        return GetTextValue(videoId);
                    }
                }

                if (element.TryGetProperty(
                    "watchEndpoint",
                    out JsonElement endpoint))
                {
                    string result =
                        FindVideoId(endpoint);

                    if (!string.IsNullOrEmpty(result))
                        return result;
                }

                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string result =
                        FindVideoId(property.Value);

                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement child in element.EnumerateArray())
                {
                    string result =
                        FindVideoId(child);

                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
            }

            return "";
        }

        private List<string> ExtractTextValues(
            JsonElement element)
        {
            var values = new List<string>();

            ExtractTextValuesRecursive(
                element,
                values);

            return values;
        }

        private void ExtractTextValuesRecursive(
            JsonElement element,
            List<string> values)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty(
                    "runs",
                    out JsonElement runs) &&
                    runs.ValueKind == JsonValueKind.Array)
                {
                    string combined = "";

                    foreach (JsonElement run in runs.EnumerateArray())
                    {
                        if (run.TryGetProperty(
                            "text",
                            out JsonElement runText))
                        {
                            combined +=
                                GetTextValue(runText);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(combined))
                    {
                        values.Add(combined);
                    }
                }
                else if (element.TryGetProperty(
                    "simpleText",
                    out JsonElement simpleText))
                {
                    string text =
                        GetTextValue(simpleText);

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        values.Add(text);
                    }
                }

                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.Name == "runs" ||
                        property.Name == "simpleText")
                    {
                        continue;
                    }

                    ExtractTextValuesRecursive(
                        property.Value,
                        values);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement child in element.EnumerateArray())
                {
                    ExtractTextValuesRecursive(
                        child,
                        values);
                }
            }
        }

        private string FindText(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString() ?? "";
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty(
                    "simpleText",
                    out JsonElement simpleText))
                {
                    return GetTextValue(simpleText);
                }

                if (element.TryGetProperty(
                    "text",
                    out JsonElement text))
                {
                    string result =
                        GetTextValue(text);

                    if (!string.IsNullOrEmpty(result))
                        return result;
                }

                if (element.TryGetProperty(
                    "runs",
                    out JsonElement runs))
                {
                    string result =
                        GetTextValue(runs);

                    if (!string.IsNullOrEmpty(result))
                        return result;
                }

                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string result =
                        FindText(property.Value);

                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement child in element.EnumerateArray())
                {
                    string result =
                        FindText(child);

                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
            }

            return "";
        }

        private string FindThumbnailUrl(
            JsonElement element)
        {
            string bestUrl = "";

            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty(
                    "thumbnails",
                    out JsonElement thumbnails) &&
                    thumbnails.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement thumbnail in thumbnails.EnumerateArray())
                    {
                        if (thumbnail.TryGetProperty(
                            "url",
                            out JsonElement url))
                        {
                            string value =
                                GetTextValue(url);

                            if (!string.IsNullOrEmpty(value))
                                bestUrl = value;
                        }
                    }
                }

                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.Name == "thumbnails")
                        continue;

                    string result =
                        FindThumbnailUrl(property.Value);

                    if (!string.IsNullOrEmpty(result))
                        bestUrl = result;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement child in element.EnumerateArray())
                {
                    string result =
                        FindThumbnailUrl(child);

                    if (!string.IsNullOrEmpty(result))
                        bestUrl = result;
                }
            }

            return bestUrl;
        }

        private TimeSpan ParseDuration(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return TimeSpan.Zero;

            string[] parts =
                text.Split(':');

            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int minutes) &&
                int.TryParse(parts[1], out int seconds))
            {
                return new TimeSpan(
                    0,
                    minutes,
                    seconds);
            }

            if (parts.Length == 3 &&
                int.TryParse(parts[0], out int hours) &&
                int.TryParse(parts[1], out int mins) &&
                int.TryParse(parts[2], out int secs))
            {
                return new TimeSpan(
                    hours,
                    mins,
                    secs);
            }

            return TimeSpan.Zero;
        }
    }
}