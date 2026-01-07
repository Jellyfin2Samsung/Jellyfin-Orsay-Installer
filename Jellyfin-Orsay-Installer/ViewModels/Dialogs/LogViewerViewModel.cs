using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.ViewModels.Dialogs;

public sealed partial class LogViewerViewModel : ViewModelBase
{
    private readonly ILogService _logService;
    private readonly IClipboardService _clipboard;

    public ObservableCollection<LogEntry> Entries { get; } = new();

    public LogViewerViewModel(
        ILogService logService,
        IClipboardService clipboard,
        ILocalizationService localization)
        : base(localization)
    {
        _logService = logService;
        _clipboard = clipboard;

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
    private Task CopyAllAsync() => _clipboard.SetTextAsync(_logService.GetAllLogsAsText());
}
