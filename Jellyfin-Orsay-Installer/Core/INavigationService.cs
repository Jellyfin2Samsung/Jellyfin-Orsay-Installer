using System;
using Jellyfin.Orsay.Installer.ViewModels;

namespace Jellyfin.Orsay.Installer.Core;

/// <summary>
/// Service for navigating between pages in the wizard.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets the current page view model.
    /// </summary>
    ViewModelBase? CurrentPage { get; }

    /// <summary>
    /// Event raised when navigation occurs.
    /// </summary>
    event Action<ViewModelBase>? Navigated;

    /// <summary>
    /// Navigates to the specified page.
    /// </summary>
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

    /// <summary>
    /// Navigates to the specified page with parameters.
    /// </summary>
    void NavigateTo<TViewModel>(Action<TViewModel> configure) where TViewModel : ViewModelBase;

    /// <summary>
    /// Returns true if navigation to the specified page is possible.
    /// </summary>
    bool CanNavigateTo<TViewModel>() where TViewModel : ViewModelBase;
}
