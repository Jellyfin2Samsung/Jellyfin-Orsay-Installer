using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Orsay.Installer.Models;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.ViewModels.Pages;

public sealed partial class TvInstructionsPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _ipAddress = string.Empty;

    [ObservableProperty]
    private int _port = 80;

    [ObservableProperty]
    private TvSeries _selectedSeries = TvSeries.H;

    public ObservableCollection<TvSetupStep> Steps { get; } = new();

    public string IpAddressDisplay => Port == 80 ? IpAddress : $"{IpAddress}:{Port}";

    public bool IsSeriesESelected => SelectedSeries == TvSeries.E;
    public bool IsSeriesFSelected => SelectedSeries == TvSeries.F;
    public bool IsSeriesHSelected => SelectedSeries == TvSeries.H;

    public TvInstructionsPageViewModel(ILocalizationService localization)
        : base(localization)
    {
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
