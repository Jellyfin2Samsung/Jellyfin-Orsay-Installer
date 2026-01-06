using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.ViewModels.Dialogs;

public sealed partial class LogViewerViewModel : ViewModelBase
{
    private readonly ILogService _logService;

    public ObservableCollection<LogEntry> Entries { get; } = new();

    public LogViewerViewModel(ILogService logService, ILocalizationService localization)
        : base(localization)
    {
        _logService = logService;

        // Load existing entries
        foreach (var entry in _logService.Entries)
        {
            Entries.Add(entry);
        }

        // Subscribe to new entries
        _logService.LogAdded += OnLogAdded;
    }

    private void OnLogAdded(LogEntry entry)
    {
        // Ensure we're on the UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() => Entries.Add(entry));
    }

    [RelayCommand]
    private void Clear()
    {
        _logService.Clear();
        Entries.Clear();
    }

    [RelayCommand]
    private async Task CopyAllAsync()
    {
        var text = _logService.GetAllLogsAsText();

        // Get clipboard from the main window's top level
        if (Avalonia.Application.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow != null)
        {
            var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(desktop.MainWindow);
            var clipboard = topLevel?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(text);
            }
        }
    }
}
