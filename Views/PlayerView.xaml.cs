using OakMusic.Models;
using OakMusic.Services;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OakMusic.Views
{
    public partial class PlayerView : UserControl
    {
        public event RoutedEventHandler? PlayClicked;
        public event RoutedEventHandler? PreviousClicked;
        public event RoutedEventHandler? NextClicked;

        public event RoutedEventHandler? ShuffleClicked;
        public event RoutedEventHandler? RepeatClicked;

        public event RoutedPropertyChangedEventHandler<double>? VolumeChanged;
        public event RoutedPropertyChangedEventHandler<double>? ProgressChanged;

        public bool IsShuffleEnabled { get; private set; }

        public bool IsRepeatEnabled { get; private set; }

        private static readonly HttpClient HttpClient = new();

        private bool suppressProgressEvent;

        private SettingsService? settingsService;

        private bool isLoading;

        public PlayerView()
        {
            InitializeComponent();
        }

        public void Initialize(SettingsService service)
        {
            settingsService = service;

            isLoading = true;

            VolumeSlider.Value =
                settingsService.Settings.Volume;

            isLoading = false;
        }

        public void SetSong(Song song)
        {
            SongTitleText.Text =
                song.Title;

            string artist =
                !string.IsNullOrWhiteSpace(song.ChannelName)
                    ? song.ChannelName
                    : song.Artist;

            ChannelText.Text =
                string.IsNullOrWhiteSpace(artist)
                    ? "Arist Not Found"
                    : artist;

            suppressProgressEvent = true;

            ProgressSlider.Value =
                0;

            suppressProgressEvent = false;

            if (song.Duration.TotalSeconds > 0)
            {
                ProgressSlider.Maximum =
                    song.Duration.TotalSeconds;

                DurationText.Text =
                    FormatTime(song.Duration);
            }
            else
            {
                ProgressSlider.Maximum =
                    100;

                DurationText.Text =
                    "0:00";
            }

            CurrentTimeText.Text =
                "0:00";

            LoadAlbumCover(
                song.ThumbnailUrl);
        }

        public void SetProgress(
            double seconds)
        {
            if (seconds < 0)
            {
                seconds = 0;
            }

            if (seconds > ProgressSlider.Maximum)
            {
                seconds =
                    ProgressSlider.Maximum;
            }

            suppressProgressEvent = true;

            ProgressSlider.Value =
                seconds;

            suppressProgressEvent = false;
        }

        public void SetCurrentTime(
            TimeSpan time)
        {
            CurrentTimeText.Text =
                FormatTime(time);
        }

        public void SetPlaying(bool playing)
        {
            PlayButton.Content =
                playing
                    ? "❚❚"
                    : "▶";

            UpdatePlayButton(playing);
        }

        public void SetShuffle(
            bool enabled)
        {
            IsShuffleEnabled =
                enabled;

            UpdateShuffleButton();
        }

        public void SetRepeat(
            bool enabled)
        {
            IsRepeatEnabled =
                enabled;

            UpdateRepeatButton();
        }

        private async void LoadAlbumCover(
            string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                AlbumCover.Source =
                    null;

                return;
            }

            try
            {
                byte[] data =
                    await HttpClient.GetByteArrayAsync(
                        url);

                using var stream =
                    new System.IO.MemoryStream(
                        data);

                BitmapImage image =
                    new BitmapImage();

                image.BeginInit();

                image.CacheOption =
                    BitmapCacheOption.OnLoad;

                image.StreamSource =
                    stream;

                image.EndInit();

                image.Freeze();

                AlbumCover.Source =
                    image;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Album cover error: {ex}");

                AlbumCover.Source =
                    null;
            }
        }

        private string FormatTime(
            TimeSpan time)
        {
            return time.TotalHours >= 1
                ? time.ToString(@"h\:mm\:ss")
                : time.ToString(@"m\:ss");
        }

        private void PlayButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            PlayClicked?.Invoke(
                sender,
                e);
        }

        private void Shuffle_Click(
            object sender,
            RoutedEventArgs e)
        {
            IsShuffleEnabled =
                !IsShuffleEnabled;

            UpdateShuffleButton();

            ShuffleClicked?.Invoke(
                sender,
                e);
        }

        private void Repeat_Click(
            object sender,
            RoutedEventArgs e)
        {
            IsRepeatEnabled =
                !IsRepeatEnabled;

            UpdateRepeatButton();

            RepeatClicked?.Invoke(
                sender,
                e);
        }

        public void UpdatePlayButton(bool playing)
        {
            PlayButton.Foreground =
                playing
                    ? (Brush)FindResource("ControlForegroundBrush")
                    : (Brush)FindResource("MutedTextBrush");

            PlayButton.Background =
                playing
                    ? (Brush)FindResource("AlbumBackgroundBrush")
                    : (Brush)FindResource("SurfaceBrush");
        }
        public void UpdateShuffleButton()
        {
            if (ShuffleToggleButton == null)
            {
                return;
            }

            ShuffleToggleButton.Foreground =
                IsShuffleEnabled
                    ? (Brush)FindResource("ControlForegroundBrush")
                    : (Brush)FindResource("MutedTextBrush");

            ShuffleToggleButton.Background =
                IsShuffleEnabled
                    ? (Brush)FindResource("ControlFillBrush")
                    : (Brush)FindResource("SurfaceBrush");
        }

        public void UpdateRepeatButton()
        {
            if (RepeatToggleButton == null)
            {
                return;
            }

            RepeatToggleButton.Foreground =
                IsRepeatEnabled
                    ? (Brush)FindResource("ControlForegroundBrush")
                    : (Brush)FindResource("MutedTextBrush");

            RepeatToggleButton.Background =
                IsRepeatEnabled
                    ? (Brush)FindResource("ControlFillBrush")
                    : (Brush)FindResource("SurfaceBrush");
        }

        private void PreviousButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            PreviousClicked?.Invoke(
                sender,
                e);
        }

        private void NextButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            NextClicked?.Invoke(
                sender,
                e);
        }

        private void VolumeSlider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeIcon == null)
            {
                return;
            }

            if (e.NewValue <= 0)
            {
                VolumeIcon.Text =
                    "🔇";
            }
            else if (e.NewValue <= 33)
            {
                VolumeIcon.Text =
                    "🔈";
            }
            else if (e.NewValue <= 66)
            {
                VolumeIcon.Text =
                    "🔉";
            }
            else
            {
                VolumeIcon.Text =
                    "🔊";
            }

            if (!isLoading &&
                settingsService != null)
            {
                settingsService.Settings.Volume =
                    e.NewValue;

                settingsService.Save();
            }

            VolumeChanged?.Invoke(
                this,
                e);
        }

        private void ProgressSlider_MouseDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            ProgressSlider.CaptureMouse();
        }

        private void ProgressSlider_MouseUp(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            ProgressSlider.ReleaseMouseCapture();
        }

        private void ProgressSlider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (suppressProgressEvent)
            {
                return;
            }

            ProgressChanged?.Invoke(
                sender,
                e);
        }
    }
}