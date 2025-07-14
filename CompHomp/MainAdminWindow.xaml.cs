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
using CompHomp.Models;
using CompHomp.Services;
using CompHomp.Data;

namespace CompHomp
{
    /// <summary>
    /// Логика взаимодействия для MainAdminWindow.xaml
    /// </summary>
    public partial class MainAdminWindow : Window
    {
        private readonly UserService _userService;
        private readonly BuildService _buildService;
        private readonly OrderService _orderService;
        private readonly AuditService _auditService;
        private readonly SystemSettingsService _systemSettingsService;
        private readonly AppDbContext _context;
        private readonly User _currentUser;
        private readonly IStatisticsService _statisticsService;

        public MainAdminWindow(User currentUser, AppDbContext context, IStatisticsService statisticsService)
        {
            _context = context;
            _statisticsService = statisticsService;
            _currentUser = currentUser;
            _userService = new UserService(_context);
            _buildService = new BuildService(_context);
            _orderService = new OrderService(_context);
            _auditService = new AuditService(_context);
            _systemSettingsService = new SystemSettingsService(_context, _auditService, _currentUser);
            InitializeComponent();
            InitializeEventHandlers();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context.Dispose();
        }

        private void InitializeEventHandlers()
        {
            UsersListButton.Click += (s, e) => OpenUsersListWindow();
            AddUserButton.Click += (s, e) => OpenAddUserWindow();
            BlockUsersButton.Click += (s, e) => OpenBlockUsersWindow();
            BuildsListButton.Click += (s, e) => OpenBuildsListWindow();
            CreateBuildButton.Click += (s, e) => OpenCreateBuildWindow();
            ModerateBuildButton.Click += (s, e) => OpenBuildModerationWindow();
            SalesStatisticsButton.Click += (s, e) => OpenSalesStatisticsWindow();
            SystemSettingsButton.Click += (s, e) => OpenSystemSettingsWindow();
            ComponentManagementButton.Click += (s, e) => OpenComponentManagementWindow();
            LogsButton.Click += (s, e) => ShowLogsWindow_Click(s, e);
            ReportGeneratorButton.Click += (s, e) => OpenReportGeneratorWindow();
            ExitButton.Click += (s, e) =>
            {
                var loginWindow = new MainWindow();
                loginWindow.Show();
                Close();
            };
        }

        private void OpenUsersListWindow()
        {
            var usersListWindow = new UsersListWindow(_userService);
            usersListWindow.Show();
        }

        private void OpenAddUserWindow()
        {
            var addUserWindow = new AddUserWindow(_userService);
            if (addUserWindow.ShowDialog() == true)
            {
                // Обновляем список пользователей, если они отображаются
                if (Application.Current.Windows.OfType<UsersListWindow>().FirstOrDefault() is UsersListWindow usersListWindow)
                {
                    usersListWindow.LoadUsers();
                }
            }
        }

        private void OpenBlockUsersWindow()
        {
            var blockUsersWindow = new BlockUsersWindow(_userService);
            blockUsersWindow.Show();
        }

        private void OpenBuildsListWindow()
        {
            var buildsListWindow = new BuildsListWindow(_buildService);
            buildsListWindow.Show();
        }

        private void OpenCreateBuildWindow()
        {
            var createBuildWindow = new CreateBuildWindow(_currentUser);
            createBuildWindow.Owner = this;
            createBuildWindow.ShowDialog();
        }

        private void OpenBuildModerationWindow()
        {
            var buildModerationWindow = new BuildModerationWindow(_buildService);
            buildModerationWindow.Show();
        }

        private void OpenSalesStatisticsWindow()
        {
            var salesStatisticsWindow = new SalesStatisticsWindow(_context, _statisticsService);
            salesStatisticsWindow.Show();
        }

        private void OpenSystemSettingsWindow()
        {
            var systemSettingsWindow = new SystemSettingsWindow(_systemSettingsService, _auditService, _currentUser);
            systemSettingsWindow.Closed += (s, e) => ApplySystemSettings();
            systemSettingsWindow.ShowDialog();
        }

        private void ApplySystemSettings()
        {
            var settings = _systemSettingsService.GetSystemSettings();
            // Применяем настройки к текущей сессии
            if (_userService != null)
            {
                _userService.UpdateSecuritySettings(
                    settings.MaxLoginAttempts,
                    settings.LockoutDurationMinutes,
                    settings.MinPasswordLength,
                    settings.RequireComplexPassword
                );
            }

            if (_auditService != null)
            {
                _auditService.UpdateRetentionPeriod(settings.AuditLogRetentionDays, _currentUser);
            }
        }

        private void OpenComponentManagementWindow()
        {
            var componentManagementWindow = ComponentManagementWindow.GetInstance();
            componentManagementWindow.Show();
        }

        private void ShowLogsWindow_Click(object sender, RoutedEventArgs e)
        {
            var logsWindow = new LogsWindow(_auditService);
            logsWindow.ShowDialog();
        }

        private void OpenReportGeneratorWindow()
        {
            var reportGeneratorWindow = new ReportGeneratorWindow(_context);
            reportGeneratorWindow.Show();
        }
    }
}
