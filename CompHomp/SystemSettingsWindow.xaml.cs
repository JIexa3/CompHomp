using System;
using System.Windows;
using System.Windows.Input;
using CompHomp.Services;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp
{
    public partial class SystemSettingsWindow : Window
    {
        private readonly SystemSettingsService _settingsService;
        private readonly User _currentUser;

        public SystemSettingsWindow(SystemSettingsService settingsService, IAuditService auditService, User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _settingsService = settingsService;
            LoadSystemSettings();
            InitializeEventHandlers();
        }

        private void LoadSystemSettings()
        {
            var settings = _settingsService.GetSystemSettings();

            MaxLoginAttemptsTextBox.Text = settings.MaxLoginAttempts.ToString();
            LockoutDurationTextBox.Text = settings.LockoutDurationMinutes.ToString();
            MinPasswordLengthTextBox.Text = settings.MinPasswordLength.ToString();
            ComplexPasswordCheckBox.IsChecked = settings.RequireComplexPassword;
            AuditLogRetentionTextBox.Text = settings.AuditLogRetentionDays.ToString();
        }

        private void InitializeEventHandlers()
        {
            SaveButton.Click += SaveSystemSettings;
            CancelButton.Click += (s, e) => Close();
        }

        private void SaveSystemSettings(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                var newSettings = new SystemSettings
                {
                    MaxLoginAttempts = int.Parse(MaxLoginAttemptsTextBox.Text),
                    LockoutDurationMinutes = int.Parse(LockoutDurationTextBox.Text),
                    MinPasswordLength = int.Parse(MinPasswordLengthTextBox.Text),
                    RequireComplexPassword = ComplexPasswordCheckBox.IsChecked ?? false,
                    AuditLogRetentionDays = int.Parse(AuditLogRetentionTextBox.Text)
                };

                try
                {
                    _settingsService.UpdateSystemSettings(newSettings);
                    MessageBox.Show("Настройки системы обновлены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            if (!int.TryParse(MaxLoginAttemptsTextBox.Text, out int maxAttempts) || maxAttempts <= 0)
            {
                MessageBox.Show("Максимальное количество попыток должно быть положительным числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(LockoutDurationTextBox.Text, out int lockoutDuration) || lockoutDuration < 0)
            {
                MessageBox.Show("Длительность блокировки не может быть отрицательной", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(MinPasswordLengthTextBox.Text, out int minLength) || minLength <= 0)
            {
                MessageBox.Show("Минимальная длина пароля должна быть положительным числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(AuditLogRetentionTextBox.Text, out int retentionDays) || retentionDays <= 0)
            {
                MessageBox.Show("Период хранения журнала должен быть положительным числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}
