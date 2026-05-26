using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Jellyfin.Orsay.Installer.Core;
using Jellyfin.Orsay.Installer.Models;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.Services;

/// <summary>
/// Discovers Samsung Orsay TVs using SSDP and port scanning.
/// </summary>
public sealed partial class SsdpTvDiscoveryService : ITvDiscoveryService, IDisposable
{
    // SSDP multicast address and port
    private static readonly IPAddress SsdpMulticastAddress = IPAddress.Parse("239.255.255.250");
    private const int SsdpPort = 1900;

    // Samsung-specific identifiers in SSDP responses
    private static readonly string[] SamsungSsdpPatterns =
    [
        "samsung",
        "sec:",
        "schemas-upnp-org:device:MediaRenderer",
        "urn:samsung.com",
        "allshare"
    ];

    /// <summary>
    /// Regex pattern for Samsung Orsay TV model names.
    /// Format: U[Region][Size][Series][Model] e.g., UE40F6400, UN55H5500
    /// Region: A=Australia, E=Europe, K=Korea, N=North America
    /// Series: E=2012, F=2013, H=2014, J=2015
    /// </summary>
    [GeneratedRegex(@"U[AEKN]\d{2}[EFHJ]\d{3,5}", RegexOptions.IgnoreCase)]
    private static partial Regex SamsungModelPattern();

    private readonly ILogService _logService;
    private readonly HttpClient _httpClient;
    private volatile bool _isDiscovering;

    public bool IsDiscovering => _isDiscovering;

    public event Action<DiscoveredTv>? TvDiscovered;
    public event Action<TvDiscoveryProgress>? ProgressChanged;

