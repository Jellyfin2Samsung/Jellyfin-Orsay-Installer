using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Orsay.Installer.Models;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.ViewModels.Dialogs;

public sealed partial class TvScannerViewModel : ViewModelBase, IDisposable
{
    private readonly ITvDiscoveryService _discoveryService;
    private readonly ILogService _logService;
    private CancellationTokenSource? _cts;
    private TvDiscoveryProgress? _lastProgress;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private int _progressPercent;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private DiscoveredTv? _selectedTv;

    [ObservableProperty]
    private bool _hasResults;

    /// <summary>
    /// Collection of discovered TVs.
    /// </summary>
    public ObservableCollection<DiscoveredTv> DiscoveredTvs { get; } = new();

    /// <summary>
    /// Local IP address to use for discovery. Must be set before scanning.
    /// </summary>
    public string LocalIpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Event raised when a TV is selected and the dialog should close.
    /// </summary>
    public event Action<DiscoveredTv?>? TvSelected;

    /// <summary>
    /// Event raised when a new TV with best confidence is discovered.
    /// Used for auto-selecting series in main window without closing dialog.
    /// </summary>
    public event Action<DiscoveredTv>? BestTvFound;

    /// <summary>
    /// Event raised when the dialog should be closed.
    /// </summary>
    public event Action? CloseRequested;

    public TvScannerViewModel(
        ITvDiscoveryService discoveryService,
        ILogService logService,
        ILocalizationService localization)
        : base(localization)
    {
        _discoveryService = discoveryService;
        _logService = logService;

        // Subscribe to discovery events
        _discoveryService.TvDiscovered += OnTvDiscovered;
        _discoveryService.ProgressChanged += OnProgressChanged;
    }

    [RelayCommand(CanExecute = nameof(CanStartScan))]
    private async Task StartScanAsync()
    {
        if (string.IsNullOrEmpty(LocalIpAddress))
        {
            StatusMessage = L["Discovery.NoInterface"];
            return;
        }

        IsScanning = true;
        DiscoveredTvs.Clear();
        HasResults = false;
        SelectedTv = null;

        _cts = new CancellationTokenSource();

        try
        {
            _logService.Log($"Starting TV discovery from {LocalIpAddress}");
            var result = await _discoveryService.DiscoverAsync(LocalIpAddress, cancellationToken: _cts.Token);

            if (result.IsSuccess)
            {
                HasResults = DiscoveredTvs.Count > 0;
                StatusMessage = HasResults
                    ? L.Format("Discovery.Found", DiscoveredTvs.Count)
                    : L["Discovery.NoTvsFound"];
            }
            else
            {
                StatusMessage = result.Error ?? L["Discovery.Error"];
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = L["Discovery.Cancelled"];
        }
        finally
        {
            IsScanning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private bool CanStartScan() => !IsScanning;

    [RelayCommand(CanExecute = nameof(CanCancelScan))]
    private void CancelScan()
    {
        _cts?.Cancel();
    }

    private bool CanCancelScan() => IsScanning;

    [RelayCommand(CanExecute = nameof(CanSelectTv))]
    private void SelectTv()
    {
        TvSelected?.Invoke(SelectedTv);
        CloseRequested?.Invoke();
    }

    private bool CanSelectTv() => SelectedTv != null;

    [RelayCommand]
    private void Close()
    {
        _cts?.Cancel();
        TvSelected?.Invoke(null);
        CloseRequested?.Invoke();
    }

    private void OnTvDiscovered(DiscoveredTv tv)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            // Check if TV with same IP already exists
            var existing = DiscoveredTvs.FirstOrDefault(t => t.IpAddress == tv.IpAddress);
            if (existing != null)
            {
                var index = DiscoveredTvs.IndexOf(existing);
                DiscoveredTvs[index] = tv;
            }
            else
            {
                DiscoveredTvs.Add(tv);
            }
            HasResults = DiscoveredTvs.Count > 0;

            // Track best TV by confidence score and notify main window
            var currentBest = DiscoveredTvs.MaxBy(t => t.ConfidenceScore);
            if (currentBest != null && (SelectedTv == null || currentBest.ConfidenceScore > SelectedTv.ConfidenceScore))
            {
                SelectedTv = currentBest;
                BestTvFound?.Invoke(currentBest);
            }
        });
    }

    private void OnProgressChanged(TvDiscoveryProgress progress)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            _lastProgress = progress;
            ProgressPercent = progress.ProgressPercent;
            StatusMessage = GetLocalizedPhaseMessage(progress);
        });
    }

    protected override void OnLocalizationChanged()
    {
        // Re-evaluate status message with new language
        if (_lastProgress != null)
        {
            StatusMessage = GetLocalizedPhaseMessage(_lastProgress);
        }
    }

    private string GetLocalizedPhaseMessage(TvDiscoveryProgress progress)
    {
        return progress.Phase switch
        {
            TvDiscoveryPhase.Starting => L["Discovery.Phase.Starting"],
            TvDiscoveryPhase.SsdpBroadcast => L["Discovery.Phase.Ssdp"],
            TvDiscoveryPhase.SsdpListening => L["Discovery.Phase.Ssdp"],
            TvDiscoveryPhase.PortProbing => string.IsNullOrEmpty(progress.CurrentIp)
                ? L["Discovery.Phase.PortScan"]
                : L.Format("Discovery.Phase.PortScanIp", progress.CurrentIp),
            TvDiscoveryPhase.FetchingDeviceInfo => L["Discovery.Phase.FetchingInfo"],
            TvDiscoveryPhase.Completed => L.Format("Discovery.Found", progress.TvsFound),
            TvDiscoveryPhase.Cancelled => L["Discovery.Cancelled"],
            _ => progress.Message
        };
    }

    partial void OnSelectedTvChanged(DiscoveredTv? value)
    {
        SelectTvCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        _discoveryService.TvDiscovered -= OnTvDiscovered;
        _discoveryService.ProgressChanged -= OnProgressChanged;
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
