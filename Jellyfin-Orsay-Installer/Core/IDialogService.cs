using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Jellyfin.Orsay.Installer.Models;

namespace Jellyfin.Orsay.Installer.Core;

/// <summary>
/// Service for displaying dialogs and windows.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a modal dialog window.
    /// </summary>
    Task<TResult?> ShowDialogAsync<TWindow, TResult>(Window owner) where TWindow : Window;

    /// <summary>
    /// Shows a non-modal window.
    /// </summary>
    void ShowWindow<TWindow>() where TWindow : Window;

    /// <summary>
    /// Shows the log viewer window.
    /// </summary>
    void ShowLogViewer();

    /// <summary>
    /// Shows the TV scanner window.
    /// </summary>
    /// <param name="localIpAddress">Local IP address to use for discovery.</param>
    /// <param name="onTvSelected">Callback when user confirms TV selection (dialog closes).</param>
    /// <param name="onBestTvFound">Callback during scan when TV with highest confidence is found (dialog stays open).</param>
    void ShowTvScanner(
        string localIpAddress,
        Action<DiscoveredTv?>? onTvSelected = null,
        Action<DiscoveredTv>? onBestTvFound = null);

    /// <summary>
    /// Shows an error message dialog.
    /// </summary>
    Task ShowErrorAsync(string title, string message);

    /// <summary>
    /// Shows an information message dialog.
    /// </summary>
    Task ShowInfoAsync(string title, string message);
}
