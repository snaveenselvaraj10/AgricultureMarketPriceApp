using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace AgricultureMarketPriceApp.Converters
{
    public class PercentToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Return a SolidColorBrush directly to avoid depending on external resource dictionaries
            if (value == null) return Color.FromArgb("#E9F9F0");
            if (double.TryParse(value.ToString(), out var d))
            {
                if (d >= 0) return Color.FromArgb("#E9F9F0");
                return Color.FromArgb("#FF6B6B");
            }
            return Color.FromArgb("#E9F9F0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
