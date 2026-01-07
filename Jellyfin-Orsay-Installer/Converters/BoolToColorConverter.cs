using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Jellyfin.Orsay.Installer.Converters;

/// <summary>
/// Converts a boolean to a color brush.
/// Parameter format: "trueColor;falseColor" (e.g., "#27AE60;#E74C3C")
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public static readonly BoolToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue || parameter is not string colorParam)
            return new SolidColorBrush(Colors.Gray);

        var colors = colorParam.Split(';');
        if (colors.Length != 2)
            return new SolidColorBrush(Colors.Gray);

        var colorStr = boolValue ? colors[0] : colors[1];
        return new SolidColorBrush(Color.Parse(colorStr));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
