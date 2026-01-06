using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.ViewModels.Pages;

public sealed partial class CompletedPageViewModel : ViewModelBase
{
    public CompletedPageViewModel(ILocalizationService localization)
        : base(localization)
    {
    }

    [RelayCommand]
    private void BuyMeABeer()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "https://ko-fi.com/patrickst",
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}
