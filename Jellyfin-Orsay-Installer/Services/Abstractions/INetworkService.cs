using System.Collections.Generic;
using Jellyfin.Orsay.Installer.Models;

namespace Jellyfin.Orsay.Installer.Services.Abstractions;

/// <summary>
/// Service for network interface discovery.
/// </summary>
public interface INetworkService
{
    /// <summary>
    /// Gets all available network interfaces with their IP addresses.
    /// </summary>
    IReadOnlyList<NetworkInterfaceInfo> GetAllNetworkInterfaces();

    /// <summary>
    /// Gets the preferred/recommended network interface.
    /// Prioritizes: Ethernet > Wi-Fi > Other (excludes VPN, loopback).
    /// </summary>
    NetworkInterfaceInfo? GetPreferredInterface();

    /// <summary>
    /// Refreshes the list of network interfaces.
    /// </summary>
    void Refresh();
}
