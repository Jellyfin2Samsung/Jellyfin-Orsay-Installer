namespace Jellyfin.Orsay.Installer.Models;

/// <summary>
/// Represents the progress of a TV discovery operation.
/// </summary>
public record TvDiscoveryProgress
{
    /// <summary>
    /// Gets the current phase of discovery.
    /// </summary>
    public TvDiscoveryPhase Phase { get; init; }

    /// <summary>
    /// Gets the progress percentage (0-100).
    /// </summary>
    public int ProgressPercent { get; init; }

    /// <summary>
    /// Gets the current status message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the number of TVs discovered so far.
    /// </summary>
    public int TvsFound { get; init; }

    /// <summary>
    /// Gets the current IP being scanned (for port scan phase).
    /// </summary>
    public string? CurrentIp { get; init; }
}

/// <summary>
/// Phases of TV discovery.
/// </summary>
public enum TvDiscoveryPhase
{
    /// <summary>Starting discovery.</summary>
    Starting,

    /// <summary>Sending SSDP M-SEARCH broadcast.</summary>
    SsdpBroadcast,

    /// <summary>Waiting for SSDP responses.</summary>
    SsdpListening,

    /// <summary>Probing ports on candidate IPs.</summary>
    PortProbing,

    /// <summary>Fetching device descriptions.</summary>
    FetchingDeviceInfo,

    /// <summary>Discovery completed.</summary>
    Completed,

    /// <summary>Discovery cancelled.</summary>
    Cancelled
}
