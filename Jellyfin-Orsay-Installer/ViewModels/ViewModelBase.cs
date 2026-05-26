using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Orsay.Installer.Localization;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.ViewModels;

/// <summary>
/// Base class for all ViewModels in the application.
/// Provides shared localization support via the L property.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    /// <summary>
    /// Localization helper for XAML bindings.
    /// Usage: {Binding L[Key]} or L.Format("Key", args) in code.
    /// </summary>
    public LocalizationViewModel L { get; }

    /// <summary>
    /// Direct access to localization service for advanced scenarios.
    /// </summary>
    protected ILocalizationService Localization { get; }

    protected ViewModelBase(ILocalizationService localization)
    {
        Localization = localization;
        L = new LocalizationViewModel(localization);
        localization.LanguageChanged += OnLocalizationChanged;
    }

    /// <summary>
    /// Override to handle language changes for computed properties
    /// that use L.Format() or conditional logic.
    /// </summary>
    protected virtual void OnLocalizationChanged()
    {
        // Default: no action needed if all strings use L[Key] in XAML
    }
}