    public SsdpTvDiscoveryService(ILogService logService)
    {
        _logService = logService;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    public async Task<Result<IReadOnlyList<DiscoveredTv>>> DiscoverAsync(
        string localIpAddress,
        TvDiscoveryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (_isDiscovering)
            return Result<IReadOnlyList<DiscoveredTv>>.Failure("Discovery already in progress");

        options ??= new TvDiscoveryOptions();
        var discoveredTvs = new ConcurrentDictionary<string, DiscoveredTv>();

        try
        {
            _isDiscovering = true;
            ReportProgress(TvDiscoveryPhase.Starting, 0, "Starting discovery...");

            // Phase 1: SSDP Discovery
            _logService.Log("Starting SSDP discovery...");
            ReportProgress(TvDiscoveryPhase.SsdpBroadcast, 10, "Sending SSDP broadcast...");

            var ssdpResults = await DiscoverViaSsdpAsync(
                localIpAddress,
                options.SsdpTimeout,
                cancellationToken);

            foreach (var tv in ssdpResults)
            {
                if (discoveredTvs.TryAdd(tv.IpAddress, tv))
                {
                    TvDiscovered?.Invoke(tv);
                    _logService.Log($"Found TV via SSDP: {tv.DisplayText}");
                }
            }

            ReportProgress(TvDiscoveryPhase.SsdpListening, 40,
                $"SSDP complete. Found {discoveredTvs.Count} TV(s).", discoveredTvs.Count);

            // Phase 2: Port Scanning (if enabled)
            if (options.EnablePortScan)
            {
                _logService.Log("Starting port scan...");
                ReportProgress(TvDiscoveryPhase.PortProbing, 50, "Scanning network ports...");

                var portScanResults = await ScanSubnetAsync(
                    localIpAddress,
                    options,
                    discoveredTvs.Keys.ToHashSet(),
                    cancellationToken);

                foreach (var tv in portScanResults)
                {
                    if (discoveredTvs.TryAdd(tv.IpAddress, tv))
                    {
                        TvDiscovered?.Invoke(tv);
                        _logService.Log($"Found TV via port scan: {tv.DisplayText}");
                    }
                    else
                    {
                        // Merge with SSDP result (upgrade confidence)
                        var existing = discoveredTvs[tv.IpAddress];
                        var merged = existing with
                        {
                            DiscoveryMethod = TvDiscoveryMethod.SsdpAndPortScan,
                            ConfidenceScore = Math.Min(100, existing.ConfidenceScore + 20),
                            OpenPorts = tv.OpenPorts
                        };
                        discoveredTvs[tv.IpAddress] = merged;
                    }
                }
            }

            ReportProgress(TvDiscoveryPhase.Completed, 100,
                $"Discovery complete. Found {discoveredTvs.Count} TV(s).", discoveredTvs.Count);

            var result = discoveredTvs.Values
                .OrderByDescending(tv => tv.ConfidenceScore)
                .ThenBy(tv => tv.IpAddress)
                .ToList();

            _logService.Log($"Discovery completed: {result.Count} Samsung TV(s) found");
            return Result<IReadOnlyList<DiscoveredTv>>.Success(result);
        }
        catch (OperationCanceledException)
        {
            ReportProgress(TvDiscoveryPhase.Cancelled, 0, "Discovery cancelled.");
            _logService.Log("Discovery cancelled", LogLevel.Warning);
            return Result<IReadOnlyList<DiscoveredTv>>.Success(discoveredTvs.Values.ToList());
        }
        catch (Exception ex)
        {
            _logService.Log($"Discovery error: {ex.Message}", LogLevel.Error);
            return Result<IReadOnlyList<DiscoveredTv>>.Failure($"Discovery failed: {ex.Message}");
        }
        finally
        {
            _isDiscovering = false;
        }
    }

    public async Task<DiscoveredTv?> ProbeAsync(
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var ports = new[] { 7676, 8443, 55000 };
        var openPorts = new List<int>();

        foreach (var port in ports)
        {
            if (await IsPortOpenAsync(ipAddress, port, TimeSpan.FromSeconds(2), cancellationToken))
            {
                openPorts.Add(port);
            }
        }

        if (openPorts.Count == 0)
            return null;

        // Try to fetch device description
        var deviceInfo = await FetchDeviceDescriptionAsync(ipAddress, cancellationToken);

        var confidence = CalculateConfidence(deviceInfo, openPorts, null);

        if (confidence < 30)
            return null;

        return new DiscoveredTv
        {
            IpAddress = ipAddress,
            FriendlyName = deviceInfo?.FriendlyName,
            ModelName = deviceInfo?.ModelName,
            ModelNumber = deviceInfo?.ModelNumber,
            SerialNumber = deviceInfo?.SerialNumber,
            Manufacturer = deviceInfo?.Manufacturer,
            DiscoveryMethod = TvDiscoveryMethod.PortScan,
            ConfidenceScore = confidence,
            OpenPorts = openPorts
        };
    }

    // ─────────────────────────────────────────────────────────────────
    // SSDP Discovery
    // ─────────────────────────────────────────────────────────────────

    private async Task<List<DiscoveredTv>> DiscoverViaSsdpAsync(
        string localIpAddress,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var results = new List<DiscoveredTv>();

        // M-SEARCH message for UPnP root devices
        var searchMessage = BuildMSearchMessage("ssdp:all");

        try
        {
            using var udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Parse(localIpAddress), 0));
            udpClient.JoinMulticastGroup(SsdpMulticastAddress);

            var messageBytes = Encoding.UTF8.GetBytes(searchMessage);
            var multicastEndpoint = new IPEndPoint(SsdpMulticastAddress, SsdpPort);

            // Send M-SEARCH multiple times for reliability
            for (int i = 0; i < 3; i++)
            {
                await udpClient.SendAsync(messageBytes, messageBytes.Length, multicastEndpoint);
                await Task.Delay(100, cancellationToken);
            }

            // Collect responses
            var seenIps = new HashSet<string>();
            var endTime = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < endTime)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    udpClient.Client.ReceiveTimeout = Math.Max(100, (int)(endTime - DateTime.UtcNow).TotalMilliseconds);

                    var receiveTask = udpClient.ReceiveAsync(cancellationToken);
                    var delayTask = Task.Delay(500, cancellationToken);

