using System;
using Jellyfin.Orsay.Installer.Core;
using Jellyfin.Orsay.Installer.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Orsay.Installer.Services;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private ViewModelBase? _currentPage;

    public ViewModelBase? CurrentPage => _currentPage;

    public event Action<ViewModelBase>? Navigated;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        _currentPage = viewModel;
        Navigated?.Invoke(viewModel);
    }

    public void NavigateTo<TViewModel>(Action<TViewModel> configure) where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        configure(viewModel);
        _currentPage = viewModel;
        Navigated?.Invoke(viewModel);
    }

    public bool CanNavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        return _serviceProvider.GetService<TViewModel>() != null;
    }
}
