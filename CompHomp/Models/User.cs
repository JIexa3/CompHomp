using CompHomp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompHomp.Models
{
    public enum UserRole
    {
        Customer=0,
        Admin=1
        
    }

    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }  // В реальном приложении должен храниться хеш пароля
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int LoginAttempts { get; set; }
        public DateTime? LastLoginAttempt { get; set; }
        public List<PurchaseHistory> PurchaseHistory { get; set; }
        public List<Build> Builds { get; set; }
        public List<Order> Orders { get; set; }
        public List<CartItem> CartItems { get; set; }
        public List<Sale> Sales { get; set; }
        public List<PurchaseHistoryItem> PurchaseHistoryItems { get; set; }

        public User()
        {
            PurchaseHistory = new List<PurchaseHistory>();
            Builds = new List<Build>();
            Orders = new List<Order>();
            CartItems = new List<CartItem>();
            Sales = new List<Sale>();
            PurchaseHistoryItems = new List<PurchaseHistoryItem>();
            RegistrationDate = DateTime.Now;
            IsBlocked = false;
            IsActive = true;
            LoginAttempts = 0;
        }

        public static (bool isValid, string errorMessage) ValidatePassword(string password, AppDbContext context)
        {
            var settings = context.SystemSettings.FirstOrDefault();
            if (settings == null)
                return (false, "Ошибка: настройки системы не найдены");

            if (string.IsNullOrEmpty(password))
                return (false, "Пароль не может быть пустым");

            if (password.Length < settings.MinPasswordLength)
                return (false, $"Пароль должен содержать не менее {settings.MinPasswordLength} символов");

            if (settings.RequireComplexPassword)
            {
                bool hasUpperCase = password.Any(char.IsUpper);
                bool hasLowerCase = password.Any(char.IsLower);
                bool hasDigit = password.Any(char.IsDigit);
                bool hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

                if (!hasUpperCase)
                    return (false, "Пароль должен содержать хотя бы одну заглавную букву");
                if (!hasLowerCase)
                    return (false, "Пароль должен содержать хотя бы одну строчную букву");
                if (!hasDigit)
                    return (false, "Пароль должен содержать хотя бы одну цифру");
                if (!hasSpecialChar)
                    return (false, "Пароль должен содержать хотя бы один специальный символ");
            }

            return (true, string.Empty);
        }
    }
}
