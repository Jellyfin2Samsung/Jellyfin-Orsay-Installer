using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Jellyfin.Orsay.Installer.Services.Abstractions;
using Jellyfin.Orsay.Installer.ViewModels;
using Jellyfin.Orsay.Installer.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Orsay.Installer;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public static IServiceProvider Services => ((App)Current!)._serviceProvider!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Setup DI container
        var services = new ServiceCollection();
        services.AddAppServices();
        _serviceProvider = services.BuildServiceProvider();

        // Load the language early so it's ready before the UI
        var settings = _serviceProvider.GetRequiredService<ISettingsService>();
        var localization = _serviceProvider.GetRequiredService<ILocalizationService>();
        localization.SetLanguage(settings.LoadLanguage());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var mainWindowViewModel = _serviceProvider!.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel,
            };
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
