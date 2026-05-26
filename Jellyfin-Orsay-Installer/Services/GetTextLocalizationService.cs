using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Karambolo.PO;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.Services;

/// <summary>
/// Localization service using GNU GetText format with Karambolo.PO library.
/// Supports pluralization and standard .po file format for easy translations.
/// </summary>
public sealed class GetTextLocalizationService : ILocalizationService
{
    private POCatalog? _catalog;
    private string _currentLanguage = "en";
    private readonly string _localizationPath;
    private readonly List<string> _availableLanguages = new();

    public string CurrentLanguage => _currentLanguage;
    public IReadOnlyList<string> AvailableLanguages => _availableLanguages;

    public event Action? LanguageChanged;

    public GetTextLocalizationService()
    {
        _localizationPath = Path.Combine(AppContext.BaseDirectory, "Localization");

        DetectAvailableLanguages();
        SetLanguage("en");
    }

    private void DetectAvailableLanguages()
    {
        _availableLanguages.Clear();

        if (!Directory.Exists(_localizationPath))
        {
            _availableLanguages.Add("en");
            return;
        }

        // Find all language directories that contain messages.po
        foreach (var dir in Directory.GetDirectories(_localizationPath))
        {
            var langCode = Path.GetFileName(dir);
            var poFile = Path.Combine(dir, "LC_MESSAGES", "messages.po");

            if (File.Exists(poFile))
            {
                _availableLanguages.Add(langCode);
            }
        }

        // Ensure English is always available
        if (!_availableLanguages.Contains("en"))
        {
            _availableLanguages.Insert(0, "en");
        }
        else
        {
            // Move English to the beginning
            _availableLanguages.Remove("en");
            _availableLanguages.Insert(0, "en");
        }
    }

    public void SetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            languageCode = "en";

        var poPath = Path.Combine(_localizationPath, languageCode, "LC_MESSAGES", "messages.po");

        if (File.Exists(poPath))
        {
            try
            {
                var parser = new POParser();
                using var stream = File.OpenRead(poPath);
                var result = parser.Parse(stream);

                if (result.Success)
                {
                    _catalog = result.Catalog;
                    _currentLanguage = languageCode;
                }
                else
                {
                    // If failed to parse, fallback to English
                    if (languageCode != "en")
                    {
                        SetLanguage("en");
                        return;
                    }
                    // If English also fails, use null catalog (keys will be returned as-is)
                    _catalog = null;
                    _currentLanguage = "en";
                }
            }
            catch (Exception)
            {
                // If failed to load, fallback to English
                if (languageCode != "en")
                {
                    SetLanguage("en");
                    return;
                }
                // If English also fails, use null catalog
                _catalog = null;
                _currentLanguage = "en";
            }
        }
        else
        {
            // Language file not found, try fallback to English
            if (languageCode != "en")
            {
                SetLanguage("en");
                return;
            }
            // If even English is not found, use null catalog (keys will be returned as-is)
            _catalog = null;
            _currentLanguage = "en";
        }

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(languageCode);
        }
        catch
        {
            // Ignore culture setting errors
        }

        LanguageChanged?.Invoke();
    }

    public string GetString(string key)
    {
        if (_catalog == null)
            return key;

        var poKey = new POKey(key);
        var translation = _catalog.GetTranslation(poKey);

        return !string.IsNullOrEmpty(translation) ? translation : key;
    }

    public string GetString(string key, params object[] args)
    {
        var template = GetString(key);
        try
        {
            return string.Format(template, args);
        }
        catch
        {
            return template;
        }
    }

    /// <summary>
    /// Gets a pluralized string based on count.
    /// </summary>
    /// <param name="singularKey">The key for singular form (also used as msgid)</param>
    /// <param name="pluralKey">The key for plural form (used as msgid_plural)</param>
    /// <param name="count">The count to determine which form to use</param>
    /// <returns>The appropriate plural form</returns>
    public string GetPluralString(string singularKey, string pluralKey, long count)
    {
        if (_catalog == null)
            return count == 1 ? singularKey : pluralKey;

        var poKey = new POKey(singularKey, pluralKey);
        var translation = _catalog.GetTranslation(poKey, (int)count);

        return !string.IsNullOrEmpty(translation) ? translation : (count == 1 ? singularKey : pluralKey);
    }

    /// <summary>
    /// Gets a pluralized string with format arguments.
    /// </summary>
    public string GetPluralString(string singularKey, string pluralKey, long count, params object[] args)
    {
        var template = GetPluralString(singularKey, pluralKey, count);
        try
        {
            return string.Format(template, args);
        }
        catch
        {
            return template;
        }
    }

    /// <summary>
    /// Gets a string with context (for disambiguating identical source strings).
    /// </summary>
    /// <param name="context">Context for the string (msgctxt in PO file)</param>
    /// <param name="key">The message key (msgid)</param>
    /// <returns>The translated string</returns>
    public string GetStringWithContext(string context, string key)
    {
        if (_catalog == null)
            return key;

        var poKey = new POKey(key, contextId: context);
        var translation = _catalog.GetTranslation(poKey);

        return !string.IsNullOrEmpty(translation) ? translation : key;
    }
}
