using Jellyfin.Orsay.Installer.Core;
using Jellyfin.Orsay.Installer.Services;
using Jellyfin.Orsay.Installer.Services.Abstractions;
using Jellyfin.Orsay.Installer.ViewModels;
using Jellyfin.Orsay.Installer.ViewModels.Dialogs;
using Jellyfin.Orsay.Installer.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Orsay.Installer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // Core services (Singleton)
        services.AddSingleton<ILocalizationService, GetTextLocalizationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ITvDiscoveryService, SsdpTvDiscoveryService>();
        services.AddSingleton<IClipboardService, ClipboardService>();

        // Transient services (new instance each time)
        services.AddTransient<IOrsayPackager, OrsayPackager>();
        services.AddTransient<IOrsayServer, KestrelOrsayServer>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<WizardViewModel>();

        // Page ViewModels (Transient - created fresh for each navigation)
        services.AddTransient<WelcomePageViewModel>();
        services.AddTransient<NetworkSetupPageViewModel>();
        services.AddTransient<TvInstructionsPageViewModel>();
        services.AddTransient<ServerRunningPageViewModel>();
        services.AddTransient<CompletedPageViewModel>();

        // Dialog ViewModels
        services.AddTransient<LogViewerViewModel>();
        services.AddTransient<TvScannerViewModel>();

        return services;
    }
}
