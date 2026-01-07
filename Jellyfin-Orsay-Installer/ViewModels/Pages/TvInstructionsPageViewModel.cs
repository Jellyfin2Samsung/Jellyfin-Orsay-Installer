using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Orsay.Installer.Core;
using Jellyfin.Orsay.Installer.Models;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.ViewModels.Pages;

public sealed partial class TvInstructionsPageViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string _ipAddress = string.Empty;

    [ObservableProperty]
    private int _port = 80;

    [ObservableProperty]
    private TvSeries _selectedSeries = TvSeries.H;

    [ObservableProperty]
    private string? _detectedTvInfo;

    public ObservableCollection<TvSetupStep> Steps { get; } = new();

    public string IpAddressDisplay => Port == 80 ? IpAddress : $"{IpAddress}:{Port}";

    public bool IsSeriesESelected => SelectedSeries == TvSeries.E;
    public bool IsSeriesFSelected => SelectedSeries == TvSeries.F;
    public bool IsSeriesHSelected => SelectedSeries == TvSeries.H;

    public TvInstructionsPageViewModel(
        IDialogService dialogService,
        ILocalizationService localization)
        : base(localization)
    {
        _dialogService = dialogService;
        UpdateSteps();
    }

    protected override void OnLocalizationChanged()
    {
        UpdateSteps();
    }

    partial void OnIpAddressChanged(string value)
    {
        OnPropertyChanged(nameof(IpAddressDisplay));
        UpdateSteps();
    }

    partial void OnPortChanged(int value)
    {
        OnPropertyChanged(nameof(IpAddressDisplay));
        UpdateSteps();
    }

    partial void OnSelectedSeriesChanged(TvSeries value)
    {
        OnPropertyChanged(nameof(IsSeriesESelected));
        OnPropertyChanged(nameof(IsSeriesFSelected));
        OnPropertyChanged(nameof(IsSeriesHSelected));
        UpdateSteps();
    }

    [RelayCommand]
    private void SelectSeriesE() => SelectedSeries = TvSeries.E;

    [RelayCommand]
    private void SelectSeriesF() => SelectedSeries = TvSeries.F;

    [RelayCommand]
    private void SelectSeriesH() => SelectedSeries = TvSeries.H;

    [RelayCommand]
    private void ScanForTvs()
    {
        _dialogService.ShowTvScanner(
            IpAddress,
            onTvSelected: selectedTv =>
            {
                // Called when user clicks "Select" (dialog closes)
                if (selectedTv != null)
                {
                    ApplyDetectedTv(selectedTv);
                }
            },
            onBestTvFound: bestTv =>
            {
                // Called during scan when best TV found (dialog stays open)
                ApplyDetectedTv(bestTv);
            });
    }

    private void ApplyDetectedTv(DiscoveredTv tv)
    {
        var detectedSeries = DetectSeriesFromModel(tv.ModelName);
        if (detectedSeries.HasValue)
        {
            SelectedSeries = detectedSeries.Value;
        }

        DetectedTvInfo = string.IsNullOrEmpty(tv.ModelName)
            ? tv.IpAddress
            : $"{tv.ModelName} ({tv.IpAddress})";
    }

    /// <summary>
    /// Detects TV series from Samsung model name.
    /// Model format: UE[size][Series][model] e.g., UE40F6400, UE55H6400
    /// Series letters: E (2012), F (2013), H (2014), J (2015)
    /// </summary>
    private static TvSeries? DetectSeriesFromModel(string? modelName)
    {
        if (string.IsNullOrEmpty(modelName))
            return null;

        // Pattern: looks for series letter after size digits
        // Examples: UE40E5500, UE55F6400, UN40H5500, UE48J6300
        var match = Regex.Match(modelName, @"U[AEKN]\d{2}([EFHJ])", RegexOptions.IgnoreCase);
        if (!match.Success)
            return null;

        return match.Groups[1].Value.ToUpperInvariant() switch
        {
            "E" => TvSeries.E,
            "F" => TvSeries.F,
            "H" or "J" => TvSeries.H, // J series uses same instructions as H
            _ => null
        };
    }

    private void UpdateSteps()
    {
        Steps.Clear();

        var steps = SelectedSeries switch
        {
            TvSeries.E => GetSeriesESteps(),
            TvSeries.F => GetSeriesFSteps(),
            TvSeries.H => GetSeriesHSteps(),
            _ => GetSeriesHSteps()
        };

        foreach (var step in steps)
        {
            Steps.Add(step);
        }
    }

    private TvSetupStep[] GetSeriesESteps() =>
    [
        new(1, L["TvSetup.E.Step1.Title"], L["TvSetup.E.Step1.Desc"]),
        new(2, L["TvSetup.E.Step2.Title"], L["TvSetup.E.Step2.Desc"]),
        new(3, L["TvSetup.E.Step3.Title"], L["TvSetup.E.Step3.Desc"]),
        new(4, L["TvSetup.E.Step4.Title"], L.Format("TvSetup.E.Step4.Desc", IpAddressDisplay)),
        new(5, L["TvSetup.E.Step5.Title"], L["TvSetup.E.Step5.Desc"])
    ];

    private TvSetupStep[] GetSeriesFSteps() =>
    [
        new(1, L["TvSetup.F.Step1.Title"], L["TvSetup.F.Step1.Desc"]),
        new(2, L["TvSetup.F.Step2.Title"], L["TvSetup.F.Step2.Desc"]),
        new(3, L["TvSetup.F.Step3.Title"], L["TvSetup.F.Step3.Desc"]),
        new(4, L["TvSetup.F.Step4.Title"], L.Format("TvSetup.F.Step4.Desc", IpAddressDisplay)),
        new(5, L["TvSetup.F.Step5.Title"], L["TvSetup.F.Step5.Desc"]),
        new(6, L["TvSetup.F.Step6.Title"], L["TvSetup.F.Step6.Desc"])
    ];

    private TvSetupStep[] GetSeriesHSteps() =>
    [
        new(1, L["TvSetup.H.Step1.Title"], L["TvSetup.H.Step1.Desc"]),
        new(2, L["TvSetup.H.Step2.Title"], L["TvSetup.H.Step2.Desc"]),
        new(3, L["TvSetup.H.Step3.Title"], L["TvSetup.H.Step3.Desc"]),
        new(4, L["TvSetup.H.Step4.Title"], L.Format("TvSetup.H.Step4.Desc", IpAddressDisplay)),
        new(5, L["TvSetup.H.Step5.Title"], L["TvSetup.H.Step5.Desc"]),
        new(6, L["TvSetup.H.Step6.Title"], L["TvSetup.H.Step6.Desc"])
    ];
}
