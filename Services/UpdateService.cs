using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OakMusic.Services
{
    public sealed class UpdateService
    {
        private const string LatestReleaseApi =
            "https://api.github.com/repos/ContinuedOak/YouTube-Music-Player/releases/latest";

        private const string ReleasesPage =
            "https://github.com/ContinuedOak/YouTube-Music-Player/releases/latest";

        private static readonly HttpClient Http = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("OakMusic", "1.0"));

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/vnd.github+json"));

            return client;
        }

        private static bool IsNewerRelease(
            string releaseTag,
            string currentVersion)
        {
            // Supports tags such as beta_v1.2 and v0.1.2.
            string tagVersion = releaseTag;

            if (tagVersion.StartsWith("beta_v", StringComparison.OrdinalIgnoreCase))
                tagVersion = tagVersion["beta_v".Length..];
            else if (tagVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                tagVersion = tagVersion[1..];

            if (!Version.TryParse(tagVersion, out Version? latest))
                return false;

            if (!Version.TryParse(currentVersion, out Version? current))
                return false;

            return latest > current;
        }

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                using HttpResponseMessage response =
                    await Http.GetAsync(LatestReleaseApi);

                if (!response.IsSuccessStatusCode)
                    return;

                await using var stream =
                    await response.Content.ReadAsStreamAsync();

                using JsonDocument document =
                    await JsonDocument.ParseAsync(stream);

                JsonElement root = document.RootElement;

                string latestTag =
                    root.GetProperty("tag_name").GetString() ?? "";

                string releaseName =
                    root.TryGetProperty("name", out JsonElement nameElement)
                        ? nameElement.GetString() ?? latestTag
                        : latestTag;

                string releaseUrl =
                    root.TryGetProperty("html_url", out JsonElement urlElement)
                        ? urlElement.GetString() ?? ReleasesPage
                        : ReleasesPage;

                if (!IsNewerRelease(latestTag, AppInfo.Version))
                    return;

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var result = System.Windows.MessageBox.Show(
                        $"A new version of OakMusic is available!\n\n" +
                        $"Installed version: {AppInfo.Version}\n" +
                        $"Latest version: {releaseName}\n\n" +
                        "Would you like to view the release and download the update?",
                        "OakMusic Update Available",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Information);

                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = releaseUrl,
                            UseShellExecute = true
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    System.Windows.MessageBox.Show(
                        ex.ToString(),
                        "OakMusic Update Check Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                });
            }
        }

        private static bool TryParseVersion(
            string value,
            out Version? version)
        {
            value = value.Trim();

            if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                value = value[1..];

            // Supports standard tags such as 1.2.3 and v1.2.3.
            // Ignores suffixes such as -beta when parsing the numeric version.
            int suffixIndex = value.IndexOfAny(['-', '+']);

            if (suffixIndex >= 0)
                value = value[..suffixIndex];

            return Version.TryParse(value, out version);
        }
    }
}
