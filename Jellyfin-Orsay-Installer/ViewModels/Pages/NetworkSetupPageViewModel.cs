using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Orsay.Installer.Models;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.ViewModels.Pages;

public sealed partial class NetworkSetupPageViewModel : ViewModelBase
{
    private readonly INetworkService _networkService;
    private readonly IClipboardService _clipboard;

    private NetworkListItem? _previousSelectedItem;

    public ObservableCollection<NetworkListItem> AvailableItems { get; } = new();

    [ObservableProperty]
    private NetworkListItem? _selectedItem;

    [ObservableProperty]
    private int _port = 80;

    /// <summary>
    /// Gets the selected network interface (null if separator is selected).
    /// </summary>
    public NetworkInterfaceInfo? SelectedInterface => SelectedItem?.Interface;

    public bool IsValid => SelectedInterface != null && Port > 0 && Port < 65536;

    public NetworkSetupPageViewModel(
        INetworkService networkService,
        IClipboardService clipboard,
        ILocalizationService localization)
        : base(localization)
    {
        _networkService = networkService;
        _clipboard = clipboard;
        RefreshInterfaces();
    }

    /// <summary>
    /// Called when language changes - rebuild the list with updated separator texts.
    /// </summary>
    protected override void OnLocalizationChanged()
    {
        RebuildInterfaceList(_networkService.GetAllNetworkInterfaces());
    }

    [RelayCommand]
    private void RefreshInterfaces()
    {
        _networkService.Refresh();
        RebuildInterfaceList(_networkService.GetAllNetworkInterfaces());
    }

    /// <summary>
    /// Rebuilds the UI list from the given interfaces with localized separators.
    /// </summary>
    private void RebuildInterfaceList(IReadOnlyList<NetworkInterfaceInfo> allInterfaces)
    {
        // Remember current selection
        var currentSelectedIp = SelectedInterface?.IpAddress;

        AvailableItems.Clear();

        // Group interfaces by GroupName
        var physical = allInterfaces.Where(i => i.GroupName == "Physical").ToList();
        var other = allInterfaces.Where(i => i.GroupName == "Other").ToList();
        var virtualAdapters = allInterfaces.Where(i => i.GroupName == "Virtual").ToList();

        // Add Physical group (no separator needed for first group)
        foreach (var iface in physical)
        {
            AvailableItems.Add(new NetworkListItem { Interface = iface });
        }

        // Add Other group with separator (if any)
        if (other.Count > 0)
        {
            AvailableItems.Add(new NetworkListItem
            {
                IsSeparator = true,
                SeparatorText = L["Network.GroupOther"]
            });

            foreach (var iface in other)
            {
                AvailableItems.Add(new NetworkListItem { Interface = iface });
            }
        }

        // Add Virtual group with separator (if any)
        if (virtualAdapters.Count > 0)
        {
            AvailableItems.Add(new NetworkListItem
            {
                IsSeparator = true,
                SeparatorText = L["Network.GroupVirtual"]
            });

            foreach (var iface in virtualAdapters)
            {
                AvailableItems.Add(new NetworkListItem { Interface = iface });
            }
        }

        // Restore selection: try previous IP, then preferred, then first non-separator
        NetworkListItem? newSelection = null;

        if (currentSelectedIp != null)
        {
            newSelection = AvailableItems.FirstOrDefault(item =>
                item.Interface?.IpAddress == currentSelectedIp);
        }

        if (newSelection == null)
        {
            var preferred = _networkService.GetPreferredInterface();
            newSelection = AvailableItems.FirstOrDefault(item => item.Interface == preferred);
        }

        newSelection ??= AvailableItems.FirstOrDefault(item => !item.IsSeparator);

        SelectedItem = newSelection;
        _previousSelectedItem = SelectedItem;
    }

    partial void OnSelectedItemChanged(NetworkListItem? value)
    {
        // Prevent selecting separators
        if (value?.IsSeparator == true)
        {
            // Restore previous selection
            SelectedItem = _previousSelectedItem;
            return;
        }

        _previousSelectedItem = value;
        OnPropertyChanged(nameof(SelectedInterface));
        OnPropertyChanged(nameof(IsValid));
    }

    partial void OnPortChanged(int value)
    {
        OnPropertyChanged(nameof(IsValid));
    }

    [RelayCommand]
    private Task CopyIpAsync() => _clipboard.SetTextAsync(SelectedInterface?.IpAddress);
}
