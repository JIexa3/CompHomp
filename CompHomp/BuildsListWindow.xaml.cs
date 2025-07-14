using System.Windows;
using CompHomp.Services;
using CompHomp.Models;
using CompHomp.Data;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CompHomp
{
    public partial class BuildsListWindow : Window
    {
        private readonly BuildService _buildService;
        private ObservableCollection<Build> _builds;

        public BuildsListWindow(BuildService buildService)
        {
            InitializeComponent();
            _buildService = buildService;
            
            // Инициализируем ObservableCollection
            _builds = new ObservableCollection<Build>();
            BuildsDataGrid.ItemsSource = _builds;

            Loaded += async (s, e) => await LoadBuildsAsync();
            InitializeEventHandlers();
        }

        private async Task LoadBuildsAsync()
        {
            try 
            {
                var builds = await _buildService.GetAllBuildsAsync();
                
                // Используем Dispatcher для обновления UI
                await Dispatcher.InvokeAsync(() =>
                {
                    // Очищаем и перезаполняем ObservableCollection
                    _builds.Clear();
                    foreach (var build in builds)
                    {
                        _builds.Add(build);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сборок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeEventHandlers()
        {
            RefreshButton.Click += (s, e) => LoadBuildsAsync();
            DeleteBuildButton.Click += DeleteBuild;
            EditBuildButton.Click += EditBuildButton_Click;
        }

        private void DeleteBuild(object sender, RoutedEventArgs e)
        {
            var selectedBuild = BuildsDataGrid.SelectedItem as Build;
            if (selectedBuild != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить сборку {selectedBuild.Name}?", 
                    "Подтверждение удаления", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    _buildService.DeleteBuild(selectedBuild.Id);
                    LoadBuildsAsync();
                }
            }
            else
            {
                MessageBox.Show("Выберите сборку для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditBuildButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBuild = BuildsDataGrid.SelectedItem as Build;
            if (selectedBuild != null)
            {
                var editBuildWindow = new EditBuildWindow(_buildService, selectedBuild);
                editBuildWindow.ShowDialog();
                
                await LoadBuildsAsync(); // Обновляем список после редактирования
            }
            else
            {
                MessageBox.Show("Выберите сборку для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
