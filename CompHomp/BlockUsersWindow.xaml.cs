using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CompHomp.Services;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp
{
    public partial class BlockUsersWindow : Window
    {
        private readonly UserService _userService;
        public ObservableCollection<UserViewModel> Users { get; set; }

        public BlockUsersWindow(UserService userService)
        {
            InitializeComponent();
            _userService = userService;
            LoadUsers();
            InitializeEventHandlers();
        }

        private void LoadUsers()
        {
            var users = _userService.GetAllUsers();
            Users = new ObservableCollection<UserViewModel>(
                users.Where(u => !u.IsDeleted) // Исключаем удаленных пользователей
                    .Select(u => new UserViewModel
                    {
                        Id = u.Id,
                        Username = u.Login,
                        Email = u.Email,
                        Role = u.Role,
                        IsActive = !u.IsBlocked,
                        IsSelected = false
                    })
                    .OrderBy(u => u.Username)
            );
            UsersDataGrid.ItemsSource = Users;

            System.Diagnostics.Debug.WriteLine("Users loaded. Total count: " + Users.Count);
            foreach (var user in Users)
            {
                System.Diagnostics.Debug.WriteLine($"User: {user.Username}, IsActive: {user.IsActive}, IsBlocked: {user.IsBlocked}");
            }
        }

        private void InitializeEventHandlers()
        {
            BlockSelectedButton.Click += BlockSelectedUsers;
            UnblockSelectedButton.Click += UnblockSelectedUsers;
            CancelButton.Click += (s, e) => Close();
        }

        private void BlockSelectedUsers(object sender, RoutedEventArgs e)
        {
            var selectedUsers = Users.Where(u => u.IsSelected).ToList();
            if (selectedUsers.Any())
            {
                foreach (var user in selectedUsers)
                {
                    _userService.UpdateUserStatus(user.Id, false);
                }
                LoadUsers(); // Refresh the list
                MessageBox.Show("Выбранные пользователи заблокированы", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Выберите пользователей для блокировки", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UnblockSelectedUsers(object sender, RoutedEventArgs e)
        {
            var selectedUsers = Users.Where(u => u.IsSelected).ToList();
            if (selectedUsers.Any())
            {
                foreach (var user in selectedUsers)
                {
                    _userService.UpdateUserStatus(user.Id, true);
                }
                LoadUsers(); // Refresh the list
                MessageBox.Show("Выбранные пользователи разблокированы", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Выберите пользователей для разблокировки", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public class UserViewModel
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public UserRole Role { get; set; }
            public bool IsActive { get; set; }
            public bool IsSelected { get; set; }
            public bool IsBlocked { get; set; }
        }
    }
}
