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

        public void Initialize(
            SettingsService service)
        {
            settingsService =
                service;

            isLoading = true;

            AppSettings settings =
                settingsService.Settings;

            DarkModeCheckBox.IsChecked =
                settings.DarkMode;

            AlwaysOnTopCheckBox.IsChecked =
                settings.AlwaysOnTop;

            isLoading = false;
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