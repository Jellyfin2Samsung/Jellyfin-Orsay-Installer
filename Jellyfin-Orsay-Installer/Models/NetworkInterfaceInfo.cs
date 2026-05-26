namespace Jellyfin.Orsay.Installer.Models;

/// <summary>
/// Represents information about a network interface.
/// </summary>
public record NetworkInterfaceInfo
{
    /// <summary>
    /// Gets the name of the network interface (e.g., "Wi-Fi", "Ethernet").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the IPv4 address of the interface.
    /// </summary>
    public required string IpAddress { get; init; }

    /// <summary>
    /// Gets the type of the adapter (e.g., "Ethernet", "Wi-Fi", "Virtual", "VPN", "Other").
    /// </summary>
    public required string AdapterType { get; init; }

    /// <summary>
    /// Gets the raw adapter description from the system (e.g., "Intel(R) Wi-Fi 6 AX201").
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets whether this adapter has a default gateway configured.
    /// Adapters with gateways are more likely to have internet/LAN access.
    /// </summary>
    public bool HasGateway { get; init; }

    /// <summary>
    /// Gets whether this is detected as a virtual adapter (Hyper-V, VMware, Docker, etc.).
    /// </summary>
    public bool IsVirtual { get; init; }

    /// <summary>
    /// Gets the internal score used for ranking (higher = better).
    /// </summary>
    public int Score { get; init; }

    /// <summary>
    /// Gets whether this is the recommended/preferred interface.
    /// </summary>
    public bool IsPreferred { get; init; }

    /// <summary>
    /// Gets the group name for UI grouping: "Physical", "Other", or "Virtual".
    /// </summary>
    public string GroupName => IsVirtual ? "Virtual" :
                               AdapterType is "Ethernet" or "Wi-Fi" ? "Physical" : "Other";

    /// <summary>
    /// Gets the display string for UI presentation.
    /// </summary>
    public string DisplayText => $"{IpAddress} ({Name})";

    public override string ToString() => DisplayText;
}
