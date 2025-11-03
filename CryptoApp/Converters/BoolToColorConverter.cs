using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace CryptoApp.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isPrime)
            {
                return isPrime ? Brushes.Green : Brushes.Red;
            }
            // По умолчанию (когда value is null) цвет будет серым
            return Brushes.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}