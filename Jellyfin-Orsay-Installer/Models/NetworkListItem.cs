namespace Jellyfin.Orsay.Installer.Models;

/// <summary>
/// Represents an item in the network interface list.
/// Can be either a network interface or a group separator.
/// </summary>
public record NetworkListItem
{
    /// <summary>
    /// Gets whether this item is a group separator.
    /// </summary>
    public bool IsSeparator { get; init; }

    /// <summary>
    /// Gets the separator text (only used when IsSeparator is true).
    /// </summary>
    public string? SeparatorText { get; init; }

    /// <summary>
    /// Gets the network interface (only used when IsSeparator is false).
    /// </summary>
    public NetworkInterfaceInfo? Interface { get; init; }

    /// <summary>
    /// Gets the display text for UI binding.
    /// </summary>
    public string DisplayText => IsSeparator
        ? SeparatorText ?? string.Empty
        : Interface?.DisplayText ?? string.Empty;

    public override string ToString() => DisplayText;
}
