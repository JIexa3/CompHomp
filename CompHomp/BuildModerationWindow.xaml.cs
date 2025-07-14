using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CompHomp.Services;
using CompHomp.Models;
using CompHomp.Data;
using System.Threading.Tasks;

namespace CompHomp
{
    public partial class BuildModerationWindow : Window
    {
        private readonly BuildService _buildService;
        public ObservableCollection<Build> Builds { get; set; }

        public BuildModerationWindow(BuildService buildService)
        {
            InitializeComponent();
            _buildService = buildService;
            DataContext = this;
            LoadBuildsAsync();
            InitializeEventHandlers();
        }

        private async Task LoadBuildsAsync()
        {
            var builds = await _buildService.GetBuildsForModeration();
            Builds = new ObservableCollection<Build>(builds);
            BuildsDataGrid.ItemsSource = Builds;
        }

        private void InitializeEventHandlers()
        {
            RefreshButton.Click += async (s, e) => await LoadBuildsAsync();
            CancelButton.Click += (s, e) => Close();
        }

        private async void ApproveBuild(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var build = button?.Tag as Build;

            if (build != null)
            {
                try
                {
                    // Отключаем кнопки на время операции
                    button.IsEnabled = false;
                    
                    await _buildService.ApproveBuild(build.Id);
                    await LoadBuildsAsync();
                    MessageBox.Show("Сборка одобрена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при одобрении сборки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Включаем кнопки обратно
                    button.IsEnabled = true;
                }
            }
        }

        private async void RejectBuild(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var build = button?.Tag as Build;

            if (build != null)
            {
                try
                {
                    // Отключаем кнопки на время операции
                    button.IsEnabled = false;
                    
                    await _buildService.RejectBuild(build.Id);
                    await LoadBuildsAsync();
                    MessageBox.Show("Сборка отклонена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при отклонении сборки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Включаем кнопки обратно
                    button.IsEnabled = true;
                }
            }
        }
    }
}
