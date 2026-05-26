using System;
using System.IO;
using System.Text.Json;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private AppSettings _settings;

    public SettingsService()
    {
        _settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        _settings = LoadSettings();
    }

    public string LoadLanguage() => _settings.Language;

    public void SaveLanguage(string languageCode)
    {
        _settings = _settings with { Language = languageCode };
        SaveSettings();
    }

    public int LoadPort() => _settings.Port;

    public void SavePort(int port)
    {
        _settings = _settings with { Port = port };
        SaveSettings();
    }

    public string? LoadLastIpAddress() => _settings.LastIpAddress;

    public void SaveLastIpAddress(string ipAddress)
    {
        _settings = _settings with { LastIpAddress = ipAddress };
        SaveSettings();
    }

    private AppSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return new AppSettings();

            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    private void SaveSettings()
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Silently ignore save errors
        }
    }

    private record AppSettings
    {
        public string Language { get; init; } = "en";
        public int Port { get; init; } = 80;
        public string? LastIpAddress { get; init; }
    }
}
