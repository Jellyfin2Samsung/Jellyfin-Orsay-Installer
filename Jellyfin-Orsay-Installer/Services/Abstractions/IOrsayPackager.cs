using Jellyfin.Orsay.Installer.Core;
using Jellyfin.Orsay.Installer.Models;

namespace Jellyfin.Orsay.Installer.Services.Abstractions;

/// <summary>
/// Service for packaging Samsung Orsay widgets.
/// </summary>
public interface IOrsayPackager
{
    /// <summary>
    /// Gets the default output path for packaged widgets.
    /// </summary>
    string GetDefaultOutputPath();

    /// <summary>
    /// Builds and packages the widget.
    /// </summary>
    /// <param name="outputRoot">Directory to output the packaged widget.</param>
    /// <param name="appName">Name of the application.</param>
    /// <param name="localIp">Local IP address for the download URL.</param>
    /// <param name="port">Port number for the download URL.</param>
    /// <returns>Result containing build information or error.</returns>
    Result<PackageResult> BuildWidget(string outputRoot, string appName, string localIp, int port);
}
