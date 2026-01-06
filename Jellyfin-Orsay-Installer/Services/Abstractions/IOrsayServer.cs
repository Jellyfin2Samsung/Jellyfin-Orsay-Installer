using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Orsay.Installer.Models;

namespace Jellyfin.Orsay.Installer.Services.Abstractions;

/// <summary>
/// Service for serving Samsung Orsay widgets via HTTP.
/// </summary>
public interface IOrsayServer : IAsyncDisposable
{
    /// <summary>
    /// Gets the current server status.
    /// </summary>
    ServerStatus Status { get; }

    /// <summary>
    /// Gets whether the server is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Event raised when a request is received.
    /// </summary>
    event Action<ServerRequest>? OnRequest;

    /// <summary>
    /// Event raised when a log message is generated.
    /// </summary>
    event Action<string>? OnLog;

    /// <summary>
    /// Starts the server.
    /// </summary>
    /// <param name="rootPath">Root directory to serve files from.</param>
    /// <param name="ip">IP address to bind to.</param>
    /// <param name="port">Port number to listen on.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StartAsync(string rootPath, string ip, int port, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the server.
    /// </summary>
    Task StopAsync();
}
