using System.Threading.Tasks;
using Avalonia.Controls;

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
    /// Shows an error message dialog.
    /// </summary>
    Task ShowErrorAsync(string title, string message);

    /// <summary>
    /// Shows an information message dialog.
    /// </summary>
    Task ShowInfoAsync(string title, string message);
}
