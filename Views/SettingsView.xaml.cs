using System;
using System.Windows;
using System.Windows.Controls;
using OakMusic.Models;
using OakMusic.Services;

namespace OakMusic.Views
{
    public partial class SettingsView : UserControl
    {
        private SettingsService? settingsService;
        private bool isLoading;

        public event EventHandler? SettingsChanged;

        public SettingsView()
        {
            InitializeComponent();
        }

        public void Initialize(SettingsService service)
        {
            settingsService = service;
            isLoading = true;

            AppSettings settings =
                settingsService.Settings;

            DarkModeCheckBox.IsChecked =
                settings.DarkMode;

            AlwaysOnTopCheckBox.IsChecked =
                settings.AlwaysOnTop;

            UpdatePlaytime(
                settings.Playtime);

            isLoading = false;
        }

        public void UpdatePlaytime(
            double seconds)
        {
            TimeSpan time =
                TimeSpan.FromSeconds(seconds);

            UserTime.Text =
                time.TotalHours >= 1
                    ? $"Listen Timer: {(int)time.TotalHours}h {time.Minutes}m"
                    : $"Listen Timer: {time.Minutes}m {time.Seconds}s";
        }

        private void SettingChanged(
            object sender,
            RoutedEventArgs e)
        {
            if (isLoading ||
                settingsService == null)
            {
                return;
            }

            settingsService.Settings.DarkMode =
                DarkModeCheckBox.IsChecked == true;

            settingsService.Settings.AlwaysOnTop =
                AlwaysOnTopCheckBox.IsChecked == true;

            settingsService.Save();

            SettingsChanged?.Invoke(
                this,
                EventArgs.Empty);
        }
    }
}