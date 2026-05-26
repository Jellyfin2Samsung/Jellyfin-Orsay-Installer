namespace Jellyfin.Orsay.Installer.Models;

/// <summary>
/// Represents the result of packaging a widget.
/// </summary>
public record PackageResult
{
    /// <summary>
    /// Gets the name of the generated ZIP file.
    /// </summary>
    public required string ZipFileName { get; init; }

    /// <summary>
    /// Gets the size of the ZIP file in bytes.
    /// </summary>
    public required long ZipSize { get; init; }

    /// <summary>
    /// Gets the widget ID.
    /// </summary>
    public required string WidgetId { get; init; }

    /// <summary>
    /// Gets the full path to the output directory.
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// Gets the download URL for the widget.
    /// </summary>
    public required string DownloadUrl { get; init; }
}
