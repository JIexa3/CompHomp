using System;
using System.Windows;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using CompHomp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Windows.Controls;
using CompHomp.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Drawing = DocumentFormat.OpenXml.Wordprocessing.Drawing;

namespace CompHomp
{
    public partial class ReportGeneratorWindow : Window
    {
        private readonly AppDbContext _context;
        private WordprocessingDocument _document;

        public ReportGeneratorWindow(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
            InitializeWindow();
            ReportTypeComboBox.SelectedIndex = 0; // Устанавливаем первый элемент по умолчанию
        }

        private void InitializeWindow()
        {
            // Установка дат по умолчанию
            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Now;
        }

        private void InitializeDates()
        {
            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Now;
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ReportTypeComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Пожалуйста, выберите тип отчета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Пожалуйста, выберите период для отчета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var startDate = StartDatePicker.SelectedDate.Value;
                var endDate = EndDatePicker.SelectedDate.Value;

                if (startDate > endDate)
                {
                    MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                GenerateButton.IsEnabled = false; // Отключаем кнопку на время генерации
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait; // Меняем курсор на ожидание

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Word Document (*.docx)|*.docx",
                    FileName = $"Отчет_{DateTime.Now:yyyy-MM-dd}",
                    DefaultExt = ".docx",
                    AddExtension = true
                };

                var dialogResult = saveFileDialog.ShowDialog();

                if (dialogResult == true)
                {
                    try
                    {
                        using (_document = WordprocessingDocument.Create(saveFileDialog.FileName, WordprocessingDocumentType.Document))
                        {
                            await GenerateReport(saveFileDialog.FileName, startDate, endDate);
                        }
                        MessageBox.Show("Отчет успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Спрашиваем пользователя, хочет ли он открыть файл
                        var openFileResult = MessageBox.Show("Хотите открыть созданный отчет?", "Открыть отчет", 
                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (openFileResult == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start("explorer.exe", saveFileDialog.FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                GenerateButton.IsEnabled = true; // Включаем кнопку обратно
                System.Windows.Input.Mouse.OverrideCursor = null; // Возвращаем стандартный курсор
            }
        }

        private async System.Threading.Tasks.Task GenerateReport(string filePath, DateTime startDate, DateTime endDate)
        {
            var mainPart = _document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Добавляем заголовок
            var reportTitle = GetReportTitle();
            AddHeading(body, reportTitle);

            // Добавляем информацию о периоде
            AddParagraph(body, $"Период: с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}");

            switch (ReportTypeComboBox.SelectedIndex)
            {
                case 0: // Отчет по продажам
                    await GenerateSalesReport(body, startDate, endDate);
                    break;
                case 1: // Отчет по пользователям
                    await GenerateUsersReport(body);
                    break;
                case 2: // Отчет по сборкам
                    await GenerateBuildsReport(body, startDate, endDate);
                    break;
            }

            mainPart.Document.Save();
        }

        private string GetReportTitle()
        {
            if (ReportTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content.ToString() ?? "Отчет";
            }
            return "Отчет";
        }

        private async Task GenerateSalesReport(Body body, DateTime startDate, DateTime endDate)
        {
            // Устанавливаем конец дня для конечной даты
            endDate = endDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            
            var sales = await _context.PurchaseHistories
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Build)
                .Where(o => o.PurchaseDate >= startDate.Date && o.PurchaseDate <= endDate)
                .ToListAsync();

            AddHeading(body, "Статистика продаж", 2);
            
            var totalSales = sales.Count;
            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var paidOrders = sales.Count(s => s.OrderStatus == "Оплачен");
            
            AddParagraph(body, $"Общее количество заказов: {totalSales}");
            AddParagraph(body, $"Оплаченных заказов: {paidOrders}");
            AddParagraph(body, $"Общая выручка: {totalRevenue:C2}");
            AddParagraph(body, "");

            // Добавляем популярные сборки в текстовом виде
            var popularBuilds = sales
                .SelectMany(s => s.Items)
                .Where(i => i.Build != null)
                .GroupBy(i => i.Build.Name)
                .Select(g => new { Name = g.Key, Count = g.Sum(i => i.Quantity) })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            if (popularBuilds.Any())
            {
                AddHeading(body, "Топ 5 популярных сборок:", 3);
                foreach (var build in popularBuilds)
                {
                    AddParagraph(body, $"- {build.Name}: {build.Count} шт.");
                }
                AddParagraph(body, "");
            }

            if (IncludeDetailsCheckBox.IsChecked == true)
            {
                AddHeading(body, "Детальная информация по продажам", 2);

                foreach (var sale in sales.OrderByDescending(s => s.PurchaseDate))
                {
                    AddParagraph(body, $"Заказ #{sale.Id} - {sale.PurchaseDate:dd.MM.yyyy HH:mm}");
                    AddParagraph(body, $"Покупатель: {sale.User?.Login ?? "Неизвестно"}");
                    AddParagraph(body, $"Статус: {sale.OrderStatus}");
                    AddParagraph(body, $"Сумма: {sale.TotalAmount:C2}");
                    
                    AddParagraph(body, "Состав заказа:");
                    foreach (var item in sale.Items)
                    {
                        AddParagraph(body, $"- Сборка: {item.Build?.Name ?? "Неизвестно"} (Количество: {item.Quantity}, Цена: {item.Price:C2})");
                    }
                    AddParagraph(body, "");
                }
            }
        }

        private async Task GenerateUsersReport(Body body)
        {
            var users = await _context.Users.ToListAsync();
            
            AddHeading(body, "Статистика пользователей", 2);
            AddParagraph(body, $"Общее количество пользователей: {users.Count}");
            
            var activeUsers = users.Count(u => u.IsActive);
            AddParagraph(body, $"Активных пользователей: {activeUsers}");
            AddParagraph(body, $"Заблокированных пользователей: {users.Count - activeUsers}");

            if (IncludeDetailsCheckBox.IsChecked == true)
            {
                AddHeading(body, "Список пользователей", 3);
                foreach (var user in users)
                {
                    AddParagraph(body, $"Пользователь: {user.Login}");
                    AddParagraph(body, $"Email: {user.Email}");
                    AddParagraph(body, $"Статус: {(user.IsActive ? "Активен" : "Заблокирован")}");
                    AddParagraph(body, "");
                }
            }
        }

        private async Task GenerateBuildsReport(Body body, DateTime startDate, DateTime endDate)
        {
            var builds = await _context.Builds
                .Include(b => b.User)
                .Where(b => b.CreatedDate >= startDate && b.CreatedDate <= endDate)
                .ToListAsync();

            AddHeading(body, "Статистика сборок", 2);
            AddParagraph(body, $"Общее количество сборок: {builds.Count}");
            
            var moderatedBuilds = builds.Count(b => b.Status != BuildStatus.Pending);
            var approvedBuilds = builds.Count(b => b.Status == BuildStatus.Approved);
            var rejectedBuilds = builds.Count(b => b.Status == BuildStatus.Rejected);
            
            AddParagraph(body, $"Проверенных сборок: {moderatedBuilds}");
            AddParagraph(body, $"Одобренных сборок: {approvedBuilds}");
            AddParagraph(body, $"Отклоненных сборок: {rejectedBuilds}");
            AddParagraph(body, $"Ожидают проверки: {builds.Count - moderatedBuilds}");

            if (IncludeDetailsCheckBox.IsChecked == true)
            {
                AddHeading(body, "Детальная информация по сборкам", 3);
                foreach (var build in builds)
                {
                    AddParagraph(body, $"Сборка #{build.Id}");
                    AddParagraph(body, $"Название: {build.Name}");
                    AddParagraph(body, $"Автор: {build.User.Login}");
                    AddParagraph(body, $"Создана: {build.CreatedDate:dd.MM.yyyy}");
                    AddParagraph(body, $"Статус: {GetBuildStatusDisplay(build.Status)}");
                    AddParagraph(body, $"Базовая цена: {build.TotalPrice:C2}");
                    AddParagraph(body, "");
                }
            }
        }

        private string GetBuildStatusDisplay(BuildStatus status)
        {
            return status switch
            {
                BuildStatus.Pending => "На рассмотрении",
                BuildStatus.Approved => "Одобрено",
                BuildStatus.Rejected => "Отклонено",
                _ => status.ToString()
            };
        }

        private void AddHeading(Body body, string text, int level = 1)
        {
            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = $"Heading{level}" }
                ),
                new Run(new Text(text))
            );
            body.AppendChild(paragraph);
        }

        private void AddParagraph(Body body, string text)
        {
            var paragraph = new Paragraph(
                new Run(new Text(text))
            );
            body.AppendChild(paragraph);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
