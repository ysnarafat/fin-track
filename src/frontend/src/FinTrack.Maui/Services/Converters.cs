using System.Globalization;

namespace FinTrack.Maui.Services;

/// <summary>
/// Converter for formatting currency values
/// </summary>
public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue.ToString("C", culture);
        }
        
        if (value is double doubleValue)
        {
            return doubleValue.ToString("C", culture);
        }
        
        if (value is float floatValue)
        {
            return floatValue.ToString("C", culture);
        }
        
        return "$0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue && decimal.TryParse(stringValue.Replace("$", "").Replace(",", ""), out var result))
        {
            return result;
        }
        
        return 0m;
    }
}

/// <summary>
/// Converter for formatting percentage values
/// </summary>
public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return $"{doubleValue:F0}%";
        }
        
        if (value is decimal decimalValue)
        {
            return $"{decimalValue:F0}%";
        }
        
        if (value is float floatValue)
        {
            return $"{floatValue:F0}%";
        }
        
        return "0%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for converting percentage to decimal for ProgressBar
/// </summary>
public class PercentageToDecimalConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return Math.Min(doubleValue / 100.0, 1.0); // Cap at 100%
        }
        
        if (value is decimal decimalValue)
        {
            return Math.Min((double)decimalValue / 100.0, 1.0);
        }
        
        if (value is float floatValue)
        {
            return Math.Min(floatValue / 100.0, 1.0);
        }
        
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue * 100.0;
        }
        
        return 0.0;
    }
}

/// <summary>
/// Converter for amount to color based on positive/negative values
/// </summary>
public class AmountToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue >= 0 ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444"); // Green for positive, red for negative
        }
        
        if (value is double doubleValue)
        {
            return doubleValue >= 0 ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444");
        }
        
        if (value is float floatValue)
        {
            return floatValue >= 0 ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444");
        }
        
        return Color.FromArgb("#6B7280"); // Gray for unknown
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to check if value is not null
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to invert boolean values
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        
        return false;
    }
}

/// <summary>
/// Converter for boolean to color with parameter support
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string colorParam)
        {
            var colors = colorParam.Split('|');
            if (colors.Length == 2)
            {
                return Color.FromArgb(boolValue ? colors[0] : colors[1]);
            }
        }
        
        return Color.FromArgb("#6B7280"); // Default gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for boolean to string with parameter support
/// </summary>
public class BoolToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string stringParam)
        {
            var strings = stringParam.Split('|');
            if (strings.Length == 2)
            {
                return boolValue ? strings[0] : strings[1];
            }
        }
        
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for integer to boolean with parameter support
/// </summary>
public class IntToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string condition)
        {
            if (condition.StartsWith(">"))
            {
                if (int.TryParse(condition.Substring(1), out var threshold))
                {
                    return intValue > threshold;
                }
            }
            else if (condition.StartsWith("<"))
            {
                if (int.TryParse(condition.Substring(1), out var threshold))
                {
                    return intValue < threshold;
                }
            }
            else if (condition.StartsWith("="))
            {
                if (int.TryParse(condition.Substring(1), out var threshold))
                {
                    return intValue == threshold;
                }
            }
        }
        
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for index-based selection (for Picker)
/// </summary>
public class IndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue - 1; // Convert 1-based to 0-based index
        }
        
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue + 1; // Convert 0-based to 1-based index
        }
        
        return 1;
    }
}