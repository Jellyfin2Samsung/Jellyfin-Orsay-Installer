using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Jellyfin.Orsay.Installer.Core;
using Jellyfin.Orsay.Installer.Models;
using Jellyfin.Orsay.Installer.ViewModels.Dialogs;
using Jellyfin.Orsay.Installer.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Orsay.Installer.Services;

public sealed class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;
    private LogViewerWindow? _logViewerWindow;
    private TvScannerWindow? _tvScannerWindow;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult?> ShowDialogAsync<TWindow, TResult>(Window owner) where TWindow : Window
    {
        var window = _serviceProvider.GetRequiredService<TWindow>();
        return await window.ShowDialog<TResult?>(owner);
    }

    public void ShowWindow<TWindow>() where TWindow : Window
    {
        var window = _serviceProvider.GetRequiredService<TWindow>();
        window.Show();
    }

    public void ShowLogViewer()
    {
        if (_logViewerWindow != null && _logViewerWindow.IsVisible)
        {
            _logViewerWindow.Activate();
            return;
        }

        var viewModel = _serviceProvider.GetRequiredService<LogViewerViewModel>();
        _logViewerWindow = new LogViewerWindow { DataContext = viewModel };
        _logViewerWindow.Closed += (_, _) => _logViewerWindow = null;

        var mainWindow = GetMainWindow();
        if (mainWindow != null)
        {
            _logViewerWindow.Show(mainWindow);
        }
        else
        {
            _logViewerWindow.Show();
        }
    }

    public void ShowTvScanner(
        string localIpAddress,
        Action<DiscoveredTv?>? onTvSelected = null,
        Action<DiscoveredTv>? onBestTvFound = null)
    {
        if (_tvScannerWindow != null && _tvScannerWindow.IsVisible)
        {
            _tvScannerWindow.Activate();
            return;
        }

        var viewModel = _serviceProvider.GetRequiredService<TvScannerViewModel>();
        viewModel.LocalIpAddress = localIpAddress;

        if (onTvSelected != null)
        {
            viewModel.TvSelected += onTvSelected;
        }

        if (onBestTvFound != null)
        {
            viewModel.BestTvFound += onBestTvFound;
        }

        _tvScannerWindow = new TvScannerWindow { DataContext = viewModel };
        _tvScannerWindow.Closed += (_, _) => _tvScannerWindow = null;

        var mainWindow = GetMainWindow();
        if (mainWindow != null)
        {
            _tvScannerWindow.Show(mainWindow);
        }
        else
        {
            _tvScannerWindow.Show();
        }
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null) return;

        // Simple message box implementation using Avalonia
        // In a real app, you'd use a proper dialog library
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    new Button { Content = "OK", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(0, 20, 0, 0) }
                }
            }
        };

        var button = ((StackPanel)dialog.Content).Children[1] as Button;
        button!.Click += (_, _) => dialog.Close();

        await dialog.ShowDialog(mainWindow);
    }

    public async Task ShowInfoAsync(string title, string message)
    {
        await ShowErrorAsync(title, message);
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}
