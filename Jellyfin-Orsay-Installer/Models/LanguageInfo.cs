namespace Jellyfin.Orsay.Installer.Models;

/// <summary>
/// Represents language metadata for the language selector.
/// </summary>
/// <param name="Code">ISO language code (e.g., "ru")</param>
/// <param name="NativeName">Name in the language itself (e.g., "Русский")</param>
/// <param name="EnglishName">English name (e.g., "Russian")</param>
/// <param name="FlagEmoji">Country flag emoji (e.g., "🇷🇺")</param>
public sealed record LanguageInfo(
    string Code,
    string NativeName,
    string EnglishName,
    string FlagEmoji
)
{
    /// <summary>
    /// Uppercase language code for display (e.g., "RU").
    /// </summary>
    public string CodeUpper => Code.ToUpperInvariant();
}
