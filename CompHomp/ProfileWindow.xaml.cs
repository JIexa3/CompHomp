using CompHomp.Models;
using CompHomp.Services;
using CompHomp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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
using BCrypt.Net;

namespace CompHomp
{
    /// <summary>
    /// Логика взаимодействия для ProfileWindow.xaml
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private readonly User _currentUser;
        private readonly OrderService _orderService;
        private readonly AuthService _authService;
        private readonly AppDbContext _dbContext;
        public ProfileWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _dbContext = new AppDbContext();
            _orderService = new OrderService(_dbContext);
            _authService = new AuthService(_dbContext);

            LoadUserData();
            LoadOrders();
        }
        private void LoadUserData()
        {
            NameTextBox.Text = _currentUser.Login;
            EmailTextBox.Text = _currentUser.Email;
        }

        private void LoadOrders()
        {
            try
            {
                var orders = _orderService.GetUserOrders(_currentUser.Id);
                OrdersGrid.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация входных данных
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Логин не может быть пустым", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Введите корректный email", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Проверка уникальности логина
                var existingUserByLogin = _dbContext.Users
                    .FirstOrDefault(u => u.Login == NameTextBox.Text && u.Id != _currentUser.Id);
                if (existingUserByLogin != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка уникальности email
                var existingUserByEmail = _dbContext.Users
                    .FirstOrDefault(u => u.Email == EmailTextBox.Text && u.Id != _currentUser.Id);
                if (existingUserByEmail != null)
                {
                    MessageBox.Show("Пользователь с таким email уже существует", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Обновление данных
                var userToUpdate = _dbContext.Users.Find(_currentUser.Id);
                if (userToUpdate == null)
                {
                    MessageBox.Show("Пользователь не найден", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                userToUpdate.Login = NameTextBox.Text;
                userToUpdate.Email = EmailTextBox.Text;

                // Обновление пароля
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    if (PasswordBox.Password.Length < 6)
                    {
                        MessageBox.Show("Пароль должен содержать не менее 6 символов", "Ошибка", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    
                    userToUpdate.Password = AuthService.HashPassword(PasswordBox.Password);
                }

                _dbContext.SaveChanges();
                
                // Обновить текущего пользователя
                _currentUser.Login = userToUpdate.Login;
                _currentUser.Email = userToUpdate.Email;
                
                MessageBox.Show("Данные успешно сохранены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод валидации email
        private bool IsValidEmail(string email)
        {
            try 
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch 
            {
                return false;
            }
        }

        private void ViewOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement element && element.Tag is int orderId)
                {
                    var detailsWindow = new PurchaseDetailsWindow(_dbContext, orderId);
                    detailsWindow.Owner = this;
                    detailsWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии деталей заказа: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrdersGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Можно добавить дополнительную логику при выборе заказа
        }
    }
}
