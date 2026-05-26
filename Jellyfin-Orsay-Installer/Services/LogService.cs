using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.Services;

public sealed class LogService : ILogService
{
    private readonly List<LogEntry> _entries = new();
    private readonly object _lock = new();

    public IReadOnlyList<LogEntry> Entries
    {
        get
        {
            lock (_lock)
            {
                return _entries.ToList();
            }
        }
    }

    public event Action<LogEntry>? LogAdded;

    public void Log(string message, LogLevel level = LogLevel.Info)
    {
        var entry = new LogEntry(DateTime.Now, message, level);

        lock (_lock)
        {
            _entries.Add(entry);
        }

        LogAdded?.Invoke(entry);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }

    public string GetAllLogsAsText()
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            foreach (var entry in _entries)
            {
                sb.AppendLine(entry.ToString());
            }
            return sb.ToString();
        }
    }
}
