using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using CompHomp.Models;
using CompHomp.Services;
using CompHomp.Data;
using Microsoft.EntityFrameworkCore;

namespace CompHomp
{
    public partial class SalesStatisticsWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly IStatisticsService _statisticsService;

        public SalesStatisticsWindow(AppDbContext context, IStatisticsService statisticsService)
        {
            InitializeComponent();
            _context = context;
            _statisticsService = statisticsService;

            // Устанавливаем диапазон дат по умолчанию
            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Now;

            // Загружаем статистику при инициализации
            LoadSalesStatistics();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSalesStatistics();
        }

        private void LoadSalesStatistics()
        {
            try
            {
                var dailyStats = _statisticsService.GetDailySalesStatistics(StartDatePicker.SelectedDate, EndDatePicker.SelectedDate);
                var totalStats = _statisticsService.GetTotalSalesStatistics(StartDatePicker.SelectedDate, EndDatePicker.SelectedDate);
                var buildStats = _statisticsService.GetBuildSalesStatistics(StartDatePicker.SelectedDate, EndDatePicker.SelectedDate);

                // Проверка наличия данных
                if (!dailyStats.Any())
                {
                    // Очищаем элементы
                    TotalRevenueTextBlock.Text = "0 ₽";
                    TotalSalesCountTextBlock.Text = "0";
                    AverageOrderValueTextBlock.Text = "0 ₽";

                    SalesLineChart.Series.Clear();
                    BuildCategoryPieChart.Series.Clear();
                    TopBuildsListView.ItemsSource = null;
                    TopBuildsListView.Visibility = Visibility.Collapsed;
                    NoBuildsTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                // Обновляем текстовые блоки
                TotalRevenueTextBlock.Text = $"{totalStats.TotalRevenue:N2} ₽";
                TotalSalesCountTextBlock.Text = totalStats.TotalSalesCount.ToString();
                AverageOrderValueTextBlock.Text = $"{totalStats.AverageOrderValue:N2} ₽";

                // Настройка линейного графика продаж
                ConfigureSalesLineChart(dailyStats);

                // Настройка круговой диаграммы с использованием статистики по сборкам
                ConfigureBuildCategoryPieChart(buildStats);

                // Настройка списка топ сборок
                ConfigureTopBuildsListView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статистики: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureSalesLineChart(List<SaleStatistics> dailyStats)
        {
            if (dailyStats == null || !dailyStats.Any())
            {
                SalesLineChart.Series.Clear();
                return;
            }

            var salesData = dailyStats.OrderBy(s => s.SaleDate).ToList();

            SalesLineChart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Выручка",
                    Values = new ChartValues<decimal>(salesData.Select(s => s.TotalRevenue)),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 8,
                    LineSmoothness = 0.6,
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 52, 152, 219)),
                    Stroke = new SolidColorBrush(Color.FromRgb(52, 152, 219))
                },
                new LineSeries
                {
                    Title = "Количество продаж",
                    Values = new ChartValues<int>(salesData.Select(s => s.TotalSalesCount)),
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 8,
                    LineSmoothness = 0.6,
                    StrokeThickness = 2,
                    ScalesYAt = 1,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 46, 204, 113)),
                    Stroke = new SolidColorBrush(Color.FromRgb(46, 204, 113))
                }
            };

            // Настройка осей
            SalesLineChart.AxisX[0].Labels = salesData.Select(s => s.SaleDate.ToString("dd.MM")).ToList();
            SalesLineChart.AxisX[0].Separator = new Separator
            {
                Step = Math.Max(1, salesData.Count / 10), // Показываем максимум 10 меток на оси X
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 2 },
                Stroke = new SolidColorBrush(Color.FromRgb(224, 224, 224))
            };

            // Основная ось Y для выручки
            SalesLineChart.AxisY[0].LabelFormatter = value => value.ToString("N0") + " ₽";
            SalesLineChart.AxisY[0].Separator = new Separator
            {
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 2 },
                Stroke = new SolidColorBrush(Color.FromRgb(224, 224, 224))
            };

            // Дополнительная ось Y для количества продаж
            SalesLineChart.AxisY.Add(new Axis
            {
                Position = AxisPosition.RightTop,
                Title = "Количество продаж",
                LabelFormatter = value => value.ToString("N0"),
                Separator = new Separator
                {
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 2 },
                    Stroke = new SolidColorBrush(Color.FromRgb(224, 224, 224))
                }
            });
        }

        private void ConfigureBuildCategoryPieChart(List<SaleStatistics> buildSalesStats)
        {
            try 
            {
                // Отладочная информация о статистике
                MessageBox.Show($"Получено статистических записей о сборках: {buildSalesStats?.Count ?? 0}", 
                    "Отладка статистики сборок", MessageBoxButton.OK, MessageBoxImage.Information);

                BuildCategoryPieChart.Series.Clear(); // Очищаем предыдущие данные

                if (buildSalesStats == null || !buildSalesStats.Any())
                {
                    MessageBox.Show("Нет данных для построения диаграммы сборок", 
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Цвета для секторов диаграммы
                var colors = new List<Color>
                {
                    Color.FromRgb(52, 152, 219),   // Blue
                    Color.FromRgb(46, 204, 113),   // Green
                    Color.FromRgb(155, 89, 182),   // Purple
                    Color.FromRgb(230, 126, 34),   // Orange
                    Color.FromRgb(231, 76, 60),    // Red
                    Color.FromRgb(52, 73, 94)      // Dark Gray
                };

                for (int i = 0; i < buildSalesStats.Count; i++)
                {
                    var stat = buildSalesStats[i];
                    var color = colors[i % colors.Count];

                    BuildCategoryPieChart.Series.Add(new PieSeries
                    {
                        Title = $"{stat.BuildName} ({stat.TotalSalesCount} шт.)",
                        Values = new ChartValues<decimal> { stat.TotalRevenue },
                        DataLabels = true,
                        LabelPoint = point => $"{point.Participation:P1}\n{stat.TotalRevenue:N0} ₽",
                        Fill = new SolidColorBrush(color),
                        Stroke = new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B)),
                        StrokeThickness = 2
                    });
                }

                BuildCategoryPieChart.LegendLocation = LegendLocation.Right;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при построении диаграммы: {ex.Message}\n\n" +
                    $"Внутренняя ошибка: {ex.InnerException?.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureTopBuildsListView()
        {
            var startDate = StartDatePicker.SelectedDate ?? DateTime.Now.AddMonths(-1);
            var endDate = EndDatePicker.SelectedDate ?? DateTime.Now;

            var topBuilds = _context.PurchaseHistoryItems
                .Include(phi => phi.Build)
                .Where(phi => phi.Build != null && 
                       phi.PurchaseHistory.PurchaseDate >= startDate && 
                       phi.PurchaseHistory.PurchaseDate <= endDate)
                .GroupBy(phi => phi.Build)
                .Select(g => new 
                { 
                    Name = g.Key.Name ?? "Unnamed Build", 
                    SalesCount = g.Sum(phi => phi.Quantity),
                    TotalRevenue = g.Sum(phi => phi.Price * phi.Quantity)
                })
                .OrderByDescending(b => b.SalesCount)
                .Take(5)
                .ToList();

            if (topBuilds.Any())
            {
                TopBuildsListView.ItemsSource = topBuilds.Select(b => new
                {
                    b.Name,
                    SalesCount = $"Продано: {b.SalesCount} шт. на сумму {b.TotalRevenue:N2} ₽"
                });
                TopBuildsListView.Visibility = Visibility.Visible;
                NoBuildsTextBlock.Visibility = Visibility.Collapsed;
            }
            else 
            {
                TopBuildsListView.Visibility = Visibility.Collapsed;
                NoBuildsTextBlock.Visibility = Visibility.Visible;
                NoBuildsTextBlock.Text = "За выбранный период нет проданных сборок";
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Устанавливаем DialogResult после инициализации окна
            if (Owner != null)
            {
                DialogResult = true;
            }
        }
    }
}
