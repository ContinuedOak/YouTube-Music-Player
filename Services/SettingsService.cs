using System;
using System.IO;
using System.Text.Json;
using OakMusic.Models;

namespace OakMusic.Services
{
    public class SettingsService
    {
        private readonly string settingsDirectory;
        private readonly string settingsFile;

        public AppSettings Settings { get; private set; }

        public SettingsService()
        {
            settingsDirectory =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData),
                    "OaksMusic");

            settingsFile =
                Path.Combine(
                    settingsDirectory,
                    "settings.json");

            Settings =
                Load();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(
                    settingsDirectory);

                JsonSerializerOptions options =
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                string json =
                    JsonSerializer.Serialize(
                        Settings,
                        options);

                File.WriteAllText(
                    settingsFile,
                    json);
            }
            catch
            {
            }
        }

        private AppSettings Load()
        {
            try
            {
                if (!File.Exists(settingsFile))
                {
                    return new AppSettings();
                }

                string json =
                    File.ReadAllText(
                        settingsFile);

                AppSettings? settings =
                    JsonSerializer.Deserialize<AppSettings>(
                        json);

                return settings ??
                       new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }
    }
}