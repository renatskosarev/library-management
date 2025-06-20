using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace library_management.Converters;

public class NullableDateToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            return new SolidColorBrush(Colors.Green);
        }
        else if (value == null)
        {
            return new SolidColorBrush(Colors.Red);
        }
        return new SolidColorBrush(Colors.Black);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 