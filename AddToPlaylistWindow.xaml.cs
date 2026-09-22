using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OakMusic.Models;

namespace OakMusic
{
    public partial class AddToPlaylistWindow : Window
    {
        public Playlist? SelectedPlaylist { get; private set; }

        public bool CreateNewPlaylistRequested { get; private set; }

        public AddToPlaylistWindow(
            List<Playlist> playlists)
        {
            InitializeComponent();

            BuildPlaylistList(
                playlists);
        }

        private void BuildPlaylistList(
            List<Playlist> playlists)
        {
            PlaylistPanel.Children.Clear();

            Button createButton =
                new Button
                {
                    Content = "+  Create New Playlist",
                    Height = 44,
                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            8),
                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                40,
                                40,
                                40)),
                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                235,
                                235,
                                235)),
                    BorderThickness =
                        new Thickness(0),
                    HorizontalContentAlignment =
                        HorizontalAlignment.Left,
                    Padding =
                        new Thickness(
                            14,
                            0,
                            14,
                            0),
                    FontSize = 13
                };

            createButton.Click +=
                CreateButton_Click;

            PlaylistPanel.Children.Add(
                createButton);

            foreach (Playlist playlist in playlists)
            {
                Button playlistButton =
                    new Button
                    {
                        Tag = playlist,
                        Height = 54,
                        Margin =
                            new Thickness(
                                0,
                                0,
                                0,
                                6),
                        Background =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    32,
                                    32,
                                    32)),
                        Foreground =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    235,
                                    235,
                                    235)),
                        BorderThickness =
                            new Thickness(0),
                        HorizontalContentAlignment =
                            HorizontalAlignment.Stretch
                    };

                playlistButton.Click +=
                    PlaylistButton_Click;

                Grid grid =
                    new Grid();

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
                            new GridLength(60)
                    });

                StackPanel text =
                    new StackPanel
                    {
                        VerticalAlignment =
                            VerticalAlignment.Center,
                        Margin =
                            new Thickness(
                                14,
                                0,
                                0,
                                0)
                    };

                TextBlock name =
                    new TextBlock
                    {
                        Text = playlist.Name,
                        Foreground =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    235,
                                    235,
                                    235)),
                        FontSize = 13,
                        FontWeight =
                            FontWeights.SemiBold,
                        TextTrimming =
                            TextTrimming.CharacterEllipsis
                    };

                TextBlock count =
                    new TextBlock
                    {
                        Text =
                            $"{playlist.Songs.Count} songs",
                        Foreground =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    110,
                                    110,
                                    110)),
                        FontSize = 11,
                        Margin =
                            new Thickness(
                                0,
                                3,
                                0,
                                0)
                    };

                text.Children.Add(name);
                text.Children.Add(count);

                Grid.SetColumn(
                    text,
                    0);

                grid.Children.Add(text);

                TextBlock arrow =
                    new TextBlock
                    {
                        Text = "›",
                        Foreground =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    110,
                                    110,
                                    110)),
                        FontSize = 22,
                        HorizontalAlignment =
                            HorizontalAlignment.Center,
                        VerticalAlignment =
                            VerticalAlignment.Center
                    };

                Grid.SetColumn(
                    arrow,
                    1);

                grid.Children.Add(arrow);

                playlistButton.Content =
                    grid;

                PlaylistPanel.Children.Add(
                    playlistButton);
            }
        }

        private void PlaylistButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            if (button.Tag is not Playlist playlist)
            {
                return;
            }

            SelectedPlaylist =
                playlist;

            CreateNewPlaylistRequested =
                false;

            DialogResult =
                true;
        }

        private void CreateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SelectedPlaylist = null;

            CreateNewPlaylistRequested =
                true;

            DialogResult =
                true;
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SelectedPlaylist = null;

            CreateNewPlaylistRequested =
                false;

            DialogResult =
                false;
        }
    }
}