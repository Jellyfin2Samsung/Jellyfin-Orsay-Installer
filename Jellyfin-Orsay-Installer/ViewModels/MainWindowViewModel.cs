using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Orsay.Installer.Models;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private static readonly Dictionary<string, LanguageInfo> LanguageData = new()
    {
        ["en"] = new("en", "English", "English", "🇬🇧"),
        ["nl"] = new("nl", "Nederlands", "Dutch", "🇳🇱"),
        ["ru"] = new("ru", "Русский", "Russian", "🇷🇺"),
    };

    private readonly ISettingsService _settings;

    public WizardViewModel Wizard { get; }

    public ObservableCollection<LanguageInfo> Languages { get; } = new();

    [ObservableProperty]
    private LanguageInfo _selectedLanguage = null!;

    partial void OnSelectedLanguageChanged(LanguageInfo value)
    {
        if (value is null) return;
        Localization.SetLanguage(value.Code);
        _settings.SaveLanguage(value.Code);
    }

    public MainWindowViewModel(
        ILocalizationService localization,
        ISettingsService settings,
        WizardViewModel wizard)
        : base(localization)
    {
        _settings = settings;
        Wizard = wizard;

        // Populate Languages from available languages
        foreach (var code in Localization.AvailableLanguages)
        {
            if (LanguageData.TryGetValue(code, out var info))
                Languages.Add(info);
            else
                Languages.Add(new LanguageInfo(code, code.ToUpper(), code.ToUpper(), "🌐"));
        }

        // Set selected language from settings
        var savedCode = _settings.LoadLanguage();
        _selectedLanguage = Languages.FirstOrDefault(l => l.Code == savedCode)
                            ?? Languages.First();
    }
}
