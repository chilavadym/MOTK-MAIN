using Avalonia.Data.Converters;
using Common.Units;
using System;
using System.Globalization;

namespace MOTK.Converters;

public class UnitConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            OilCondition oc => oc.ToString("N0 u"),
            Temperature tm => tm.ToString("N1 u"),
            _ => throw new NotImplementedException()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}