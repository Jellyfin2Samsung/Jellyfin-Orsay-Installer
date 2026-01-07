using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.Services;

public sealed class ClipboardService : IClipboardService
{
    public async Task SetTextAsync(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow != null)
        {
            var clipboard = TopLevel.GetTopLevel(desktop.MainWindow)?.Clipboard;
            if (clipboard != null)
                await clipboard.SetTextAsync(text);
        }
    }
}
