using Microsoft.Win32;
using OakMusic.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OakMusic
{
    public partial class PlaylistNameWindow : Window
    {
        public string PlaylistName { get; private set; } = "";

        public string IconPath { get; private set; } = "";

        public bool IconChanged { get; private set; }

        public PlaylistNameWindow()
        {
            InitializeComponent();

            WindowTitle.Text =
                "Create Playlist";

            SaveButton.Content =
                "Create";
        }

        public PlaylistNameWindow(
            Playlist playlist)
        {
            InitializeComponent();

            WindowTitle.Text =
                "Edit Playlist";

            SaveButton.Content =
                "Save";

            PlaylistNameTextBox.Text =
                playlist.Name;

            IconPath =
                playlist.IconPath ?? "";

            RemoveIconButton.Visibility =
                string.IsNullOrWhiteSpace(IconPath)
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            LoadIconPreview();
        }

        private void ChooseIconButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenFileDialog dialog =
                new OpenFileDialog
                {
                    Title =
                        "Choose Playlist Icon",

                    Filter =
                        "Image Files|*.png;*.jpg;*.jpeg;*.webp|All Files|*.*",

                    CheckFileExists =
                        true,

                    Multiselect =
                        false
                };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                BitmapImage image =
                    new BitmapImage();

                image.BeginInit();

                image.UriSource =
                    new Uri(
                        dialog.FileName,
                        UriKind.Absolute);

                image.CacheOption =
                    BitmapCacheOption.OnLoad;

                image.EndInit();

                image.Freeze();

                IconPath =
                    dialog.FileName;

                IconChanged =
                    true;

                PlaylistIconPreview.Source =
                    image;

                RemoveIconButton.Visibility =
                    Visibility.Visible;
            }
            catch
            {
                IconPath =
                    "";

                PlaylistIconPreview.Source =
                    null;
            }
        }

        private void RemoveIconButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            IconPath =
                "";

            IconChanged =
                true;

            PlaylistIconPreview.Source =
                null;

            RemoveIconButton.Visibility =
                Visibility.Collapsed;
        }

        private void LoadIconPreview()
        {
            if (string.IsNullOrWhiteSpace(
                    IconPath) ||
                !File.Exists(
                    IconPath))
            {
                IconPath =
                    "";

                PlaylistIconPreview.Source =
                    null;

                RemoveIconButton.Visibility =
                    Visibility.Collapsed;

                return;
            }

            try
            {
                BitmapImage image =
                    new BitmapImage();

                image.BeginInit();

                image.UriSource =
                    new Uri(
                        IconPath,
                        UriKind.Absolute);

                image.CacheOption =
                    BitmapCacheOption.OnLoad;

                image.EndInit();

                image.Freeze();

                PlaylistIconPreview.Source =
                    image;
            }
            catch
            {
                PlaylistIconPreview.Source =
                    null;
            }
        }

        private void CreateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string name =
                PlaylistNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            PlaylistName =
                name;

            DialogResult =
                true;
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult =
                false;
        }
    }
}