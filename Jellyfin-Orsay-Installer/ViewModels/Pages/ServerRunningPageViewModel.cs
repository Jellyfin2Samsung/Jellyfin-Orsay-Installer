using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Orsay.Installer.Models;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.ViewModels.Pages;

public sealed partial class ServerRunningPageViewModel : ViewModelBase, IAsyncDisposable
{
    private readonly IOrsayServer _server;
    private readonly IOrsayPackager _packager;
    private readonly ILogService _logService;

    [ObservableProperty]
    private ServerStatus _status = ServerStatus.Stopped;

    [ObservableProperty]
    private bool _isInstallDetected;

    public ObservableCollection<ServerRequest> RecentRequests { get; } = new();

    [ObservableProperty]
    private string _serverUrl = string.Empty;

    [ObservableProperty]
    private string _ipAddress = string.Empty;

    [ObservableProperty]
    private int _port = 80;

    public event Action? InstallationCompleted;

    public ServerRunningPageViewModel(
        IOrsayServer server,
        IOrsayPackager packager,
        ILogService logService,
        ILocalizationService localization)
        : base(localization)
    {
        _server = server;
        _packager = packager;
        _logService = logService;

        _server.OnRequest += HandleRequest;
        _server.OnLog += msg => _logService.Log(msg);
    }

    public async Task StartAsync()
    {
        var outputPath = _packager.GetDefaultOutputPath();
        var result = _packager.BuildWidget(outputPath, "Jellyfin", IpAddress, Port);

        if (result.IsFailure)
        {
            _logService.Log($"Failed to build widget: {result.Error}", LogLevel.Error);
            return;
        }

        _logService.Log($"Widget packaged: {result.Value!.WidgetId} ({result.Value.ZipSize:N0} bytes)");
        ServerUrl = result.Value.DownloadUrl;

        await _server.StartAsync(outputPath, IpAddress, Port);
        Status = Status with { IsRunning = true, ServerUrl = ServerUrl };
    }

    private void HandleRequest(ServerRequest request)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            RecentRequests.Add(request);
            if (RecentRequests.Count > 50)
                RecentRequests.RemoveAt(0);

            var newStatus = Status with
            {
                RequestCount = Status.RequestCount + 1,
                LastRequestPath = request.Path,
                WidgetListRequested = Status.WidgetListRequested || request.Path.EndsWith("widgetlist.xml", StringComparison.OrdinalIgnoreCase),
                WidgetDownloaded = Status.WidgetDownloaded || request.Path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
            };

            Status = newStatus;

            if (newStatus.IsInstallationComplete && !IsInstallDetected)
            {
                IsInstallDetected = true;
                _logService.Log("Installation detected!");
                InstallationCompleted?.Invoke();
            }
        });
    }

    [RelayCommand]
    private async Task StopServerAsync()
    {
        await _server.StopAsync();
        Status = ServerStatus.Stopped;
    }

    public async ValueTask DisposeAsync()
    {
        _server.OnRequest -= HandleRequest;
        await _server.DisposeAsync();
    }
}
