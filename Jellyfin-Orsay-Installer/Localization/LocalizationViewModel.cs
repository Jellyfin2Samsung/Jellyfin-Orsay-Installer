using System.ComponentModel;
using System.Runtime.CompilerServices;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.Localization;

/// <summary>
/// ViewModel that provides localized strings for data binding.
/// Supports simple key lookup, formatted strings, and pluralization.
/// </summary>
public sealed class LocalizationViewModel : INotifyPropertyChanged
{
    private readonly ILocalizationService _localization;

    public event PropertyChangedEventHandler? PropertyChanged;

    public LocalizationViewModel(ILocalizationService localization)
    {
        _localization = localization;
        _localization.LanguageChanged += OnLanguageChanged;
    }

    /// <summary>
    /// Gets a localized string by key.
    /// Usage in XAML: {Binding L[Key]}
    /// </summary>
    public string this[string key] => _localization.GetString(key);

    /// <summary>
    /// Gets a formatted localized string.
    /// Usage in ViewModel: L.Format("Key", arg1, arg2)
    /// </summary>
    public string Format(string key, params object[] args)
        => _localization.GetString(key, args);

    /// <summary>
    /// Gets a pluralized string.
    /// Usage in ViewModel: L.Plural("singular", "plural", count)
    /// </summary>
    public string Plural(string singularKey, string pluralKey, long count)
        => _localization.GetPluralString(singularKey, pluralKey, count);

    /// <summary>
    /// Gets a pluralized string with format arguments.
    /// </summary>
    public string Plural(string singularKey, string pluralKey, long count, params object[] args)
        => _localization.GetPluralString(singularKey, pluralKey, count, args);

    private void OnLanguageChanged()
    {
        // Refresh ALL bindings by raising PropertyChanged with empty string
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    /// <summary>
    /// Manually trigger refresh of all localized strings.
    /// </summary>
    public void Refresh() => OnLanguageChanged();
}
