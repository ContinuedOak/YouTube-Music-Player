using OakMusic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OakMusic.Views
{
    public partial class PlaylistView : UserControl
    {
        public event EventHandler? CreatePlaylistClicked;

        public event EventHandler<Playlist>? PlaylistClicked;

        public event EventHandler<Playlist>? EditPlaylistClicked;

        public event EventHandler? BackClicked;

        public event EventHandler<Playlist>? PlayAllClicked;

        public event EventHandler<Song>? SongClicked;

        public event EventHandler<Song>? DeleteSongClicked;

        public event EventHandler<Playlist>? DeletePlaylistClicked;

        private List<Playlist> playlists =
            new();

        private Playlist? currentPlaylist;

        public PlaylistView()
        {
            InitializeComponent();
        }

        public void SetPlaylists(
            List<Playlist> playlists)
        {
            this.playlists =
                playlists;

            ShowPlaylistList();
        }

        public void ShowPlaylist(
            Playlist playlist)
        {
            currentPlaylist =
                playlist;

            PlaylistTitleText.Text =
                playlist.Name;

            PlaylistSongCountText.Text =
                $"{playlist.Songs.Count} " +
                (playlist.Songs.Count == 1
                    ? "song"
                    : "songs");

            PlaylistSongsPanel.Children.Clear();

            foreach (Song song in
                playlist.Songs)
            {
                PlaylistSongsPanel.Children.Add(
                    CreateSongRow(song));
            }
        }

        private void ShowPlaylistList()
        {
            currentPlaylist =
                null;

            PlaylistTitleText.Text =
                "Playlists";

            PlaylistSongCountText.Text =
                "";

            PlaylistSongsPanel.Children.Clear();

            foreach (Playlist playlist in
                playlists)
            {
                PlaylistSongsPanel.Children.Add(
                    CreatePlaylistRow(
                        playlist));
            }
        }

        private Border CreatePlaylistRow(
            Playlist playlist)
        {
            Grid grid =
                new Grid
                {
                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            6)
                };

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            42)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            42)
                });

            Button openButton =
                new Button
                {
                    Height =
                        52,

                    HorizontalContentAlignment =
                        HorizontalAlignment.Stretch,

                    Background =
                        (Brush)FindResource("ControlBoarderBrush"),

                    Foreground =
                        (Brush)FindResource("PrimaryTextBrush"),

                    BorderThickness =
                        new Thickness(0),

                    Padding =
                        new Thickness(
                            10,
                            0,
                            10,
                            0),

                    Tag =
                        playlist
                };

            openButton.Click +=
                PlaylistButton_Click;

            Grid content =
                new Grid();

            content.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(48)
                });

            content.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            Border icon =
                new Border
                {
                    Width =
                        38,

                    Height =
                        38,

                    CornerRadius =
                        new CornerRadius(6),

                    Background =
                        (Brush)FindResource("AlbumBackgroundBrush"),

                    ClipToBounds =
                        true,

                    VerticalAlignment =
                        VerticalAlignment.Center
                };

            if (!string.IsNullOrWhiteSpace(
                    playlist.IconPath) &&
                File.Exists(
                    playlist.IconPath))
            {
                try
                {
                    BitmapImage image =
                        new BitmapImage();

                    image.BeginInit();

                    image.UriSource =
                        new Uri(
                            playlist.IconPath,
                            UriKind.Absolute);

                    image.CacheOption =
                        BitmapCacheOption.OnLoad;

                    image.EndInit();

                    image.Freeze();

                    Image imageControl =
                        new Image
                        {
                            Source =
                                image,

                            Stretch =
                                Stretch.UniformToFill
                        };

                    icon.Child =
                        imageControl;
                }
                catch
                {
                    icon.Child =
                        CreateDefaultPlaylistIcon();
                }
            }
            else
            {
                icon.Child =
                    CreateDefaultPlaylistIcon();
            }

            Grid.SetColumn(
                icon,
                0);

            content.Children.Add(
                icon);

            TextBlock name =
                new TextBlock
                {
                    Text =
                        playlist.Name,

                    Foreground =
                        (Brush)FindResource("PrimaryTextBrush"),

                    FontSize =
                        14,

                    VerticalAlignment =
                        VerticalAlignment.Center,

                    TextTrimming =
                        TextTrimming.CharacterEllipsis
                };

            Grid.SetColumn(
                name,
                1);

            content.Children.Add(
                name);

            openButton.Content =
                content;

            Grid.SetColumn(
                openButton,
                0);

            grid.Children.Add(
                openButton);

            Button editButton =
                new Button
                {
                    Content =
                        "✎",

                    Height =
                        52,

                    Background =
                        (Brush)FindResource("ControlBoarderBrush"),

                    Foreground =
                        (Brush)FindResource("PrimaryTextBrush"),

                    BorderThickness =
                        new Thickness(0),

                    FontSize =
                        17,

                    Tag =
                        playlist,

                    ToolTip =
                        "Edit Playlist"
                };

            editButton.Click +=
                EditPlaylistButton_Click;

            Grid.SetColumn(
                editButton,
                1);

            grid.Children.Add(
                editButton);

            Button deleteButton =
                new Button
                {
                    Content =
                        "×",

                    Height =
                        52,

                    Background =
                        (Brush)FindResource("ControlBoarderBrush"),

                    Foreground =
                        (Brush)FindResource("PrimaryTextBrush"),

                    BorderThickness =
                        new Thickness(0),

                    FontSize =
                        18,

                    Tag =
                        playlist,

                    ToolTip =
                        "Delete Playlist"
                };

            deleteButton.Click +=
                DeletePlaylistButton_Click;

            Grid.SetColumn(
                deleteButton,
                2);

            grid.Children.Add(
                deleteButton);

            return new Border
            {
                Child =
                    grid
            };
        }

        private TextBlock CreateDefaultPlaylistIcon()
        {
            return new TextBlock
            {
                Text =
                    "♫",

                Foreground =
                    (Brush)FindResource("AlbumBackgroundBrush"),

                FontSize =
                    18,

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                VerticalAlignment =
                    VerticalAlignment.Center
            };
        }

        private Border CreateSongRow(
            Song song)
        {
            Grid grid =
                new Grid
                {
                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            6)
                };

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(42)
                });

            Button playButton =
                new Button
                {
                    Height =
                        48,

                    HorizontalContentAlignment =
                        HorizontalAlignment.Stretch,

                    Background =
                        (Brush)FindResource("AlbumBackgroundBrush"),

                    Foreground =
                        (Brush)FindResource("PrimaryTextBrush"),

                    BorderThickness =
                        new Thickness(0),

                    Padding =
                        new Thickness(
                            14,
                            0,
                            14,
                            0),

                    Tag =
                        song
                };

            playButton.Click +=
                SongButton_Click;

            Grid content =
                new Grid();

            content.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            content.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(80)
                });

            TextBlock title =
                new TextBlock
                {
                    Text =
                        song.Title,

                    Foreground =
                        (Brush)FindResource("PrimaryTextBrush"),

                    FontSize =
                        13,

                    VerticalAlignment =
                        VerticalAlignment.Center,

                    TextTrimming =
                        TextTrimming.CharacterEllipsis
                };

            Grid.SetColumn(
                title,
                0);

            content.Children.Add(
                title);

            TextBlock duration =
                new TextBlock
                {
                    Text =
                        song.DurationText,

                    Foreground =
                        (Brush)FindResource("PrimaryTextBrush"),

                    FontSize =
                        12,

                    HorizontalAlignment =
                        HorizontalAlignment.Right,

                    VerticalAlignment =
                        VerticalAlignment.Center
                };

            Grid.SetColumn(
                duration,
                1);

            content.Children.Add(
                duration);

            playButton.Content =
                content;

            Grid.SetColumn(
                playButton,
                0);

            grid.Children.Add(
                playButton);

            Button deleteButton =
                new Button
                {
                    Content =
                        "×",

                    Height =
                        48,

                    Background =
                        (Brush)FindResource("AlbumBackgroundBrush"),

                    Foreground =
                        (Brush)FindResource("PrimaryTextBrush"),

                    BorderThickness =
                        new Thickness(0),

                    FontSize =
                        18,

                    Tag =
                        song,

                    ToolTip =
                        "Remove Song"
                };

            deleteButton.Click +=
                DeleteSongButton_Click;

            Grid.SetColumn(
                deleteButton,
                1);

            grid.Children.Add(
                deleteButton);

            return new Border
            {
                Child =
                    grid
            };
        }

        private void PlaylistButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.Tag is not Playlist playlist)
            {
                return;
            }

            PlaylistClicked?.Invoke(
                this,
                playlist);
        }

        private void EditPlaylistButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.Tag is not Playlist playlist)
            {
                return;
            }

            EditPlaylistClicked?.Invoke(
                this,
                playlist);
        }

        private void DeletePlaylistButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.Tag is not Playlist playlist)
            {
                return;
            }

            DeletePlaylistClicked?.Invoke(
                this,
                playlist);
        }

        private void SongButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.Tag is not Song song)
            {
                return;
            }

            SongClicked?.Invoke(
                this,
                song);
        }

        private void DeleteSongButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button ||
                button.Tag is not Song song)
            {
                return;
            }

            DeleteSongClicked?.Invoke(
                this,
                song);
        }

        private void CreatePlaylistButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            CreatePlaylistClicked?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            BackClicked?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void PlayAllButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (currentPlaylist == null)
                return;

            PlayAllClicked?.Invoke(
                this,
                currentPlaylist);
        }
    }
}