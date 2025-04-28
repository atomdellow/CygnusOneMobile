using System;
using System.Globalization;
using Microsoft.Maui.Controls; // Added missing import for IValueConverter

namespace CygnusOneMobile.Converters
{
    public class BoolToLoginTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isBusy && isBusy)
            {
                return "Logging in...";
            }
            
            return "Login";
        }
        
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class BoolNegationConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            
            return true;
        }
        
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            
            return false;
        }
    }
    
    public class StringNotEmptyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is string stringValue && !string.IsNullOrWhiteSpace(stringValue);
        }
        
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class BoolToPasswordVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isPasswordHidden && isPasswordHidden)
            {
                return "👁️"; // Eye icon when password is hidden (to show it)
            }
            
            return "🔒"; // Lock icon when password is visible (to hide it)
        }
        
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}