using Avalonia.Controls;
using Jellyfin.Orsay.Installer.Core;
using Jellyfin.Orsay.Installer.Models;
using Jellyfin.Orsay.Installer.Services.Abstractions;
using Jellyfin.Orsay.Installer.ViewModels.Pages;
using Xunit;

namespace Jellyfin.Orsay.Installer.Tests;

public sealed class TvInstructionsPageViewModelTests
{
    [Theory]
    [InlineData("UE40E5500", TvSeries.E)]
    [InlineData("UE39F5370SS", TvSeries.F)]
    [InlineData("UN40H5500", TvSeries.H)]
    [InlineData("UE48J6300", TvSeries.H)]
    public void DetectSeriesFromModel_ReturnsExpectedSeries(string modelName, TvSeries expected)
    {
        Assert.Equal(expected, TvInstructionsPageViewModel.DetectSeriesFromModel(modelName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("UE40D6500")]
    [InlineData("invalid-model")]
    public void DetectSeriesFromModel_ReturnsNull_ForUnsupportedOrUnknownModel(string? modelName)
    {
        Assert.Null(TvInstructionsPageViewModel.DetectSeriesFromModel(modelName));
    }

    [Fact]
    public void SeriesE_InstructionsUsePasswordAndSynchronizationTermsFromGuide()
    {
        var viewModel = CreateViewModel("192.168.1.20");

        viewModel.SelectedSeries = TvSeries.E;

        Assert.Collection(
            viewModel.Steps,
            step => Assert.Equal("Open Smart Hub", step.Title),
            step => Assert.Contains("'000000'", step.Description),
            step => Assert.Contains("Developer", step.Title),
            step => Assert.Contains("Setting Server IP", step.Description),
            step => Assert.Contains("User Application Synchronization", step.Description));
    }

    [Fact]
    public void SeriesF_InstructionsMatchMoreAppsFlow_ForDetectedIssueModel()
    {
        var viewModel = CreateViewModel("192.168.1.20");

        viewModel.SelectedSeries = TvSeries.F;

        Assert.Collection(
            viewModel.Steps,
            step => Assert.Equal("Open Samsung Account", step.Title),
            step => Assert.Contains("'develop' as email", step.Description),
            step => Assert.Contains("More Apps", step.Description),
            step => Assert.Contains("IP Settings", step.Description),
            step => Assert.Contains("Start App Sync", step.Description),
            step => Assert.Contains("restart your TV", step.Description, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SeriesH_InstructionsAllowPasswordFieldOrSkippedPassword()
    {
        var viewModel = CreateViewModel("192.168.1.20", 8096);

        viewModel.SelectedSeries = TvSeries.H;

        Assert.Collection(
            viewModel.Steps,
            step => Assert.Equal("Open Samsung Account", step.Title),
            step =>
            {
                Assert.Contains("'000000'", step.Description);
                Assert.Contains("Remember", step.Description);
            },
            step => Assert.Contains("hold the center", step.Description),
            step => Assert.Contains("192.168.1.20:8096", step.Description),
            step => Assert.Contains("Start User App Sync", step.Description),
            step => Assert.Contains("installation prompt", step.Description, StringComparison.OrdinalIgnoreCase));
    }

    private static TvInstructionsPageViewModel CreateViewModel(string ipAddress, int port = 80)
    {
        return new TvInstructionsPageViewModel(new FakeDialogService(), new FakeLocalizationService())
        {
            IpAddress = ipAddress,
            Port = port
        };
    }

    private sealed class FakeLocalizationService : ILocalizationService
    {
        private static readonly IReadOnlyDictionary<string, string> Strings = new Dictionary<string, string>
        {
            ["TvSetup.E.Step1.Title"] = "Open Smart Hub",
            ["TvSetup.E.Step1.Desc"] = "Press the Smart Hub button on your remote, then press the red A button to open the login menu.",
            ["TvSetup.E.Step2.Title"] = "Login as Developer",
            ["TvSetup.E.Step2.Desc"] = "Enter 'develop' as the Samsung Account ID. Enter '000000' in the password field, or leave it empty if your model accepts that. Press Sign In.",
            ["TvSetup.E.Step3.Title"] = "Open Developer Settings",
            ["TvSetup.E.Step3.Desc"] = "Press the Tools button on your remote, go to Settings, then select Developer.",
            ["TvSetup.E.Step4.Title"] = "Enter IP Address",
            ["TvSetup.E.Step4.Desc"] = "Select 'Setting Server IP' and enter: {0}",
            ["TvSetup.E.Step5.Title"] = "Sync Applications",
            ["TvSetup.E.Step5.Desc"] = "Select 'User Application Synchronization' to start the installation. The app will appear in Smart Hub.",
            ["TvSetup.F.Step1.Title"] = "Open Samsung Account",
            ["TvSetup.F.Step1.Desc"] = "Press Menu button, go to Smart Features, then select Samsung Account.",
            ["TvSetup.F.Step2.Title"] = "Login as Developer",
            ["TvSetup.F.Step2.Desc"] = "Enter 'develop' as email. Leave password empty (or try 'sso1029dev!' if needed). Press Sign In.",
            ["TvSetup.F.Step3.Title"] = "Open App Settings",
            ["TvSetup.F.Step3.Desc"] = "Press Smart Hub, go to More Apps (bottom right), then press Options.",
            ["TvSetup.F.Step4.Title"] = "Enter IP Address",
            ["TvSetup.F.Step4.Desc"] = "Select 'IP Settings' and enter: {0}",
            ["TvSetup.F.Step5.Title"] = "Sync Applications",
            ["TvSetup.F.Step5.Desc"] = "Select 'Start App Sync' to begin installation.",
            ["TvSetup.F.Step6.Title"] = "Restart if Needed",
            ["TvSetup.F.Step6.Desc"] = "If apps don't appear, restart your TV and check Smart Hub again.",
            ["TvSetup.H.Step1.Title"] = "Open Samsung Account",
            ["TvSetup.H.Step1.Desc"] = "Go to Smart Hub, select Samsung Account, then Sign In.",
            ["TvSetup.H.Step2.Title"] = "Login as Developer",
            ["TvSetup.H.Step2.Desc"] = "Enter 'develop' as login. Use '000000' if the password field is shown, or leave it empty if your model skips it. Check 'Remember' and press Sign In.",
            ["TvSetup.H.Step3.Title"] = "Open Sync Menu",
            ["TvSetup.H.Step3.Desc"] = "In Smart Hub, point your remote at any app and hold the center of the D-pad until a menu appears.",
            ["TvSetup.H.Step4.Title"] = "Enter IP Address",
            ["TvSetup.H.Step4.Desc"] = "Select 'IP Setting' and enter: {0} (confirm each number group with the D-pad center).",
            ["TvSetup.H.Step5.Title"] = "Start Sync",
            ["TvSetup.H.Step5.Desc"] = "Open the menu again and select 'Start User App Sync'.",
            ["TvSetup.H.Step6.Title"] = "Complete Installation",
            ["TvSetup.H.Step6.Desc"] = "Accept the installation prompt. Exit Smart Hub and re-enter to see the installed apps."
        };

        public string CurrentLanguage => "en";
        public IReadOnlyList<string> AvailableLanguages => ["en"];
        public event Action? LanguageChanged;

        public void SetLanguage(string languageCode)
        {
            LanguageChanged?.Invoke();
        }

        public string GetString(string key)
            => Strings.TryGetValue(key, out var value) ? value : key;

        public string GetString(string key, params object[] args)
            => string.Format(GetString(key), args);

        public string GetPluralString(string singularKey, string pluralKey, long count)
            => count == 1 ? GetString(singularKey) : GetString(pluralKey);

        public string GetPluralString(string singularKey, string pluralKey, long count, params object[] args)
            => string.Format(GetPluralString(singularKey, pluralKey, count), args);

        public string GetStringWithContext(string context, string key)
            => GetString(key);
    }

    private sealed class FakeDialogService : IDialogService
    {
        public Task<TResult?> ShowDialogAsync<TWindow, TResult>(Window owner) where TWindow : Window
            => Task.FromResult<TResult?>(default);

        public void ShowWindow<TWindow>() where TWindow : Window
        {
        }

        public void ShowLogViewer()
        {
        }

        public void ShowTvScanner(
            string localIpAddress,
            Action<DiscoveredTv?>? onTvSelected = null,
            Action<DiscoveredTv>? onBestTvFound = null)
        {
        }

        public Task ShowErrorAsync(string title, string message) => Task.CompletedTask;

        public Task ShowInfoAsync(string title, string message) => Task.CompletedTask;
    }
}
