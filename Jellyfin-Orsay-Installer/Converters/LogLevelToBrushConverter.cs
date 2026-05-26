using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Jellyfin.Orsay.Installer.Services.Abstractions;

namespace Jellyfin.Orsay.Installer.Converters;

public class LogLevelToBrushConverter : IValueConverter
{
    public static readonly LogLevelToBrushConverter Instance = new();

    private static readonly IBrush InfoBrush = new SolidColorBrush(Color.Parse("#3498DB"));
    private static readonly IBrush WarningBrush = new SolidColorBrush(Color.Parse("#F39C12"));
    private static readonly IBrush ErrorBrush = new SolidColorBrush(Color.Parse("#E74C3C"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogLevel level)
        {
            return level switch
            {
                LogLevel.Info => InfoBrush,
                LogLevel.Warning => WarningBrush,
                LogLevel.Error => ErrorBrush,
                _ => InfoBrush
            };
        }
        return InfoBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
