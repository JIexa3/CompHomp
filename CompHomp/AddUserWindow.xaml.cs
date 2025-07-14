using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using CompHomp.Models;
using CompHomp.Services;

namespace CompHomp
{
    public partial class AddUserWindow : Window
    {
        private readonly UserService _userService;

        public AddUserWindow(UserService userService)
        {
            InitializeComponent();
            _userService = userService;
            InitializeEventHandlers();
            
            // Устанавливаем свойства окна
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
        }

        private void InitializeEventHandlers()
        {
            SaveButton.Click += SaveUser;
            CancelButton.Click += (s, e) => { DialogResult = false; Close(); };
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

        private void SaveUser(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = UsernameTextBox.Text.Trim();
                string email = EmailTextBox.Text.Trim();
                string password = PasswordBox.Password;

                // Проверка на пустые поля
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
                {
                    MessageBox.Show("Все поля должны быть заполнены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка email
                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Неверный формат email. Email не должен содержать кириллицу и должен содержать символ @", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получение выбранной роли
                var selectedItem = RoleComboBox.SelectedItem as ComboBoxItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Выберите роль пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var role = (UserRole)selectedItem.Tag;
                var isActive = IsActiveCheckBox.IsChecked ?? false;

                // Создание пользователя
                if (_userService.CreateUser(username, email, password, role, isActive))
                {
                    MessageBox.Show("Пользователь успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