                    var completed = await Task.WhenAny(receiveTask.AsTask(), delayTask);

                    if (completed == delayTask)
                        continue;

                    var result = await receiveTask;
                    var response = Encoding.UTF8.GetString(result.Buffer);
                    var remoteIp = result.RemoteEndPoint.Address.ToString();

                    if (seenIps.Contains(remoteIp))
                        continue;

                    _logService.Log($"SSDP response from {remoteIp}");

                    // Check if this looks like a Samsung device
                    if (IsSamsungSsdpResponse(response))
                    {
                        seenIps.Add(remoteIp);

                        var tv = await ParseSsdpResponseAsync(remoteIp, response, cancellationToken);
                        if (tv != null)
                        {
                            results.Add(tv);
                        }
                    }
                    else
                    {
                        _logService.Log($"  {remoteIp}: not Samsung (filtered)");
                    }
                }
                catch (SocketException)
                {
                    // Timeout, continue
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
            }
        }
        catch (SocketException ex)
        {
            _logService.Log($"SSDP socket error: {ex.Message}", LogLevel.Warning);
        }

        return results;
    }

    private static string BuildMSearchMessage(string searchTarget)
    {
        return $"M-SEARCH * HTTP/1.1\r\n" +
               $"HOST: 239.255.255.250:1900\r\n" +
               $"MAN: \"ssdp:discover\"\r\n" +
               $"MX: 3\r\n" +
               $"ST: {searchTarget}\r\n" +
               $"\r\n";
    }

    private static bool IsSamsungSsdpResponse(string response)
    {
        var responseLower = response.ToLowerInvariant();
        return SamsungSsdpPatterns.Any(p => responseLower.Contains(p.ToLowerInvariant()));
    }

    private async Task<DiscoveredTv?> ParseSsdpResponseAsync(
        string ipAddress,
        string ssdpResponse,
        CancellationToken cancellationToken)
    {
        // Extract USN and Location from SSDP response
        var usn = ExtractHeader(ssdpResponse, "USN");
        var location = ExtractHeader(ssdpResponse, "LOCATION");

        DeviceDescription? deviceInfo = null;

        if (!string.IsNullOrEmpty(location))
        {
            deviceInfo = await FetchDeviceDescriptionFromUrlAsync(location, cancellationToken);
        }

        deviceInfo ??= await FetchDeviceDescriptionAsync(ipAddress, cancellationToken);

        var confidence = CalculateConfidence(deviceInfo, null, ssdpResponse);

        return new DiscoveredTv
        {
            IpAddress = ipAddress,
            FriendlyName = deviceInfo?.FriendlyName,
            ModelName = deviceInfo?.ModelName,
            ModelNumber = deviceInfo?.ModelNumber,
            SerialNumber = deviceInfo?.SerialNumber,
            Manufacturer = deviceInfo?.Manufacturer,
            UniqueServiceName = usn,
            DiscoveryMethod = TvDiscoveryMethod.Ssdp,
            ConfidenceScore = confidence,
            OpenPorts = []
        };
    }

    private static string? ExtractHeader(string response, string headerName)
    {
        var pattern = $@"{headerName}:\s*(.+?)(?:\r\n|\r|\n)";
        var match = Regex.Match(response, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    // ─────────────────────────────────────────────────────────────────
    // Port Scanning
    // ─────────────────────────────────────────────────────────────────

    private async Task<List<DiscoveredTv>> ScanSubnetAsync(
        string localIpAddress,
        TvDiscoveryOptions options,
        HashSet<string> excludeIps,
        CancellationToken cancellationToken)
    {
        var results = new ConcurrentBag<DiscoveredTv>();
        var ipsToScan = GetSubnetIps(localIpAddress, options.SubnetMask)
            .Where(ip => !excludeIps.Contains(ip) && ip != localIpAddress)
            .ToList();

        if (ipsToScan.Count > 0)
        {
            _logService.Log($"Port scan: {ipsToScan.First()} - {ipsToScan.Last()} ({ipsToScan.Count} IPs)");
        }

        var semaphore = new SemaphoreSlim(options.MaxConcurrentProbes);
        var tasks = new List<Task>();
        var scanned = 0;

        foreach (var ip in ipsToScan)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await semaphore.WaitAsync(cancellationToken);

            var currentIp = ip;
            var task = Task.Run(async () =>
            {
                try
                {
                    var openPorts = new List<int>();

                    foreach (var port in options.ProbePorts)
                    {
                        if (await IsPortOpenAsync(currentIp, port, options.PortProbeTimeout, cancellationToken))
                        {
                            openPorts.Add(port);
                        }
                    }

                    if (openPorts.Count > 0)
                    {
                        _logService.Log($"  {currentIp}: ports {string.Join(", ", openPorts)} open");

                        var deviceInfo = await FetchDeviceDescriptionAsync(currentIp, cancellationToken);
                        var confidence = CalculateConfidence(deviceInfo, openPorts, null);

                        if (deviceInfo != null)
                        {
                            var name = deviceInfo.FriendlyName ?? deviceInfo.ModelName ?? "unknown";
                            _logService.Log($"  {currentIp}: {name} (confidence {confidence}%)");
                        }
                        else
                        {
                            _logService.Log($"  {currentIp}: no device info (confidence {confidence}%)");
                        }

                        if (confidence >= 40)
                        {
                            var tv = new DiscoveredTv
                            {
                                IpAddress = currentIp,
                                FriendlyName = deviceInfo?.FriendlyName,
                                ModelName = deviceInfo?.ModelName,
                                ModelNumber = deviceInfo?.ModelNumber,
                                SerialNumber = deviceInfo?.SerialNumber,
                                Manufacturer = deviceInfo?.Manufacturer,
                                DiscoveryMethod = TvDiscoveryMethod.PortScan,
                                ConfidenceScore = confidence,
                                OpenPorts = openPorts
                            };
                            results.Add(tv);
                            TvDiscovered?.Invoke(tv);
                        }
                        else
                        {
                            _logService.Log($"  {currentIp}: filtered (confidence {confidence}% < 40%)");
                        }
                    }

                    var currentScanned = Interlocked.Increment(ref scanned);
                    var progress = 50 + (int)(currentScanned * 45.0 / ipsToScan.Count);
                    ReportProgress(TvDiscoveryPhase.PortProbing, progress,
                        $"Scanning {currentIp}...", results.Count, currentIp);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        return results.ToList();
    }

    private static IEnumerable<string> GetSubnetIps(string localIp, int subnetMask)
    {
        var ipBytes = IPAddress.Parse(localIp).GetAddressBytes();
        var networkBytes = new byte[4];

        // Calculate network address
        var maskBits = subnetMask;
        for (int i = 0; i < 4; i++)
        {
            var bits = Math.Min(8, maskBits);
            var mask = (byte)(bits == 0 ? 0 : (0xFF << (8 - bits)));
            networkBytes[i] = (byte)(ipBytes[i] & mask);
            maskBits = Math.Max(0, maskBits - 8);
        }

        // Generate host addresses (skip network and broadcast)
        var hostBits = 32 - subnetMask;
        var hostCount = (1 << hostBits) - 2;
        var maxHosts = Math.Min(hostCount, 254); // Limit to /24 at most

        for (int i = 1; i <= maxHosts; i++)
        {
            var hostBytes = networkBytes.ToArray();

            // Add host number to network address
            var remaining = i;
            for (int j = 3; j >= 0 && remaining > 0; j--)
            {
                var maxForOctet = j == 3 ? 255 : 256;
                hostBytes[j] = (byte)((networkBytes[j] + (remaining % maxForOctet)) & 0xFF);
                remaining /= 256;
            }

            yield return new IPAddress(hostBytes).ToString();
        }
    }

    private static async Task<bool> IsPortOpenAsync(
        string ip,
        int port,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        try
        {
            using var tcpClient = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            await tcpClient.ConnectAsync(ip, port, cts.Token);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Device Description (UPnP)
    // ─────────────────────────────────────────────────────────────────

    private record DeviceDescription(
        string? FriendlyName,
        string? ModelName,
        string? ModelNumber,
        string? SerialNumber,
        string? Manufacturer);

    private async Task<DeviceDescription?> FetchDeviceDescriptionAsync(
        string ipAddress,
        CancellationToken cancellationToken)
    {
        // Try Samsung UPnP description URLs (7676 = AllShare, 9197 = DMR)
        var urls = new[]
        {
            $"http://{ipAddress}:7676/smp_4_",
            $"http://{ipAddress}:7676/smp_2_",
            $"http://{ipAddress}:9197/dmr"
        };

        foreach (var url in urls)
        {
            var result = await FetchDeviceDescriptionFromUrlAsync(url, cancellationToken);
            if (result != null)
                return result;
        }

        return null;
    }

    private async Task<DeviceDescription?> FetchDeviceDescriptionFromUrlAsync(
        string url,
        CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var response = await _httpClient.GetStringAsync(url, cts.Token);

            if (string.IsNullOrEmpty(response))
                return null;

            // Parse UPnP XML
            var doc = XDocument.Parse(response);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            var device = doc.Descendants(ns + "device").FirstOrDefault();
            if (device == null)
                return null;

            return new DeviceDescription(
                device.Element(ns + "friendlyName")?.Value,
                device.Element(ns + "modelName")?.Value,
                device.Element(ns + "modelNumber")?.Value,
                device.Element(ns + "serialNumber")?.Value,
                device.Element(ns + "manufacturer")?.Value);
        }
        catch
        {
            return null;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Confidence Calculation
    // ─────────────────────────────────────────────────────────────────

    private static int CalculateConfidence(
        DeviceDescription? deviceInfo,
        IReadOnlyList<int>? openPorts,
        string? ssdpResponse)
    {
        int score = 0;

        // SSDP response indicators
        if (!string.IsNullOrEmpty(ssdpResponse))
        {
            var lower = ssdpResponse.ToLowerInvariant();
            if (lower.Contains("samsung")) score += 30;
            if (lower.Contains("sec:")) score += 20;
            if (lower.Contains("allshare")) score += 15;
            if (lower.Contains("bada")) score += 25; // Orsay OS name
        }

        // Device description indicators
        if (deviceInfo != null)
        {
            if (deviceInfo.Manufacturer?.Contains("Samsung", StringComparison.OrdinalIgnoreCase) == true)
                score += 30;

            // Check if model name matches Samsung Orsay TV pattern (strong indicator)
            if (!string.IsNullOrEmpty(deviceInfo.ModelName) &&
                SamsungModelPattern().IsMatch(deviceInfo.ModelName))
            {
                score += 40; // Strong indicator - Samsung TV model pattern matched
            }
            else if (deviceInfo.ModelName?.Contains("TV", StringComparison.OrdinalIgnoreCase) == true)
            {
                score += 10; // Weak indicator - just contains "TV"
            }

            if (!string.IsNullOrEmpty(deviceInfo.FriendlyName))
                score += 5;
        }

        // Port indicators (Samsung-standard ports only)
        if (openPorts != null)
        {
            if (openPorts.Contains(7676)) score += 35;  // AllShare UPnP - Samsung standard
            if (openPorts.Contains(55000)) score += 30; // Orsay Remote Control - Samsung standard
            if (openPorts.Contains(8443)) score += 25;  // Samsung SERI SSL
        }

        return Math.Min(100, score);
    }

    // ─────────────────────────────────────────────────────────────────
    // Progress Reporting
    // ─────────────────────────────────────────────────────────────────

    private void ReportProgress(
        TvDiscoveryPhase phase,
        int percent,
        string message,
        int tvsFound = 0,
        string? currentIp = null)
    {
        ProgressChanged?.Invoke(new TvDiscoveryProgress
        {
            Phase = phase,
            ProgressPercent = percent,
            Message = message,
            TvsFound = tvsFound,
            CurrentIp = currentIp
        });
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
