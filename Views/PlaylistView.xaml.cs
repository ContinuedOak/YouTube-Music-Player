using OakMusic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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

        public event EventHandler<Playlist>? PlaylistReordered;

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

            BackButton.Visibility =
                Visibility.Visible;

            PlayAllButton.Visibility =
                Visibility.Collapsed;

            PlaylistTitleText.Text =
                playlist.Name;

            TimeSpan totalDuration =
                TimeSpan.Zero;

            foreach (Song song in playlist.Songs)
            {
                totalDuration +=
                    song.Duration;
            }

            int totalHours =
                (int)totalDuration.TotalHours;

            int totalMinutes =
                totalDuration.Minutes;

            string durationText =
                totalHours > 0
                    ? $"{totalHours}h {totalMinutes}m"
                    : $"{totalMinutes}m";

            PlaylistSongCountText.Text =
                $"{durationText} - {playlist.Songs.Count} " +
                (playlist.Songs.Count == 1
                    ? "song"
                    : "songs");

            PlaylistSongsPanel.Children.Clear();

            foreach (Song song in playlist.Songs)
            {
                PlaylistSongsPanel.Children.Add(
                    CreateSongRow(song));
            }
        }

        private void ShowPlaylistList()
        {
            currentPlaylist =
                null;

            BackButton.Visibility =
                Visibility.Collapsed;

            PlayAllButton.Visibility =
                Visibility.Visible;

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
                        (Brush)FindResource(
                            "ControlBoarderBrush"),

                    Foreground =
                        (Brush)FindResource(
                            "PrimaryTextBrush"),

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
                        (Brush)FindResource(
                            "AlbumBackgroundBrush"),

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
                        (Brush)FindResource(
                            "PrimaryTextBrush"),

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
                        (Brush)FindResource(
                            "ControlBoarderBrush"),

                    Foreground =
                        (Brush)FindResource(
                            "PrimaryTextBrush"),

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
                        (Brush)FindResource(
                            "ControlBoarderBrush"),

                    Foreground =
                        (Brush)FindResource(
                            "PrimaryTextBrush"),

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
                    (Brush)FindResource(
                        "AlbumBackgroundBrush"),

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
                        (Brush)FindResource(
                            "AlbumBackgroundBrush"),

                    Foreground =
                        (Brush)FindResource(
                            "PrimaryTextBrush"),

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

            Point dragStartPoint =
                new Point();

            bool isDragging =
                false;

            AdornerLayer? adornerLayer =
                null;

            SongDragAdorner? dragAdorner =
                null;

            playButton.PreviewMouseLeftButtonDown +=
                (sender, e) =>
                {
                    dragStartPoint =
                        e.GetPosition(null);

                    isDragging =
                        false;
                };

            playButton.PreviewMouseMove +=
                (sender, e) =>
                {
                    if (e.LeftButton != MouseButtonState.Pressed)
                    {
                        return;
                    }

                    if (isDragging)
                    {
                        return;
                    }

                    Point currentPoint =
                    e.GetPosition(null);

                    Vector difference =
                    currentPoint -
                    dragStartPoint;

                    if (Math.Abs(difference.X) <
                        SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(difference.Y) <
                        SystemParameters.MinimumVerticalDragDistance)
                    {
                        return;
                    }

                    adornerLayer =
                    AdornerLayer.GetAdornerLayer(this);

                    if (adornerLayer == null)
                    {
                        return;
                    }

                    dragAdorner =
                    new SongDragAdorner(
                        this,
                        song,
                        (Brush)FindResource(
                            "AlbumBackgroundBrush"),
                        (Brush)FindResource(
                            "PrimaryTextBrush"));

                    adornerLayer.Add(
                    dragAdorner);

                    isDragging =
                    true;

                    GiveFeedbackEventHandler?
                    giveFeedbackHandler = null;

                    giveFeedbackHandler =
                    (feedbackSender,
                     feedbackArgs) =>
                    {
                        if (dragAdorner == null)
                        {
                            return;
                        }

                        Point mousePosition =
                            Mouse.GetPosition(this);

                        dragAdorner.SetPosition(
                            mousePosition);

                        feedbackArgs.UseDefaultCursors =
                            false;

                        Mouse.SetCursor(
                            Cursors.Hand);
                    };

                    playButton.GiveFeedback +=
                    giveFeedbackHandler;

                    try
                    {
                        DragDrop.DoDragDrop(
                        playButton,
                        song,
                        DragDropEffects.Move);
                    }
                    finally
                    {
                        playButton.GiveFeedback -=
                        giveFeedbackHandler;

                        if (dragAdorner != null &&
                        adornerLayer != null)
                        {
                            adornerLayer.Remove(
                            dragAdorner);
                        }

                        dragAdorner =
                        null;

                        adornerLayer =
                        null;

                        isDragging =
                        false;

                        Mouse.SetCursor(
                        Cursors.Arrow);
                    }
                };

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
                        (Brush)FindResource(
                            "PrimaryTextBrush"),

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
                        (Brush)FindResource(
                            "PrimaryTextBrush"),

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
                        (Brush)FindResource(
                            "AlbumBackgroundBrush"),

                    Foreground =
                        (Brush)FindResource(
                            "PrimaryTextBrush"),

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

            Border row =
                new Border
                {
                    Child =
                        grid,

                    AllowDrop =
                        true
                };

            row.DragOver +=
                (sender, e) =>
                {
                    if (e.Data.GetData(
                            typeof(Song))
                        is Song)
                    {
                        e.Effects =
                            DragDropEffects.Move;

                        e.Handled =
                            true;
                    }
                    else
                    {
                        e.Effects =
                            DragDropEffects.None;
                    }
                };

            row.Drop +=
                (sender, e) =>
                {
                    if (e.Data.GetData(
                            typeof(Song))
                        is not Song draggedSong)
                    {
                        return;
                    }

                    if (currentPlaylist == null)
                    {
                        return;
                    }

                    int oldIndex =
                        currentPlaylist.Songs.IndexOf(
                            draggedSong);

                    int newIndex =
                        currentPlaylist.Songs.IndexOf(
                            song);

                    if (oldIndex < 0 ||
                        newIndex < 0 ||
                        oldIndex == newIndex)
                    {
                        return;
                    }

                    currentPlaylist.Songs.RemoveAt(
                        oldIndex);

                    if (oldIndex < newIndex)
                    {
                        newIndex--;
                    }

                    currentPlaylist.Songs.Insert(
                        newIndex,
                        draggedSong);

                    PlaylistReordered?.Invoke(
                        this,
                        currentPlaylist);

                    ShowPlaylist(
                        currentPlaylist);

                    e.Handled =
                        true;
                };

            return row;
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
            {
                return;
            }

            PlayAllClicked?.Invoke(
                this,
                currentPlaylist);
        }

        private sealed class SongDragAdorner : Adorner
        {
            private readonly Border visual;
            private readonly TranslateTransform transform;

            public SongDragAdorner(
                UIElement adornedElement,
                Song song,
                Brush background,
                Brush foreground)
                : base(adornedElement)
            {
                IsHitTestVisible =
                    false;

                Opacity =
                    0.92;

                transform =
                    new TranslateTransform();

                RenderTransform =
                    transform;

                Grid grid =
                    new Grid
                    {
                        Width = 280,
                        Height = 48
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
                            new GridLength(80)
                    });

                TextBlock title =
                    new TextBlock
                    {
                        Text =
                            song.Title,

                        Foreground =
                            foreground,

                        FontSize =
                            13,

                        VerticalAlignment =
                            VerticalAlignment.Center,

                        Margin =
                            new Thickness(
                                14,
                                0,
                                0,
                                0),

                        TextTrimming =
                            TextTrimming.CharacterEllipsis
                    };

                Grid.SetColumn(
                    title,
                    0);

                grid.Children.Add(
                    title);

                TextBlock duration =
                    new TextBlock
                    {
                        Text =
                            song.DurationText,

                        Foreground =
                            foreground,

                        FontSize =
                            12,

                        HorizontalAlignment =
                            HorizontalAlignment.Right,

                        VerticalAlignment =
                            VerticalAlignment.Center,

                        Margin =
                            new Thickness(
                                0,
                                0,
                                14,
                                0)
                    };

                Grid.SetColumn(
                    duration,
                    1);

                grid.Children.Add(
                    duration);

                visual =
                    new Border
                    {
                        Width = 280,
                        Height = 48,

                        CornerRadius =
                            new CornerRadius(6),

                        Background =
                            background,

                        BorderBrush =
                            foreground,

                        BorderThickness =
                            new Thickness(1),

                        Child =
                            grid
                    };
            }

            public void SetPosition(
                Point mousePosition)
            {
                transform.X =
                    mousePosition.X - 140;

                transform.Y =
                    mousePosition.Y - 24;

                InvalidateVisual();
            }

            protected override int VisualChildrenCount =>
                1;

            protected override Visual GetVisualChild(
                int index)
            {
                if (index != 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(index));
                }

                return visual;
            }

            protected override Size MeasureOverride(
                Size constraint)
            {
                visual.Measure(
                    constraint);

                return visual.DesiredSize;
            }

            protected override Size ArrangeOverride(
                Size finalSize)
            {
                visual.Arrange(
                    new Rect(
                        0,
                        0,
                        visual.DesiredSize.Width,
                        visual.DesiredSize.Height));

                return finalSize;
            }
        }
    }
}