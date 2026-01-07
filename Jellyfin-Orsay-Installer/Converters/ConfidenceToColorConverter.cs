using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Jellyfin.Orsay.Installer.Converters;

/// <summary>
/// Converts a confidence score (0-100) to a color brush.
/// High (70+): Green, Medium (40-69): Yellow/Orange, Low (0-39): Red
/// </summary>
public class ConfidenceToColorConverter : IValueConverter, IMultiValueConverter
{
    public static readonly ConfidenceToColorConverter Instance = new();

    private static readonly SolidColorBrush HighConfidence = new(Color.Parse("#27AE60"));   // Green
    private static readonly SolidColorBrush MediumConfidence = new(Color.Parse("#F39C12")); // Orange
    private static readonly SolidColorBrush LowConfidence = new(Color.Parse("#E74C3C"));    // Red

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int score)
        {
            return GetBrushForScore(score);
        }
        return LowConfidence;
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is int score)
        {
            return GetBrushForScore(score);
        }
        return LowConfidence;
    }

    private static SolidColorBrush GetBrushForScore(int score)
    {
        return score switch
        {
            >= 70 => HighConfidence,
            >= 40 => MediumConfidence,
            _ => LowConfidence
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
