using CompHomp.Models;
using CompHomp.Services;
using CompHomp.Data;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CompHomp
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        private string _verificationCode;
        private readonly AppDbContext _dbContext;
        private readonly EmailService _emailService;
        private readonly AuthService _authService;
        private readonly IAuditService _auditService;
        private PasswordBox _passwordBox;

        public Registration()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _emailService = new EmailService();
            _auditService = new AuditService(_dbContext);
            _authService = new AuthService(_dbContext, _auditService);
            _passwordBox = (PasswordBox)FindName("PasswordBox");
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (!email.Contains("@"))
                return false;

            // Проверка на наличие кириллицы
            if (email.Any(c => (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я')))
            {
                return false;
            }

            return true;
        }

        private async void OnRegisterClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = LoginTextBox.Text.Trim();
                string password = _passwordBox.Password;
                string email = EmailTextBox.Text.Trim();

                // Проверка на пустые поля
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
                {
                    MessageBox.Show("Все поля должны быть заполнены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка email
                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Email должен содержать символ @ и не должен содержать кириллицу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка существования пользователя
                var existingUser = _dbContext.Users.FirstOrDefault(u =>
                    u.Login == login || u.Email == email);

                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь с таким логином или email уже существует",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _auditService.LogEvent(0, "Ошибка регистрации", 
                        $"Попытка регистрации с существующим логином/email: {login}/{email}");
                    return;
                }

                // Валидация пароля
                var (isValid, errorMessage) = User.ValidatePassword(password, _dbContext);
                if (!isValid)
                {
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _auditService.LogEvent(0, "Ошибка регистрации", 
                        $"Неверный формат пароля при регистрации пользователя: {login}");
                    return;
                }

                // Генерация кода подтверждения
                _verificationCode = GenerateVerificationCode();

                try
                {
                    // Отправка кода на email
                    await _emailService.SendVerificationCodeAsync(EmailTextBox.Text, _verificationCode);
                    MessageBox.Show("Код подтверждения отправлен на ваш email",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    _auditService.LogEvent(0, "Регистрация", 
                        $"Отправлен код подтверждения на email: {email}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отправке email: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _auditService.LogEvent(0, "Ошибка регистрации", 
                        $"Ошибка отправки кода подтверждения на email: {email}. Ошибка: {ex.Message}");
                    return;
                }

                // Показываем панель верификации
                VerificationPanel.Visibility = Visibility.Visible;
                RegisterButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _auditService.LogEvent(0, "Ошибка регистрации", 
                    $"Неожиданная ошибка при регистрации: {ex.Message}");
            }
        }

        private void OnBackToLoginClick(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void OnVerifyClick(object sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text.Trim();
            var email = EmailTextBox.Text.Trim();

            if (VerificationCodeTextBox.Text != _verificationCode)
            {
                MessageBox.Show("Неверный код подтверждения",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                _auditService.LogEvent(0, "Ошибка верификации", 
                    $"Неверный код подтверждения при регистрации пользователя: {login}");
                return;
            }

            try
            {
                // Создание нового пользователя
                var user = new User
                {
                    Login = login,
                    Email = email,
                    Password = AuthService.HashPassword(_passwordBox.Password),
                    Role = UserRole.Customer,
                    RegistrationDate = DateTime.Now
                };

                // Сохранение в базу данных
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();

                _auditService.LogEvent(user.Id, "Регистрация завершена", 
                    $"Успешная регистрация нового пользователя: {login}");

                MessageBox.Show("Регистрация успешно завершена!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Открытие окна входа
                var loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _auditService.LogEvent(0, "Ошибка регистрации", 
                    $"Ошибка при сохранении нового пользователя: {login}. Ошибка: {ex.Message}");
            }
        }

        private string ValidatePassword(string password)
        {
            if (password.Length < 6)
            {
                return "Пароль должен содержать не менее 6 символов";
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return "Пароль должен содержать хотя бы один специальный символ";
            }

            return null; // пароль соответствует требованиям
        }

        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
