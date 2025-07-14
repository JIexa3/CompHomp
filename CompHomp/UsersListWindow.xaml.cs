using System.Windows;
using CompHomp.Services;
using CompHomp.Models;
using CompHomp.Data;
using System.Linq;

namespace CompHomp
{
    public partial class UsersListWindow : Window
    {
        private readonly UserService _userService;

        public UsersListWindow(UserService userService)
        {
            InitializeComponent();
            _userService = userService;
            LoadUsers();
            InitializeEventHandlers();
        }

        public void LoadUsers()
        {
            var users = _userService.GetAllUsers()
                .Where(u => !u.IsDeleted) // Исключаем удаленных пользователей
                .OrderBy(u => u.Login)
                .ToList();
            UsersDataGrid.ItemsSource = users;
        }

        private void InitializeEventHandlers()
        {
            RefreshButton.Click += (s, e) => LoadUsers();
            DeleteUserButton.Click += DeleteUser;
            EditUserButton.Click += EditUser;
        }

        private void DeleteUser(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as User;
            if (selectedUser != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить пользователя {selectedUser.Login}?", 
                    "Подтверждение удаления", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    _userService.DeleteUser(selectedUser.Id);
                    LoadUsers();
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUser(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as User;
            if (selectedUser != null)
            {
                var editUserWindow = new EditUserWindow(_userService, selectedUser);
                editUserWindow.ShowDialog();
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
