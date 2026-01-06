namespace Jellyfin.Orsay.Installer.Services.Abstractions;

/// <summary>
/// Service for persisting application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the saved language code, or default if not set.
    /// </summary>
    string LoadLanguage();

    /// <summary>
    /// Saves the language code.
    /// </summary>
    void SaveLanguage(string languageCode);

    /// <summary>
    /// Gets the last used server port, or default if not set.
    /// </summary>
    int LoadPort();

    /// <summary>
    /// Saves the server port.
    /// </summary>
    void SavePort(int port);

    /// <summary>
    /// Gets the last used IP address, or null if not set.
    /// </summary>
    string? LoadLastIpAddress();

    /// <summary>
    /// Saves the last used IP address.
    /// </summary>
    void SaveLastIpAddress(string ipAddress);
}
