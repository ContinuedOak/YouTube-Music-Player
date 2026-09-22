using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OakMusic.Models;

namespace OakMusic.Views
{
    public partial class SearchView : UserControl
    {
        private static readonly HttpClient HttpClient = new();

        public SearchView()
        {
            InitializeComponent();
        }

        public void SetResults(
            List<Song> results,
            RoutedEventHandler clickHandler,
            RoutedEventHandler addHandler)
        {
            SearchResultsPanel.Children.Clear();

            foreach (Song song in results)
            {
                Grid row =
                    CreateResultRow(
                        song,
                        clickHandler,
                        addHandler);

                SearchResultsPanel.Children.Add(row);

                LoadThumbnailAsync(
                    song.ThumbnailUrl,
                    row);
            }
        }

        private Grid CreateResultRow(
            Song song,
            RoutedEventHandler clickHandler,
            RoutedEventHandler addHandler)
        {
            Grid row =
                new Grid
                {
                    Height = 64,
                    Margin = new Thickness(0, 0, 0, 6)
                };

            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(42)
                });

            Button songButton =
                new Button
                {
                    Tag = song,
                    Height = 64,
                    Padding = new Thickness(8),
                    HorizontalContentAlignment =
                        HorizontalAlignment.Stretch,
                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(32, 32, 32)),
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(235, 235, 235)),
                    BorderThickness =
                        new Thickness(0)
                };

            songButton.Click += clickHandler;

            Grid.SetColumn(
                songButton,
                0);

            Grid content =
                new Grid();

            content.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(48)
                });

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
                    Width = new GridLength(50)
                });

            Border cover =
                new Border
                {
                    Width = 48,
                    Height = 48,
                    CornerRadius =
                        new CornerRadius(6),
                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(45, 45, 45))
                };

            TextBlock musicIcon =
                new TextBlock
                {
                    Text = "♪",
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(90, 90, 90)),
                    FontSize = 20,
                    HorizontalAlignment =
                        HorizontalAlignment.Center,
                    VerticalAlignment =
                        VerticalAlignment.Center
                };

            cover.Child = musicIcon;

            Grid.SetColumn(
                cover,
                0);

            content.Children.Add(cover);

            StackPanel textPanel =
                new StackPanel
                {
                    Margin =
                        new Thickness(
                            12,
                            0,
                            8,
                            0),
                    VerticalAlignment =
                        VerticalAlignment.Center
                };

            TextBlock title =
                new TextBlock
                {
                    Text = song.Title,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(235, 235, 235)),
                    FontSize = 13,
                    FontWeight =
                        FontWeights.SemiBold,
                    TextTrimming =
                        TextTrimming.CharacterEllipsis
                };

            string artist =
                !string.IsNullOrWhiteSpace(
                    song.ChannelName)
                    ? song.ChannelName
                    : song.Artist;

            TextBlock artistText =
                new TextBlock
                {
                    Text =
                        string.IsNullOrWhiteSpace(artist)
                            ? "YouTube Music"
                            : artist,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(125, 125, 125)),
                    FontSize = 11,
                    Margin =
                        new Thickness(
                            0,
                            3,
                            0,
                            0),
                    TextTrimming =
                        TextTrimming.CharacterEllipsis
                };

            textPanel.Children.Add(title);
            textPanel.Children.Add(artistText);

            Grid.SetColumn(
                textPanel,
                1);

            content.Children.Add(textPanel);

            TextBlock duration =
                new TextBlock
                {
                    Text = song.DurationText,
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(105, 105, 105)),
                    FontSize = 11,
                    VerticalAlignment =
                        VerticalAlignment.Center,
                    HorizontalAlignment =
                        HorizontalAlignment.Right
                };

            Grid.SetColumn(
                duration,
                2);

            content.Children.Add(duration);

            songButton.Content = content;

            row.Children.Add(songButton);

            Button addButton =
                new Button
                {
                    Tag = song,
                    Content = "+",
                    Width = 42,
                    Height = 64,
                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(32, 32, 32)),
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(180, 180, 180)),
                    BorderThickness =
                        new Thickness(0),
                    FontSize = 20,
                    FontWeight =
                        FontWeights.Normal
                };

            addButton.Click += addHandler;

            Grid.SetColumn(
                addButton,
                1);

            row.Children.Add(addButton);

            return row;
        }

        private async void LoadThumbnailAsync(
            string url,
            Grid row)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            try
            {
                byte[] data =
                    await HttpClient.GetByteArrayAsync(url);

                using var stream =
                    new System.IO.MemoryStream(data);

                BitmapImage image =
                    new BitmapImage();

                image.BeginInit();

                image.CacheOption =
                    BitmapCacheOption.OnLoad;

                image.StreamSource =
                    stream;

                image.EndInit();

                image.Freeze();

                if (row.Children.Count > 0 &&
                    row.Children[0] is Button songButton &&
                    songButton.Content is Grid grid &&
                    grid.Children.Count > 0 &&
                    grid.Children[0] is Border border)
                {
                    border.Child =
                        new Image
                        {
                            Source = image,
                            Stretch =
                                Stretch.UniformToFill
                        };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Thumbnail error: {ex}");
            }
        }
    }
}