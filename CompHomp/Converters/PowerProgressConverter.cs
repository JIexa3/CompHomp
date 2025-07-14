using System;
using System.Globalization;
using System.Windows.Data;

namespace CompHomp.Converters
{
    public class PowerProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double currentValue && parameter is string thresholdStr)
            {
                if (double.TryParse(thresholdStr, out double threshold))
                {
                    var percentage = (currentValue / 1000.0) * 100; // 1000W максимум
                    return percentage >= threshold;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
