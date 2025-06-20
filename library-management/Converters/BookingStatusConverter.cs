using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace library_management.Converters;

public class BookingStatusConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime returnDate)
        {
            return returnDate == default(DateTime) ? "Active" : "Returned";
        }
        return "Active";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Avalonia.Data.BindingOperations.DoNothing;
    }
} 