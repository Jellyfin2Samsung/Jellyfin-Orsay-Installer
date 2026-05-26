using System;
using System.Collections.Generic;

namespace Jellyfin.Orsay.Installer.Models;

/// <summary>
/// Represents a discovered Samsung Orsay TV on the network.
/// </summary>
public record DiscoveredTv
{
    /// <summary>
    /// Gets the IP address of the TV.
    /// </summary>
    public required string IpAddress { get; init; }

    /// <summary>
    /// Gets the friendly name of the TV (from SSDP/UPnP response).
    /// </summary>
    public string? FriendlyName { get; init; }

    /// <summary>
    /// Gets the model name (e.g., "UE40F6400").
    /// </summary>
    public string? ModelName { get; init; }

    /// <summary>
    /// Gets the model number (firmware/version indicator).
    /// </summary>
    public string? ModelNumber { get; init; }

    /// <summary>
    /// Gets the serial number (unique device ID).
    /// </summary>
    public string? SerialNumber { get; init; }

    /// <summary>
    /// Gets the manufacturer (should be "Samsung Electronics").
    /// </summary>
    public string? Manufacturer { get; init; }

    /// <summary>
    /// Gets the unique service name from SSDP (USN header).
    /// </summary>
    public string? UniqueServiceName { get; init; }

    /// <summary>
    /// Gets the discovery method used to find this TV.
    /// </summary>
    public TvDiscoveryMethod DiscoveryMethod { get; init; }

    /// <summary>
    /// Gets the confidence score (0-100) for this being a valid Samsung Orsay TV.
    /// Higher = more confident.
    /// </summary>
    public int ConfidenceScore { get; init; }

    /// <summary>
    /// Gets the detected open ports on the TV.
    /// </summary>
    public IReadOnlyList<int> OpenPorts { get; init; } = [];

    /// <summary>
    /// Gets the timestamp when this TV was discovered.
    /// </summary>
    public DateTime DiscoveredAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Gets display text for UI presentation.
    /// </summary>
    public string DisplayText => string.IsNullOrEmpty(FriendlyName)
        ? IpAddress
        : $"{FriendlyName} ({IpAddress})";

    /// <summary>
    /// Gets version info (model number or model name).
    /// </summary>
    public string? VersionInfo => ModelNumber ?? ModelName;

    public override string ToString() => DisplayText;
}

/// <summary>
/// Method used to discover the TV.
/// </summary>
public enum TvDiscoveryMethod
{
    /// <summary>SSDP/UPnP discovery on UDP 1900.</summary>
    Ssdp,

    /// <summary>TCP port scanning (7676 AllShare, 8443 SERI SSL, 55000 Remote).</summary>
    PortScan,

    /// <summary>Both SSDP and port scan confirmed.</summary>
    SsdpAndPortScan
}
