using System;
using System.Collections.Generic;

namespace Jellyfin.Orsay.Installer.Services.Abstractions;

/// <summary>
/// Service for multi-language support using GNU GetText format.
/// Supports pluralization and context-based translations.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the current language code (e.g., "en", "nl", "ru").
    /// </summary>
    string CurrentLanguage { get; }

    /// <summary>
    /// Gets the list of available language codes.
    /// Languages are auto-detected from Localization/{lang}/LC_MESSAGES/messages.po files.
    /// </summary>
    IReadOnlyList<string> AvailableLanguages { get; }

    /// <summary>
    /// Event raised when the language changes.
    /// </summary>
    event Action? LanguageChanged;

    /// <summary>
    /// Sets the current language.
    /// </summary>
    /// <param name="languageCode">ISO language code (e.g., "en", "ru", "nl")</param>
    void SetLanguage(string languageCode);

    /// <summary>
    /// Gets a localized string by key (msgid).
    /// </summary>
    /// <param name="key">The message key (msgid in PO file)</param>
    /// <returns>Translated string, or the key if not found</returns>
    string GetString(string key);

    /// <summary>
    /// Gets a localized string by key with format arguments.
    /// </summary>
    /// <param name="key">The message key (msgid in PO file)</param>
    /// <param name="args">Format arguments for string.Format</param>
    /// <returns>Formatted translated string</returns>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Gets a pluralized string based on count.
    /// Uses the language's plural rules (e.g., Russian has 3 forms).
    /// </summary>
    /// <param name="singularKey">The singular form key (msgid)</param>
    /// <param name="pluralKey">The plural form key (msgid_plural)</param>
    /// <param name="count">The count to determine which plural form to use</param>
    /// <returns>The appropriate plural form</returns>
    string GetPluralString(string singularKey, string pluralKey, long count);

    /// <summary>
    /// Gets a pluralized string with format arguments.
    /// </summary>
    /// <param name="singularKey">The singular form key (msgid)</param>
    /// <param name="pluralKey">The plural form key (msgid_plural)</param>
    /// <param name="count">The count to determine which plural form to use</param>
    /// <param name="args">Format arguments for string.Format</param>
    /// <returns>Formatted plural string</returns>
    string GetPluralString(string singularKey, string pluralKey, long count, params object[] args);

    /// <summary>
    /// Gets a string with context for disambiguating identical source strings.
    /// </summary>
    /// <param name="context">Context for the string (msgctxt in PO file)</param>
    /// <param name="key">The message key (msgid)</param>
    /// <returns>The translated string with context</returns>
    string GetStringWithContext(string context, string key);
}
