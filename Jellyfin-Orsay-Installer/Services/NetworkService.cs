using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Jellyfin.Orsay.Installer.Models;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.Services;

public sealed class NetworkService : INetworkService
{
    private List<NetworkInterfaceInfo> _interfaces = new();

    /// <summary>
    /// Patterns for detecting virtual network adapters.
    /// Checked against both Name and Description (case-insensitive).
    /// </summary>
    private static readonly string[] VirtualAdapterPatterns =
    {
        // Hyper-V (Windows)
        "vethernet",
        "hyper-v",

        // VMware
        "vmware",
        "vmnet",

        // VirtualBox
        "virtualbox",
        "vboxnet",
        "host-only",

        // Docker / Containers
        "docker",
        "br-",
        "veth",
        "container",

        // WSL (Windows Subsystem for Linux)
        "wsl",

        // Loopback / Software adapters
        "loopback",
        "pseudo",
        "software loopback",

        // Wi-Fi Direct / P2P (not useful for LAN)
        "wi-fi direct",
        "microsoft wi-fi direct",
        "p2p-",

        // Bluetooth (not useful for LAN server)
        "bluetooth",
        "pan network",

        // VPN TAP/TUN adapters
        "tap-",
        "tun-",
        "tap adapter",
        "utun",

        // Parallels (macOS)
        "parallels",

        // Microsoft transition adapters
        "teredo",
        "isatap",
        "6to4",

        // Corporate VPN
        "cisco anyconnect",
        "juniper",
        "fortinet",
        "palo alto",
        "globalprotect",

        // Consumer VPN
        "vpn",
        "wireguard",
        "openvpn",
        "nordvpn",
        "tailscale",
        "zerotier",
        "hamachi",
        "mullvad",
        "protonvpn",
        "expressvpn",
        "surfshark"
    };

    public NetworkService()
    {
        Refresh();
    }

    public IReadOnlyList<NetworkInterfaceInfo> GetAllNetworkInterfaces() => _interfaces;

    public NetworkInterfaceInfo? GetPreferredInterface() =>
        _interfaces.FirstOrDefault(i => i.IsPreferred) ?? _interfaces.FirstOrDefault();

    public void Refresh()
    {
        _interfaces = DiscoverInterfaces()
            .OrderByDescending(i => i.Score)
            .ThenBy(i => i.Name)
            .ToList();

        MarkPreferred();
    }

    private IEnumerable<NetworkInterfaceInfo> DiscoverInterfaces()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up)
            .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .ToList();

        foreach (var nic in interfaces)
        {
            var ipProperties = nic.GetIPProperties();
            var ipv4Addresses = ipProperties.UnicastAddresses
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .Where(ip => !ip.StartsWith("127."))
                .ToList();

            // Check for default gateway (strong indicator of working network)
            var hasGateway = ipProperties.GatewayAddresses
                .Any(g => g.Address.AddressFamily == AddressFamily.InterNetwork &&
                          !g.Address.ToString().Equals("0.0.0.0"));

            // Detect virtual adapter by name/description patterns
            var isVirtual = IsVirtualAdapter(nic);

            // Determine adapter type (considering virtual detection)
            var adapterType = GetAdapterType(nic, isVirtual);

            foreach (var ip in ipv4Addresses)
            {
                // Calculate score for this interface
                var score = CalculateScore(adapterType, hasGateway, isVirtual, ip);

                yield return new NetworkInterfaceInfo
                {
                    Name = nic.Name,
                    Description = nic.Description,
                    IpAddress = ip,
                    AdapterType = adapterType,
                    HasGateway = hasGateway,
                    IsVirtual = isVirtual,
                    Score = score,
                    IsPreferred = false
                };
            }
        }
    }

    /// <summary>
    /// Detects if the network interface is virtual based on name and description patterns.
    /// </summary>
    private static bool IsVirtualAdapter(NetworkInterface nic)
    {
        var nameLower = nic.Name.ToLowerInvariant();
        var descLower = nic.Description.ToLowerInvariant();

        // Check both name and description against patterns
        foreach (var pattern in VirtualAdapterPatterns)
        {
            if (nameLower.Contains(pattern) || descLower.Contains(pattern))
                return true;
        }

        // PPP/Tunnel interface types are typically VPN
        if (nic.NetworkInterfaceType == NetworkInterfaceType.Ppp ||
            nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
            return true;

        return false;
    }

    /// <summary>
    /// Determines the adapter type, marking virtual adapters appropriately.
    /// </summary>
    private static string GetAdapterType(NetworkInterface nic, bool isVirtual)
    {
        // If detected as virtual, mark it as such
        if (isVirtual)
            return "Virtual";

        return nic.NetworkInterfaceType switch
        {
            NetworkInterfaceType.Ethernet => "Ethernet",
            NetworkInterfaceType.Wireless80211 => "Wi-Fi",
            NetworkInterfaceType.Ppp => "VPN",
            NetworkInterfaceType.Tunnel => "VPN",
            _ => "Other"
        };
    }

    /// <summary>
    /// Calculates a score for the network interface (higher = better).
    /// </summary>
    /// <remarks>
    /// Scoring breakdown:
    /// - Base score by adapter type: Ethernet=100, Wi-Fi=80, Other=40, Virtual=20, VPN=10
    /// - Has gateway bonus: +50 (strong indicator of working network)
    /// - Is virtual penalty: -60 (even if detected as Ethernet)
    /// - CGNAT IP (100.x.x.x) penalty: -30 (often VPN networks like Tailscale)
    /// - APIPA (169.254.x.x) penalty: -80 (no DHCP, limited connectivity)
    /// </remarks>
    private static int CalculateScore(string adapterType, bool hasGateway, bool isVirtual, string ip)
    {
        // Base score by adapter type
        int score = adapterType switch
        {
            "Ethernet" => 100,
            "Wi-Fi" => 80,
            "Other" => 40,
            "Virtual" => 20,
            "VPN" => 10,
            _ => 30
        };

        // Gateway bonus (strong indicator of working internet/LAN)
        if (hasGateway)
            score += 50;

        // Virtual adapter penalty (in addition to base score)
        if (isVirtual)
            score -= 60;

        // IP address penalties
        if (ip.StartsWith("100."))
            score -= 30; // CGNAT range, often VPN (Tailscale uses 100.x.x.x)

        if (ip.StartsWith("169.254."))
            score -= 80; // APIPA - no DHCP, limited connectivity

        // Ensure score doesn't go below 1
        return Math.Max(score, 1);
    }

    private void MarkPreferred()
    {
        if (_interfaces.Count == 0) return;

        // First interface is already the best (sorted by score descending)
        _interfaces[0] = _interfaces[0] with { IsPreferred = true };
    }
}
