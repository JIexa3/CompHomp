using System.Windows;
using System.Windows.Controls; 
using System.Linq;
using CompHomp.Services;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp
{
    public partial class EditUserWindow : Window
    {
        private readonly UserService _userService;
        private readonly User _user;

        public EditUserWindow(UserService userService, User user)
        {
            InitializeComponent();
            _userService = userService;
            _user = user;

            LoadUserData();
            InitializeEventHandlers();
        }

        private void LoadUserData()
        {
            UsernameTextBox.Text = _user.Login;
            EmailTextBox.Text = _user.Email;
            
            // Обновляем логику выбора роли
            RoleComboBox.SelectedItem = _user.Role == UserRole.Admin 
                ? RoleComboBox.Items.Cast<ComboBoxItem>().First(item => item.Tag.ToString() == "Admin")
                : RoleComboBox.Items.Cast<ComboBoxItem>().First(item => item.Tag.ToString() == "Customer");
            
            StatusComboBox.SelectedItem = _user.IsBlocked 
                ? StatusComboBox.Items.Cast<ComboBoxItem>().First(item => item.Content.ToString() == "Заблокирован")
                : StatusComboBox.Items.Cast<ComboBoxItem>().First(item => item.Content.ToString() == "Активный");
            
            // Добавляем отладочный вывод
            System.Diagnostics.Debug.WriteLine($"Loading User - ID: {_user.Id}, Role: {_user.Role}, IsBlocked: {_user.IsBlocked}");
        }

        private void InitializeEventHandlers()
        {
            SaveButton.Click += SaveUser;
            CancelButton.Click += (s, e) => Close();
        }

        private void SaveUser(object sender, RoutedEventArgs e)
        {
            var selectedRole = ((ComboBoxItem)RoleComboBox.SelectedItem).Tag.ToString();
            var newRole = selectedRole == "Admin" ? UserRole.Admin : UserRole.Customer;

            var selectedStatus = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();
            var isBlocked = selectedStatus == "Заблокирован";

            _user.Login = UsernameTextBox.Text;
            _user.Email = EmailTextBox.Text;
            _user.Role = newRole;
            _user.IsBlocked = isBlocked;

            _userService.UpdateUser(_user);
            DialogResult = true;
            Close();
        }
    }
}
