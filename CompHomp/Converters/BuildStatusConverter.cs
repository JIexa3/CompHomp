using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Data;
using CompHomp.Models;

namespace CompHomp.Converters
{
    public class BuildStatusConverter : IValueConverter
    {
        public static Dictionary<BuildStatus, string> StatusTranslations { get; } = new()
        {
            { BuildStatus.Pending, "На рассмотрении" },
            { BuildStatus.Approved, "Одобрено" },
            { BuildStatus.Rejected, "Отклонено" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BuildStatus status && StatusTranslations.TryGetValue(status, out var translation))
            {
                return translation;
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                foreach (var pair in StatusTranslations)
                {
                    if (pair.Value.Equals(stringValue))
                    {
                        return pair.Key;
                    }
                }
            }
            return BuildStatus.Pending;
        }
    }
}
