using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Orsay.Installer.Core;
using Jellyfin.Orsay.Installer.Models;

namespace Jellyfin.Orsay.Installer.Services.Abstractions;

/// <summary>
/// Service for discovering Samsung Orsay TVs on the local network.
/// </summary>
public interface ITvDiscoveryService
{
    /// <summary>
    /// Gets whether a discovery is currently in progress.
    /// </summary>
    bool IsDiscovering { get; }

    /// <summary>
    /// Event raised when a TV is discovered during scanning.
    /// </summary>
    event Action<DiscoveredTv>? TvDiscovered;

    /// <summary>
    /// Event raised to report discovery progress.
    /// </summary>
    event Action<TvDiscoveryProgress>? ProgressChanged;

    /// <summary>
    /// Discovers Samsung Orsay TVs on the network using the specified interface.
    /// </summary>
    /// <param name="localIpAddress">Local IP address to use for discovery (determines subnet).</param>
    /// <param name="options">Discovery options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing list of discovered TVs or error message.</returns>
    Task<Result<IReadOnlyList<DiscoveredTv>>> DiscoverAsync(
        string localIpAddress,
        TvDiscoveryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a specific IP address is a Samsung Orsay TV.
    /// </summary>
    /// <param name="ipAddress">IP address to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Discovered TV info if valid, null otherwise.</returns>
    Task<DiscoveredTv?> ProbeAsync(
        string ipAddress,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for TV discovery.
/// </summary>
public record TvDiscoveryOptions
{
    /// <summary>
    /// Timeout for SSDP responses (default: 5 seconds).
    /// </summary>
    public TimeSpan SsdpTimeout { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Timeout for individual port probe (default: 1 second).
    /// </summary>
    public TimeSpan PortProbeTimeout { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum concurrent port probes (default: 20).
    /// </summary>
    public int MaxConcurrentProbes { get; init; } = 20;

    /// <summary>
    /// Whether to perform port scanning in addition to SSDP (default: true).
    /// </summary>
    public bool EnablePortScan { get; init; } = true;

    /// <summary>
    /// Ports to probe for Samsung Orsay TVs.
    /// Default: 7676 (AllShare UPnP), 8443 (Samsung SERI SSL), 55000 (Orsay Remote).
    /// </summary>
    public IReadOnlyList<int> ProbePorts { get; init; } = [7676, 8443, 55000];

    /// <summary>
    /// Subnet mask for port scanning (default: 24-bit, e.g., 192.168.1.0/24).
    /// </summary>
    public int SubnetMask { get; init; } = 24;
}
