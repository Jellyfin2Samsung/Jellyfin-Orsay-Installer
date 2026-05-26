using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Jellyfin.Orsay.Installer.Converters;

/// <summary>
/// Converts current step to indicator color.
/// Parameter format: "stepNumber" (e.g., "1", "2", "3")
/// Returns blue if current step >= step number, gray otherwise.
/// </summary>
public class StepIndicatorConverter : IValueConverter
{
    public static readonly StepIndicatorConverter Instance = new();

    private static readonly IBrush ActiveBrush = new SolidColorBrush(Color.Parse("#3498DB"));
    private static readonly IBrush InactiveBrush = new SolidColorBrush(Color.Parse("#BDC3C7"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int currentStep || parameter is not string stepParam)
            return InactiveBrush;

        if (!int.TryParse(stepParam, out var stepNumber))
            return InactiveBrush;

        return currentStep >= stepNumber ? ActiveBrush : InactiveBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
