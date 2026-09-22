using Microsoft.Web.WebView2.Core;
using OakMusic.Models;
using OakMusic.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace OakMusic
{
    public partial class MainWindow : Window
    {
        private enum AppView
        {
            Player,
            Search,
            Playlists
        }

        private readonly YouTubeMusicService musicService;

        private List<Song> searchResults =
            new();

        private List<Song> playbackQueue =
            new();

        private Song? currentSong;

        private int currentQueueIndex =
            -1;

        private bool playerReady;

        private bool isPlaying;

        private bool isSearching;

        private bool isSeeking;

        private readonly DispatcherTimer playbackTimer;

        private readonly PlaylistService playlistService;

        private List<Playlist> playlists =
            new();

        private Playlist? currentPlaylist;

        public MainWindow()
        {
            InitializeComponent();

            playbackTimer =
                new DispatcherTimer
                {
                    Interval =
                        TimeSpan.FromMilliseconds(
                            250)
                };

            playbackTimer.Tick +=
                PlaybackTimer_Tick;

            playbackTimer.Start();

            musicService =
                new YouTubeMusicService();

            PlayerView.PlayClicked +=
                PlayButton_Click;

            PlayerView.PreviousClicked +=
                PreviousButton_Click;

            PlayerView.NextClicked +=
                NextButton_Click;

            PlayerView.VolumeChanged +=
                VolumeSlider_ValueChanged;

            PlayerView.ProgressChanged +=
                ProgressSlider_ValueChanged;

            playlistService =
                new PlaylistService();

            playlists =
                playlistService.LoadPlaylists();

            PlaylistView.CreatePlaylistClicked +=
                CreatePlaylist_Click;

            PlaylistView.PlaylistClicked +=
                Playlist_Click;

            PlaylistView.EditPlaylistClicked +=
                EditPlaylist_Click;

            PlaylistView.SongClicked +=
                PlaylistSong_Click;

            PlaylistView.BackClicked +=
                PlaylistBack_Click;

            PlaylistView.DeleteSongClicked +=
                DeletePlaylistSong_Click;

            PlaylistView.DeletePlaylistClicked +=
                DeletePlaylist_Click;

            PlaylistView.SetPlaylists(
                playlists);

            InitializePlayerAsync();
        }

        private async void PlaybackTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (!playerReady ||
                currentSong == null ||
                !isPlaying ||
                isSeeking)
            {
                return;
            }

            try
            {
                string result =
                    await YouTubePlayer
                        .CoreWebView2
                        .ExecuteScriptAsync(
                            "getCurrentTime();");

                if (double.TryParse(
                        result.Trim('"'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out double seconds))
                {
                    PlayerView.SetProgress(
                        seconds);

                    PlayerView.SetCurrentTime(
                        TimeSpan.FromSeconds(
                            seconds));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Playback time error: {ex}");
            }
        }

        private async void PlaylistSong_Click(
            object? sender,
            Song song)
        {
            if (currentPlaylist == null)
                return;

            playbackQueue =
                new List<Song>(
                    currentPlaylist.Songs);

            int queueIndex =
                playbackQueue.FindIndex(
                    existing =>
                        existing.VideoId ==
                        song.VideoId);

            if (queueIndex < 0)
                return;

            await PlayQueueSongAsync(
                queueIndex);
        }

        private async void InitializePlayerAsync()
        {
            try
            {
                YouTubePlayer
                    .CoreWebView2InitializationCompleted +=
                    YouTubePlayer_CoreWebView2InitializationCompleted;

                await YouTubePlayer
                    .EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"WebView2 initialization error: {ex}");
            }
        }

        private void Playlist_Click(
            object? sender,
            Playlist playlist)
        {
            currentPlaylist =
                playlist;

            PlaylistView.ShowPlaylist(
                playlist);

            ShowView(
                AppView.Playlists);
        }

        private void CreatePlaylist_Click(
            object? sender,
            EventArgs e)
        {
            PlaylistNameWindow window =
                new PlaylistNameWindow();

            window.Owner =
                this;

            bool? result =
                window.ShowDialog();

            if (result != true)
            {
                return;
            }

            Playlist playlist =
                new Playlist
                {
                    Id =
                        Guid.NewGuid().ToString(),

                    Name =
                        window.PlaylistName
                };

            if (!string.IsNullOrWhiteSpace(
                    window.IconPath))
            {
                playlist.IconPath =
                    playlistService.CopyPlaylistIcon(
                        window.IconPath,
                        playlist.Id);
            }

            playlists.Add(
                playlist);

            playlistService.SavePlaylist(
                playlist);

            PlaylistView.SetPlaylists(
                playlists);
        }

        private void EditPlaylist_Click(
            object? sender,
            Playlist playlist)
        {
            PlaylistNameWindow window =
                new PlaylistNameWindow(
                    playlist);

            window.Owner =
                this;

            bool? result =
                window.ShowDialog();

            if (result != true)
            {
                return;
            }

            playlist.Name =
                window.PlaylistName;

            if (window.IconChanged)
            {
                if (string.IsNullOrWhiteSpace(
                        window.IconPath))
                {
                    playlistService.DeletePlaylistIcon(
                        playlist);

                    playlist.IconPath =
                        "";
                }
                else
                {
                    string newIconPath =
                        playlistService.CopyPlaylistIcon(
                            window.IconPath,
                            playlist.Id);

                    if (!string.IsNullOrWhiteSpace(
                            newIconPath))
                    {
                        playlist.IconPath =
                            newIconPath;
                    }
                }
            }

            playlistService.SavePlaylist(
                playlist);

            PlaylistView.SetPlaylists(
                playlists);

            if (currentPlaylist == playlist)
            {
                PlaylistView.ShowPlaylist(
                    playlist);
            }
        }

        private async void YouTubePlayer_CoreWebView2InitializationCompleted(
            object? sender,
            CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"WebView2 initialization failed: {e.InitializationException}");

                return;
            }

            try
            {
                string playerFolder =
                    System.IO.Path.Combine(
                        AppContext.BaseDirectory,
                        "Player");

                YouTubePlayer
                    .CoreWebView2
                    .SetVirtualHostNameToFolderMapping(
                        "oakmusic.local",
                        playerFolder,
                        CoreWebView2HostResourceAccessKind.Allow);

                YouTubePlayer
                    .CoreWebView2
                    .WebResourceRequested +=
                    CoreWebView2_WebResourceRequested;

                YouTubePlayer
                    .CoreWebView2
                    .WebMessageReceived +=
                    CoreWebView2_WebMessageReceived;

                YouTubePlayer
                    .CoreWebView2
                    .NavigationCompleted +=
                    CoreWebView2_NavigationCompleted;

                YouTubePlayer
                    .CoreWebView2
                    .ProcessFailed +=
                    CoreWebView2_ProcessFailed;

                YouTubePlayer
                    .CoreWebView2
                    .AddWebResourceRequestedFilter(
                        "*",
                        CoreWebView2WebResourceContext.All);

                YouTubePlayer
                    .CoreWebView2
                    .Navigate(
                        "https://oakmusic.local/player.html");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Player setup error: {ex}");
            }
        }

        private void CoreWebView2_WebResourceRequested(
            object? sender,
            CoreWebView2WebResourceRequestedEventArgs e)
        {
            try
            {
                e.Request.Headers.SetHeader(
                    "Referer",
                    "https://oakmusic.local/");
            }
            catch
            {
            }
        }

        private void CoreWebView2_NavigationCompleted(
            object? sender,
            CoreWebView2NavigationCompletedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                $"YouTube player navigation completed. Success: {e.IsSuccess}, Error: {e.WebErrorStatus}");
        }

        private void CoreWebView2_ProcessFailed(
            object? sender,
            CoreWebView2ProcessFailedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                $"WebView2 process failed: {e.ProcessFailedKind}");
        }

        private void CoreWebView2_WebMessageReceived(
            object? sender,
            CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message =
                    e.TryGetWebMessageAsString();

                System.Diagnostics.Debug.WriteLine(
                    $"YouTube message: {message}");

                try
                {
                    using JsonDocument document =
                        JsonDocument.Parse(
                            message);

                    JsonElement root =
                        document.RootElement;

                    if (!root.TryGetProperty(
                            "type",
                            out JsonElement typeElement))
                    {
                        return;
                    }

                    string? type =
                        typeElement.GetString();

                    if (type == "ready")
                    {
                        bool ready =
                            root.TryGetProperty(
                                "value",
                                out JsonElement valueElement)
                            && valueElement.ValueKind ==
                                JsonValueKind.True;

                        if (!ready)
                        {
                            return;
                        }

                        playerReady =
                            true;

                        System.Diagnostics.Debug.WriteLine(
                            "YouTube player ready.");

                        if (currentSong != null)
                        {
                            Song song =
                                currentSong;

                            Dispatcher.InvokeAsync(
                                async () =>
                                {
                                    try
                                    {
                                        await LoadSongIntoPlayerAsync(
                                            song);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine(
                                            $"Initial song load error: {ex}");
                                    }
                                });
                        }

                        return;
                    }

                    if (type == "state")
                    {
                        if (root.TryGetProperty(
                                "value",
                                out JsonElement valueElement)
                            && valueElement.TryGetInt32(
                                out int state))
                        {
                            Dispatcher.InvokeAsync(
                                () =>
                                    HandlePlayerState(
                                        state));
                        }

                        return;
                    }

                    if (type == "error")
                    {
                        string error =
                            root.TryGetProperty(
                                "value",
                                out JsonElement errorElement)
                                ? errorElement.ToString()
                                : "unknown";

                        System.Diagnostics.Debug.WriteLine(
                            $"YouTube player error code: {error}");

                        return;
                    }

                    if (type == "autoplay-blocked")
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "YouTube autoplay was blocked.");

                        return;
                    }
                }
                catch (JsonException)
                {
                    if (message == "ready")
                    {
                        playerReady =
                            true;

                        System.Diagnostics.Debug.WriteLine(
                            "YouTube player ready.");

                        if (currentSong != null)
                        {
                            Song song =
                                currentSong;

                            Dispatcher.InvokeAsync(
                                async () =>
                                {
                                    try
                                    {
                                        await LoadSongIntoPlayerAsync(
                                            song);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine(
                                            $"Initial song load error: {ex}");
                                    }
                                });
                        }

                        return;
                    }

                    if (message ==
                        "autoplay-blocked")
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "YouTube autoplay was blocked.");

                        return;
                    }

                    if (message.StartsWith(
                            "state:"))
                    {
                        string value =
                            message.Substring(
                                "state:".Length);

                        if (int.TryParse(
                                value,
                                out int state))
                        {
                            Dispatcher.InvokeAsync(
                                () =>
                                    HandlePlayerState(
                                        state));
                        }

                        return;
                    }

                    if (message.StartsWith(
                            "error:"))
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"YouTube player error: {message}");

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Web message error: {ex}");
            }
        }

        private async Task ExecutePlayerCommandAsync(
            string command)
        {
            if (!playerReady)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Player command skipped because player is not ready: {command}");

                return;
            }

            try
            {
                await YouTubePlayer
                    .CoreWebView2
                    .ExecuteScriptAsync(
                        command);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Player command error: {ex}");
            }
        }

        private async Task LoadSongIntoPlayerAsync(
            Song song)
        {
            if (!playerReady)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Cannot load song: YouTube player is not ready.");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    song.VideoId))
            {
                System.Diagnostics.Debug.WriteLine(
                    "Cannot load song: VideoId is empty.");

                return;
            }

            try
            {
                string videoId =
                    JsonSerializer.Serialize(
                        song.VideoId);

                System.Diagnostics.Debug.WriteLine(
                    $"Loading YouTube video: {song.VideoId}");

                await ExecutePlayerCommandAsync(
                    $"loadAndPlayVideo({videoId});");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"YouTube playback error: {ex}");
            }
        }

        private async void HandlePlayerState(
            int state)
        {
            switch (state)
            {
                case 0:
                    isPlaying =
                        false;

                    PlayerView.SetPlaying(
                        false);

                    if (currentSong != null)
                    {
                        PlayerView.SetProgress(
                            currentSong.Duration.TotalSeconds);

                        PlayerView.SetCurrentTime(
                            currentSong.Duration);
                    }

                    await PlayNextSongAsync();

                    break;

                case 1:
                    isPlaying =
                        true;

                    PlayerView.SetPlaying(
                        true);

                    break;

                case 2:
                    isPlaying =
                        false;

                    PlayerView.SetPlaying(
                        false);

                    break;

                case 3:
                    break;

                case 5:
                    break;
            }
        }

        private async Task PlayQueueSongAsync(
            int index)
        {
            if (index < 0 ||
                index >= playbackQueue.Count)
            {
                return;
            }

            currentQueueIndex =
                index;

            currentSong =
                playbackQueue[index];

            Song song =
                currentSong;

            ShowView(
                AppView.Player);

            PlayerView.SetSong(
                song);

            PlayerView.SetPlaying(
                false);

            isPlaying =
                false;

            if (!playerReady)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Song selected but YouTube player is not ready yet.");

                return;
            }

            await LoadSongIntoPlayerAsync(
                song);
        }

        private async Task PlayNextSongAsync()
        {
            if (playbackQueue.Count == 0)
            {
                return;
            }

            int nextIndex =
                currentQueueIndex + 1;

            if (nextIndex >=
                playbackQueue.Count)
            {
                return;
            }

            await PlayQueueSongAsync(
                nextIndex);
        }

        private async Task PlayPreviousSongAsync()
        {
            if (playbackQueue.Count == 0 ||
                currentQueueIndex < 0)
            {
                return;
            }

            if (currentQueueIndex == 0)
            {
                await RestartCurrentSongAsync();

                return;
            }

            int previousIndex =
                currentQueueIndex - 1;

            await PlayQueueSongAsync(
                previousIndex);
        }

        private async Task RestartCurrentSongAsync()
        {
            if (currentSong == null ||
                !playerReady)
            {
                return;
            }

            try
            {
                await ExecutePlayerCommandAsync(
                    "seekVideo(0, true);");

                await ExecutePlayerCommandAsync(
                    "playVideo();");

                PlayerView.SetProgress(
                    0);

                PlayerView.SetCurrentTime(
                    TimeSpan.Zero);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Restart error: {ex}");
            }
        }

        private async void PlayButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (currentSong == null ||
                !playerReady)
            {
                return;
            }

            try
            {
                if (isPlaying)
                {
                    await ExecutePlayerCommandAsync(
                        "pauseVideo();");
                }
                else
                {
                    await ExecutePlayerCommandAsync(
                        "playVideo();");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Playback command error: {ex}");
            }
        }

        private async void PreviousButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await PlayPreviousSongAsync();
        }

        private async void NextButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await PlayNextSongAsync();
        }

        private async void VolumeSlider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (!playerReady)
            {
                return;
            }

            try
            {
                string volume =
                    ((int)e.NewValue)
                    .ToString();

                await ExecutePlayerCommandAsync(
                    $"setVolume({volume});");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Volume error: {ex}");
            }
        }

        private async void ProgressSlider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (!playerReady ||
                currentSong == null ||
                isSearching ||
                isSeeking)
            {
                return;
            }

            try
            {
                await ExecutePlayerCommandAsync(
                    $"seekVideo({e.NewValue}, true);");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Seek error: {ex}");
            }
        }

        private async void SearchButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await PerformSearchAsync();
        }

        private async void SearchTextBox_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            e.Handled =
                true;

            await PerformSearchAsync();
        }

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (SearchPlaceholder == null)
            {
                return;
            }

            SearchPlaceholder.Visibility =
                string.IsNullOrWhiteSpace(
                    SearchTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private async Task PerformSearchAsync()
        {
            string query =
                SearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                    query))
            {
                return;
            }

            try
            {
                isSearching =
                    true;

                SearchPlaceholder.Visibility =
                    Visibility.Collapsed;

                ShowView(
                    AppView.Search);

                searchResults =
                    await musicService.SearchAsync(
                        query);

                SearchView.SetResults(
                    searchResults,
                    ResultButton_Click,
                    AddSongButton_Click);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Search error: {ex}");
            }
            finally
            {
                isSearching =
                    false;
            }
        }

        private void AddSongButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            if (button.Tag is not Song song)
            {
                return;
            }

            AddSongToPlaylist(
                song);
        }

        private void AddSongToPlaylist(
            Song song)
        {
            AddToPlaylistWindow window =
                new AddToPlaylistWindow(
                    playlists);

            window.Owner =
                this;

            bool? result =
                window.ShowDialog();

            if (result != true)
            {
                return;
            }

            if (window.CreateNewPlaylistRequested)
            {
                CreatePlaylistAndAddSong(
                    song);

                return;
            }

            if (window.SelectedPlaylist != null)
            {
                AddSongToExistingPlaylist(
                    song,
                    window.SelectedPlaylist);
            }
        }

        private void AddSongToExistingPlaylist(
            Song song,
            Playlist playlist)
        {
            bool alreadyExists =
                playlist.Songs.Exists(
                    existing =>
                        existing.VideoId ==
                        song.VideoId);

            if (alreadyExists)
            {
                MessageBox.Show(
                    $"\"{song.Title}\" is already in \"{playlist.Name}\".",
                    "OakMusic",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            playlist.Songs.Add(
                song);

            playlistService.SavePlaylist(
                playlist);

            PlaylistView.SetPlaylists(
                playlists);

            if (currentPlaylist == playlist)
            {
                PlaylistView.ShowPlaylist(
                    playlist);
            }
        }

        private void CreatePlaylistAndAddSong(
            Song song)
        {
            PlaylistNameWindow window =
                new PlaylistNameWindow();

            window.Owner =
                this;

            bool? result =
                window.ShowDialog();

            if (result != true)
            {
                return;
            }

            Playlist playlist =
                new Playlist
                {
                    Id =
                        Guid.NewGuid().ToString(),

                    Name =
                        window.PlaylistName
                };

            if (!string.IsNullOrWhiteSpace(
                    window.IconPath))
            {
                playlist.IconPath =
                    playlistService.CopyPlaylistIcon(
                        window.IconPath,
                        playlist.Id);
            }

            playlist.Songs.Add(
                song);

            playlists.Add(
                playlist);

            playlistService.SavePlaylist(
                playlist);

            PlaylistView.SetPlaylists(
                playlists);
        }

        private async void ResultButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            if (button.Tag is not Song song)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine(
                $"Selected song: {song.Title} | VideoId: {song.VideoId}");

            int queueIndex =
                playbackQueue.FindIndex(
                    queuedSong =>
                        queuedSong.VideoId ==
                        song.VideoId);

            if (queueIndex < 0)
            {
                playbackQueue =
                    new List<Song>(
                        searchResults);

                queueIndex =
                    playbackQueue.FindIndex(
                        queuedSong =>
                            queuedSong.VideoId ==
                            song.VideoId);
            }

            if (queueIndex < 0)
            {
                return;
            }

            ShowView(
                AppView.Player);

            await PlayQueueSongAsync(
                queueIndex);
        }

        private void PlaylistBack_Click(
            object? sender,
            EventArgs e)
        {
            currentPlaylist =
                null;

            PlaylistView.SetPlaylists(
                playlists);

            ShowView(
                AppView.Playlists);
        }

        private void DeletePlaylistSong_Click(
            object? sender,
            Song song)
        {
            if (currentPlaylist == null)
            {
                return;
            }

            MessageBoxResult result =
                MessageBox.Show(
                    $"Remove \"{song.Title}\" from \"{currentPlaylist.Name}\"?",
                    "Remove Song",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result !=
                MessageBoxResult.Yes)
            {
                return;
            }

            currentPlaylist.Songs.RemoveAll(
                existing =>
                    existing.VideoId ==
                    song.VideoId);

            playlistService.SavePlaylist(
                currentPlaylist);

            playbackQueue.RemoveAll(
                existing =>
                    existing.VideoId ==
                    song.VideoId);

            if (currentQueueIndex >=
                playbackQueue.Count)
            {
                currentQueueIndex =
                    playbackQueue.Count - 1;
            }

            PlaylistView.ShowPlaylist(
                currentPlaylist);
        }

        private void DeletePlaylist_Click(
            object? sender,
            Playlist playlist)
        {
            MessageBoxResult result =
                MessageBox.Show(
                    $"Delete the playlist \"{playlist.Name}\"?\n\nThis cannot be undone.",
                    "Delete Playlist",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            if (result !=
                MessageBoxResult.Yes)
            {
                return;
            }

            playlistService.DeletePlaylist(
                playlist);

            playlists.Remove(
                playlist);

            if (currentPlaylist ==
                playlist)
            {
                currentPlaylist =
                    null;

                PlaylistView.SetPlaylists(
                    playlists);

                ShowView(
                    AppView.Playlists);

                return;
            }

            PlaylistView.SetPlaylists(
                playlists);
        }

        private void ShowView(
            AppView view)
        {
            PlayerView.Visibility =
                view == AppView.Player
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            SearchView.Visibility =
                view == AppView.Search
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            PlaylistView.Visibility =
                view == AppView.Playlists
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void PlaylistButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            PlaylistView.SetPlaylists(
                playlists);

            ShowView(
                AppView.Playlists);
        }

        private void PlayerButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowView(
                AppView.Player);
        }

        private void TitleBar_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.LeftButton ==
                MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            WindowState =
                WindowState.Minimized;
        }

        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}