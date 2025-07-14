using System;
using System.Globalization;
using System.Windows.Data;
using CompHomp.Models;

namespace CompHomp.Converters
{
    public class UserRoleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserRole role)
            {
                return role switch
                {
                    UserRole.Customer => "Пользователь",
                    UserRole.Admin => "Администратор",
                    _ => "Неизвестно"
                };
            }
            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "Пользователь" => UserRole.Customer,
                "Администратор" => UserRole.Admin,
                _ => UserRole.Customer
            };
        }
    }
} 