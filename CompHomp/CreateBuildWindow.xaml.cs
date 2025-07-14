using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using CompHomp.Models;
using CompHomp.Data;
using CompHomp.Services;

namespace CompHomp
{
    public partial class CreateBuildWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly User _currentUser;
        private readonly BuildService _buildService;
        private string? _selectedImagePath;

        public CreateBuildWindow(User currentUser)
        {
            InitializeComponent();
            _context = new AppDbContext();
            _currentUser = currentUser;
            _buildService = new BuildService(_context);
            LoadComponents();
            
            // Initialize dialog properties
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;
            this.ShowInTaskbar = false;

            // Set default image
            SetDefaultImage();
        }

        public CreateBuildWindow(BuildService buildService, User currentUser)
        {
            InitializeComponent();
            _context = new AppDbContext();
            _currentUser = currentUser;
            _buildService = buildService;
            LoadComponents();
            
            // Initialize dialog properties
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;
            this.ShowInTaskbar = false;

            // Set default image
            SetDefaultImage();
        }

        private void LoadComponents()
        {
            try
            {
                CpuComboBox.ItemsSource = _context.Cpus.ToList();
                GpuComboBox.ItemsSource = _context.Gpus.ToList();
                MotherboardComboBox.ItemsSource = _context.Motherboards.ToList();
                RamComboBox.ItemsSource = _context.Rams.ToList();
                StorageComboBox.ItemsSource = _context.Storages.ToList();
                PsuComboBox.ItemsSource = _context.Psus.ToList();
                CaseComboBox.ItemsSource = _context.Cases.ToList();

                // Добавляем обработчики событий для пересчета цены
                CpuComboBox.SelectionChanged += UpdateTotalPrice;
                GpuComboBox.SelectionChanged += UpdateTotalPrice;
                MotherboardComboBox.SelectionChanged += UpdateTotalPrice;
                RamComboBox.SelectionChanged += UpdateTotalPrice;
                StorageComboBox.SelectionChanged += UpdateTotalPrice;
                PsuComboBox.SelectionChanged += UpdateTotalPrice;
                CaseComboBox.SelectionChanged += UpdateTotalPrice;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки компонентов: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotalPrice(object sender, SelectionChangedEventArgs e)
        {
            decimal totalPrice = 0;
            int totalPower = 0;

            // Проверка совместимости сокетов
            if (CpuComboBox.SelectedItem is Cpu cpu && MotherboardComboBox.SelectedItem is Motherboard mb)
            {
                if (cpu.Socket != mb.Socket)
                {
                    MessageBox.Show($"Внимание! Сокет процессора ({cpu.Socket}) не совместим с сокетом материнской платы ({mb.Socket})",
                        "Несовместимость компонентов", MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (sender == CpuComboBox)
                        MotherboardComboBox.SelectedIndex = -1;
                    else
                        CpuComboBox.SelectedIndex = -1;
                    return;
                }
                totalPrice += cpu.Price + mb.Price;
                totalPower += cpu.TDP;
            }
            else
            {
                if (CpuComboBox.SelectedItem is Cpu selectedCpu)
                {
                    totalPrice += selectedCpu.Price;
                    totalPower += selectedCpu.TDP;
                }
                if (MotherboardComboBox.SelectedItem is Motherboard selectedMb)
                    totalPrice += selectedMb.Price;
            }

            // Добавляем мощность и цену GPU
            if (GpuComboBox.SelectedItem is Gpu gpu)
            {
                totalPrice += gpu.Price;
                totalPower += gpu.TDP;
            }

            // Добавляем остальные компоненты
            if (RamComboBox.SelectedItem is Ram ram)
                totalPrice += ram.Price;
            if (StorageComboBox.SelectedItem is Storage storage)
                totalPrice += storage.Price;
            if (CaseComboBox.SelectedItem is Case computerCase)
                totalPrice += computerCase.Price;

            // Проверяем мощность блока питания
            if (PsuComboBox.SelectedItem is Psu psu)
            {
                totalPrice += psu.Price;
                
                // Добавляем 20% запас мощности для надежности
                int requiredPower = (int)(totalPower * 1.2);
                
                if (psu.Power < requiredPower)
                {
                    MessageBox.Show(
                        $"Внимание! Мощности блока питания ({psu.Power}Вт) недостаточно для данной конфигурации.\n" +
                        $"Требуемая мощность с учетом запаса: {requiredPower}Вт.\n" +
                        "Рекомендуется выбрать более мощный блок питания.",
                        "Недостаточная мощность", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                
                // Обновляем ProgressBar
                PowerProgressBar.Value = totalPower;
                PowerProgressBar.Maximum = psu.Power;
                PowerUsageText.Text = $"Энергопотребление: {totalPower}Вт из {psu.Power}Вт";
            }
            else
            {
                PowerProgressBar.Value = totalPower;
                PowerProgressBar.Maximum = 1000; // Значение по умолчанию
                PowerUsageText.Text = $"Энергопотребление: {totalPower}Вт";
            }

            TotalPriceTextBox.Text = totalPrice.ToString("C");
        }

        private void SetDefaultImage()
        {
            try
            {
                string defaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "default_build.png");
                if (File.Exists(defaultImagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(defaultImagePath);
                    bitmap.EndInit();
                    BuildImage.Source = bitmap;
                    _selectedImagePath = defaultImagePath;
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не показываем пользователю
                Console.WriteLine($"Ошибка загрузки изображения по умолчанию: {ex.Message}");
            }
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.EndInit();
                    BuildImage.Source = bitmap;
                    _selectedImagePath = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выборе изображения: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    SetDefaultImage(); // Возвращаемся к изображению по умолчанию в случае ошибки
                }
            }
        }

        private void CreateBuild_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполнения обязательных полей
                if (string.IsNullOrWhiteSpace(BuildNameTextBox.Text))
                {
                    MessageBox.Show("Пожалуйста, введите название сборки", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка выбора компонентов
                if (CpuComboBox.SelectedItem == null || 
                    MotherboardComboBox.SelectedItem == null ||
                    GpuComboBox.SelectedItem == null ||
                    RamComboBox.SelectedItem == null ||
                    StorageComboBox.SelectedItem == null ||
                    PsuComboBox.SelectedItem == null ||
                    CaseComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите все необходимые компоненты", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка совместимости сокетов
                var cpu = (Cpu)CpuComboBox.SelectedItem;
                var motherboard = (Motherboard)MotherboardComboBox.SelectedItem;
                if (cpu.Socket != motherboard.Socket)
                {
                    MessageBox.Show($"Процессор ({cpu.Socket}) не совместим с материнской платой ({motherboard.Socket})",
                        "Ошибка совместимости", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка мощности блока питания
                var gpu = (Gpu)GpuComboBox.SelectedItem;
                var psu = (Psu)PsuComboBox.SelectedItem;
                int totalPower = cpu.TDP + gpu.TDP;
                int requiredPower = (int)(totalPower * 1.2); // 20% запас

                if (psu.Power < requiredPower)
                {
                    var result = MessageBox.Show(
                        $"Мощности блока питания ({psu.Power}Вт) недостаточно для данной конфигурации.\n" +
                        $"Требуемая мощность с учетом запаса: {requiredPower}Вт.\n" +
                        "Вы уверены, что хотите продолжить?",
                        "Недостаточная мощность",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;
                }

                // Создание сборки
                var build = new Build
                {
                    Name = BuildNameTextBox.Text,
                    Description = BuildDescriptionTextBox.Text,
                    CreatedDate = DateTime.Now,
                    UserId = _currentUser.Id,
                    Image = _selectedImagePath,
                    IsCustom = true,
                    
                    // ID компонентов
                    CpuId = cpu.Id,
                    Cpu = cpu,
                    
                    GpuId = gpu.Id,
                    Gpu = gpu,
                    
                    MotherboardId = motherboard.Id,
                    Motherboard = motherboard,
                    
                    RamId = ((Ram)RamComboBox.SelectedItem).Id,
                    Ram = (Ram)RamComboBox.SelectedItem,
                    
                    StorageId = ((Storage)StorageComboBox.SelectedItem).Id,
                    Storage = (Storage)StorageComboBox.SelectedItem,
                    
                    PsuId = psu.Id,
                    Psu = psu,
                    
                    CaseId = ((Case)CaseComboBox.SelectedItem).Id,
                    Case = (Case)CaseComboBox.SelectedItem
                };

                // Расчет общей стоимости
                build.CalculateTotalPrice();

                // Сохранение сборки
                _context.Builds.Add(build);
                _context.SaveChanges();
                
                MessageBox.Show("Сборка успешно создана!", "Успех", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании сборки: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
