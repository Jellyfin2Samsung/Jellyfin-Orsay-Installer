using System.Threading.Tasks;

namespace Jellyfin.Orsay.Installer.Services.Abstractions;

public interface IClipboardService
{
    Task SetTextAsync(string? text);
}
