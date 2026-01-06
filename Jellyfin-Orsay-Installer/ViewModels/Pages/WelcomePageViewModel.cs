using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.ViewModels.Pages;

public sealed class WelcomePageViewModel : ViewModelBase
{
    public WelcomePageViewModel(ILocalizationService localization)
        : base(localization)
    {
    }
}
