using System;
using Avalonia.Controls;
using Jellyfin.Orsay.Installer.ViewModels.Dialogs;

namespace Jellyfin.Orsay.Installer.Views.Dialogs;

public partial class TvScannerWindow : Window
{
    public TvScannerWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is TvScannerViewModel viewModel)
        {
            viewModel.CloseRequested += OnCloseRequested;
        }
    }

    private void OnCloseRequested()
    {
        Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (DataContext is TvScannerViewModel viewModel)
        {
            viewModel.CloseRequested -= OnCloseRequested;
            viewModel.Dispose();
        }
    }
}
