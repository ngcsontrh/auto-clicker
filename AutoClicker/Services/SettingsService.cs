using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoClicker.Services
{
    public sealed class SettingsService
    {
        private readonly string _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AutoClicker",
            "Settings.json");

        public AppSettings Load()
        {
            if (!File.Exists(_settingsPath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.AppSettings)
                ?? throw new InvalidDataException("The settings file is empty.");

            if (!settings.IsValid)
            {
                throw new InvalidDataException("The settings file contains invalid values.");
            }

            return settings;
        }

        public void Save(AppSettings settings)
        {
            if (!settings.IsValid)
            {
                throw new InvalidDataException("The settings contain invalid values.");
            }

            var directory = Path.GetDirectoryName(_settingsPath)
                ?? throw new InvalidOperationException("The settings file path has no parent directory.");
            Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.AppSettings);
            File.WriteAllText(_settingsPath, json);
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(AppSettings))]
    internal partial class SettingsJsonContext : JsonSerializerContext
    {
    }
}
