using System;
using System.Globalization;
using System.Windows.Data;

namespace CompHomp.Converters
{
    public class TotalPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal price)
            {
                return string.Format(CultureInfo.GetCultureInfo("ru-RU"), "{0:C2}", price);
            }
            return "0,00 ₽";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                strValue = strValue.Replace("₽", "").Trim();
                if (decimal.TryParse(strValue, NumberStyles.Currency, CultureInfo.GetCultureInfo("ru-RU"), out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }
}
