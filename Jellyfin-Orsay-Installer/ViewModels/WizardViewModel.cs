using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Orsay.Installer.Core;
using Jellyfin.Orsay.Installer.Services.Abstractions;
using Jellyfin.Orsay.Installer.ViewModels.Pages;

namespace Jellyfin.Orsay.Installer.ViewModels;

/// <summary>
/// Orchestrates the wizard flow between pages.
/// </summary>
public sealed partial class WizardViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private readonly ISettingsService _settings;

    // Shared state between pages
    private string _selectedIpAddress = string.Empty;
    private int _selectedPort = 80;
    private ServerRunningPageViewModel? _serverPage;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private int _currentStep = 1;

    public int TotalSteps => 5;

    public string StepIndicator => L.Format("Wizard.Step", CurrentStep, TotalSteps);

    public bool CanGoBack => CurrentStep > 1 && CurrentStep < 5;
    public bool CanGoNext => CurrentStep < 4; // Not on server running or completed pages
    public bool IsFirstStep => CurrentStep == 1;

    public WizardViewModel(
        INavigationService navigation,
        IDialogService dialog,
        ILocalizationService localization,
        ISettingsService settings)
        : base(localization)
    {
        _navigation = navigation;
        _dialog = dialog;
        _settings = settings;

        _navigation.Navigated += page =>
        {
            CurrentPage = page;
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(IsFirstStep));
            OnPropertyChanged(nameof(StepIndicator));
        };

        // Start at welcome page
        _navigation.NavigateTo<WelcomePageViewModel>();
    }

    protected override void OnLocalizationChanged()
    {
        OnPropertyChanged(nameof(StepIndicator));
    }

    [RelayCommand]
    private void GoBack()
    {
        if (!CanGoBack) return;

        CurrentStep--;
        NavigateToStep(CurrentStep);
    }

    [RelayCommand]
    private async Task GoNextAsync()
    {
        if (!CanGoNext) return;

        // Save settings from current page before moving
        if (CurrentStep == 2 && CurrentPage is NetworkSetupPageViewModel networkPage)
        {
            if (networkPage.SelectedInterface != null)
            {
                _selectedIpAddress = networkPage.SelectedInterface.IpAddress;
                _selectedPort = networkPage.Port;
            }
        }

        CurrentStep++;
        await NavigateToStepAsync(CurrentStep);
    }

    [RelayCommand]
    private void OpenLogs()
    {
        _dialog.ShowLogViewer();
    }

    private async Task NavigateToStepAsync(int step)
    {
        CurrentStep = step;

        switch (step)
        {
            case 1:
                _navigation.NavigateTo<WelcomePageViewModel>();
                break;

            case 2:
                _navigation.NavigateTo<NetworkSetupPageViewModel>();
                break;

            case 3:
                _navigation.NavigateTo<TvInstructionsPageViewModel>(vm =>
                {
                    vm.IpAddress = _selectedIpAddress;
                    vm.Port = _selectedPort;
                });
                break;

            case 4:
                _navigation.NavigateTo<ServerRunningPageViewModel>(vm =>
                {
                    vm.IpAddress = _selectedIpAddress;
                    vm.Port = _selectedPort;
                    vm.InstallationCompleted += OnInstallationCompleted;
                    _serverPage = vm;
                });

                // Start the server after navigation
                if (_serverPage != null)
                {
                    await _serverPage.StartAsync();
                }
                break;

            case 5:
                _navigation.NavigateTo<CompletedPageViewModel>();
                break;
        }

        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(StepIndicator));
    }

    public void NavigateToStep(int step)
    {
        _ = NavigateToStepAsync(step);
    }

    private void OnInstallationCompleted()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            NavigateToStep(5);
        });
    }
}
