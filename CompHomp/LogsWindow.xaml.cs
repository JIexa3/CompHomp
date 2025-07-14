using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Microsoft.Win32;
using CompHomp.Models;
using CompHomp.Services;

namespace CompHomp
{
    public partial class LogsWindow : Window
    {
        private readonly IAuditService _auditService;
        private List<AuditLog> _currentLogs;
        private int _currentPage = 1;
        private const int PageSize = 50;

        public LogsWindow(IAuditService auditService)
        {
            InitializeComponent();
            _auditService = auditService;

            // Инициализация ComboBox с типами событий
            EventTypeComboBox.Items.Clear();
            EventTypeComboBox.Items.Add(new ComboBoxItem { Content = "Все события" });
            EventTypeComboBox.Items.Add(new ComboBoxItem { Content = "Вход" });
            EventTypeComboBox.Items.Add(new ComboBoxItem { Content = "Ошибка" });
            EventTypeComboBox.Items.Add(new ComboBoxItem { Content = "Отправка сборки" });
            EventTypeComboBox.Items.Add(new ComboBoxItem { Content = "Изменение настроек" });
            EventTypeComboBox.SelectedIndex = 0;

            // Установка начальной даты
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-7);
            EndDatePicker.SelectedDate = DateTime.Today;

            // Подписка на события
            ApplyFilterButton.Click += ApplyFilterButton_Click;
           
            ClearLogsButton.Click += ClearLogsButton_Click;
            EventTypeComboBox.SelectionChanged += (s, e) => ApplyFilter();
            PrevPageButton.Click += (s, e) => ChangePage(-1);
            NextPageButton.Click += (s, e) => ChangePage(1);

            LoadLogs();
        }

        private void LoadLogs()
        {
            try
            {
                _currentLogs = _auditService.GetAuditLogs(_currentPage, PageSize);
                FilterLogs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке логов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Error in LoadLogs: {ex.Message}");
                _currentLogs = new List<AuditLog>();
                UpdateLogsDisplay();
            }
        }

        private void FilterLogs()
        {
            try
            {
                string selectedEventType = ((ComboBoxItem)EventTypeComboBox.SelectedItem)?.Content?.ToString();
                var startDate = StartDatePicker.SelectedDate;
                var endDate = EndDatePicker.SelectedDate?.AddDays(1);

                var filteredLogs = _currentLogs;
                if (selectedEventType != "Все события")
                {
                    filteredLogs = filteredLogs.Where(l => l.EventType == selectedEventType).ToList();
                }
                if (startDate.HasValue)
                {
                    filteredLogs = filteredLogs.Where(l => l.Timestamp >= startDate.Value).ToList();
                }
                if (endDate.HasValue)
                {
                    filteredLogs = filteredLogs.Where(l => l.Timestamp <= endDate.Value).ToList();
                }

                _currentLogs = filteredLogs;
                UpdateLogsDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FilterLogs: {ex.Message}");
            }
        }

        private void UpdateLogsDisplay()
        {
            try
            {
                var displayLogs = _currentLogs.Select(log => new
                {
                    Timestamp = log.Timestamp.ToString("dd.MM.yyyy HH:mm:ss"),
                    Username = log.Username ?? "Неизвестный пользователь",
                    LogType = log.EventType ?? "Неизвестный тип",
                    Message = log.Description ?? ""
                }).ToList();

                LogsDataGrid.ItemsSource = displayLogs;
                
                int totalLogs = _auditService.GetAuditLogCount();
                int totalPages = (totalLogs + PageSize - 1) / PageSize;
                
                TotalLogsTextBlock.Text = $"Всего записей: {totalLogs}";
                FilteredLogsTextBlock.Text = $"Страница {_currentPage} из {totalPages}";
                
                PrevPageButton.IsEnabled = _currentPage > 1;
                NextPageButton.IsEnabled = _currentPage < totalPages;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateLogsDisplay: {ex.Message}");
            }
        }

        private void ChangePage(int delta)
        {
            _currentPage += delta;
            LoadLogs();
        }

        private void ApplyFilter()
        {
            if (IsLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Applying filter...");
                _currentPage = 1; // Сброс на первую страницу при применении фильтра
                LoadLogs();
            }
        }

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

       

        private void ClearLogsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Очистка логов теперь происходит автоматически на основе настроек периода хранения.\n" +
                "Чтобы изменить период хранения, используйте окно настроек системы.",
                "Информация",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}
