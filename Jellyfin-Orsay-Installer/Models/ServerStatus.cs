using System;

namespace Jellyfin.Orsay.Installer.Models;

/// <summary>
/// Represents the current status of the Orsay server.
/// </summary>
public record ServerStatus
{
    /// <summary>
    /// Gets whether the server is currently running.
    /// </summary>
    public bool IsRunning { get; init; }

    /// <summary>
    /// Gets the total number of requests received.
    /// </summary>
    public int RequestCount { get; init; }

    /// <summary>
    /// Gets the last request path received.
    /// </summary>
    public string? LastRequestPath { get; init; }

    /// <summary>
    /// Gets the server URL.
    /// </summary>
    public string? ServerUrl { get; init; }

    /// <summary>
    /// Gets whether the widget list was requested.
    /// </summary>
    public bool WidgetListRequested { get; init; }

    /// <summary>
    /// Gets whether the widget ZIP was downloaded.
    /// </summary>
    public bool WidgetDownloaded { get; init; }

    /// <summary>
    /// Gets whether installation is complete (both widgetlist.xml and .zip requested).
    /// </summary>
    public bool IsInstallationComplete => WidgetListRequested && WidgetDownloaded;

    public static ServerStatus Stopped => new() { IsRunning = false };
}

/// <summary>
/// Represents a request received by the server.
/// </summary>
public record ServerRequest
{
    /// <summary>
    /// HTTP method (GET, POST, etc.).
    /// </summary>
    public required string Method { get; init; }

    /// <summary>
    /// Request path (e.g., /widgetlist.xml).
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Request timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// User-Agent header value.
    /// </summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Content-Type header value.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Content-Length header value.
    /// </summary>
    public long? ContentLength { get; init; }

    /// <summary>
    /// Request body (limited to 10KB).
    /// </summary>
    public string? Body { get; init; }

    /// <summary>
    /// Remote client IP address.
    /// </summary>
    public string? RemoteIp { get; init; }

    public override string ToString() =>
        $"[{Timestamp:HH:mm:ss}] {Method} {Path}";
}
