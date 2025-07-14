using CompHomp.Models;
using CompHomp.Services;
using CompHomp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace CompHomp
{
    public class GreaterThanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int length)
            {
                return length > 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;
        private readonly LoginAttemptService _loginAttemptService;
        private readonly SaleStatisticsService _statisticsService;
        private readonly IAuditService _auditService;

        public MainWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            _auditService = new AuditService(_context);
            _userService = new UserService(_context, _auditService);
            _loginAttemptService = new LoginAttemptService(_context);
            _statisticsService = new SaleStatisticsService(_context);
            var migrationService = new UserMigrationService(_context);
            migrationService.MigrateUserPasswords();
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            string login = LoginTextbox.Text.Trim();
            string password = PasswordBox.Password;

            // Очистка предыдущих сообщений об ошибках
            ErrorMessage.Text = string.Empty;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorMessage.Text = "Пожалуйста, заполните все поля";
                return;
            }

            try
            {
                var user = _userService.GetUserByLogin(login);

                if (user == null)
                {
                    ErrorMessage.Text = "Пользователь не найден";
                    _loginAttemptService.IncrementAttempts(login);
                    return;
                }

                // Проверяем блокировку до проверки пароля
                if (user.IsBlocked)
                {
                    ErrorMessage.Text = "Пользователь заблокирован";
                    return;
                }

                // Проверяем блокировку по количеству попыток
                if (_loginAttemptService.IsBlocked(login))
                {
                    ErrorMessage.Text = _loginAttemptService.GetBlockedMessage(login);
                    return;
                }

                bool isValidPassword = _userService.ValidatePassword(login, password);

                if (isValidPassword)
                {
                    if (user.Role == UserRole.Admin)
                    {
                        var adminWindow = new MainAdminWindow(user, _context, _statisticsService);
                        adminWindow.Show();
                    }
                    else
                    {
                        var mainWindow = new MainUserWindow(user);
                        mainWindow.Show();
                    }
                    Close();
                }
                else
                {
                    ErrorMessage.Text = "Неверный логин или пароль";
                    _loginAttemptService.IncrementAttempts(login);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = "Произошла ошибка при входе";
                System.Diagnostics.Debug.WriteLine($"Ошибка входа: {ex.Message}");
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registration = new Registration();
            registration.Show();
            Hide();
        }

        private void LoginTextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (LoginTextbox.Text == "Логин")
            {
                LoginTextbox.Text = string.Empty;
            }
        }

        private void LoginTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextbox.Text))
            {
                LoginTextbox.Text = "Логин";
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password == PasswordBox.Tag.ToString())
            {
                PasswordBox.Password = string.Empty;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordBox.Password = PasswordBox.Tag.ToString();
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordHint.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}