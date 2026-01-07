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
        ["de"] = new("de", "Deutsch", "German", "🇩🇪"),
        ["fr"] = new("fr", "Français", "French", "🇫🇷"),
        ["es"] = new("es", "Español", "Spanish", "🇪🇸"),
        ["pt"] = new("pt", "Português", "Portuguese", "🇵🇹"),
        ["pl"] = new("pl", "Polski", "Polish", "🇵🇱"),
        ["it"] = new("it", "Italiano", "Italian", "🇮🇹"),
        ["uk"] = new("uk", "Українська", "Ukrainian", "🇺🇦"),
        ["zh-CN"] = new("zh-CN", "简体中文", "Chinese (Simplified)", "🇨🇳"),
        ["tr"] = new("tr", "Türkçe", "Turkish", "🇹🇷"),
        ["sv"] = new("sv", "Svenska", "Swedish", "🇸🇪"),
        ["ko"] = new("ko", "한국어", "Korean", "🇰🇷"),
        ["ja"] = new("ja", "日本語", "Japanese", "🇯🇵"),
        ["th"] = new("th", "ไทย", "Thai", "🇹🇭"),
        ["vi"] = new("vi", "Tiếng Việt", "Vietnamese", "🇻🇳"),
        ["da"] = new("da", "Dansk", "Danish", "🇩🇰"),
        ["no"] = new("no", "Norsk", "Norwegian", "🇳🇴"),
        ["fi"] = new("fi", "Suomi", "Finnish", "🇫🇮"),
        ["cs"] = new("cs", "Čeština", "Czech", "🇨🇿"),
        ["hu"] = new("hu", "Magyar", "Hungarian", "🇭🇺"),
        ["ro"] = new("ro", "Română", "Romanian", "🇷🇴"),
        ["el"] = new("el", "Ελληνικά", "Greek", "🇬🇷"),
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
