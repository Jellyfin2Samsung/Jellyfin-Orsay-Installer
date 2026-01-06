using System;
using System.Collections.Generic;

namespace Jellyfin.Orsay.Installer.Services.Abstractions;

/// <summary>
/// Service for aggregating and managing application logs.
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Gets all log entries.
    /// </summary>
    IReadOnlyList<LogEntry> Entries { get; }

    /// <summary>
    /// Event raised when a new log entry is added.
    /// </summary>
    event Action<LogEntry>? LogAdded;

    /// <summary>
    /// Adds a log entry.
    /// </summary>
    void Log(string message, LogLevel level = LogLevel.Info);

    /// <summary>
    /// Clears all log entries.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets all logs as a single string.
    /// </summary>
    string GetAllLogsAsText();
}

/// <summary>
/// Represents a single log entry.
/// </summary>
public record LogEntry(DateTime Timestamp, string Message, LogLevel Level)
{
    public override string ToString() => $"[{Timestamp:HH:mm:ss}] {Message}";
}

/// <summary>
/// Log severity level.
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}
