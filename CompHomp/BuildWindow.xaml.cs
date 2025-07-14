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
using Microsoft.EntityFrameworkCore;
using System.Windows.Media;

namespace CompHomp
{
    /// <summary>
    /// Логика взаимодействия для BuildWindow.xaml
    /// </summary>
    public partial class BuildWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly User _currentUser;
        private string? _selectedImagePath;
        private readonly ImageService _imageService;
        private Build _build;

        public BuildWindow(User currentUser)
        {
            InitializeComponent();
            _context = new AppDbContext();
            _currentUser = currentUser;
            _imageService = new ImageService();
            LoadComponents();
            SetupComponentHoverEvents();
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

                // Добавляем обработчики событий для пересчета цены и проверки совместимости
                CpuComboBox.SelectionChanged += UpdatePowerAndCompatibility;
                GpuComboBox.SelectionChanged += UpdatePowerAndCompatibility;
                MotherboardComboBox.SelectionChanged += UpdatePowerAndCompatibility;
                RamComboBox.SelectionChanged += UpdatePowerAndCompatibility;
                StorageComboBox.SelectionChanged += UpdatePowerAndCompatibility;
                PsuComboBox.SelectionChanged += UpdatePowerAndCompatibility;
                CaseComboBox.SelectionChanged += UpdatePowerAndCompatibility;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки компонентов: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePowerAndCompatibility(object sender, SelectionChangedEventArgs e)
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
                PowerProgressBar.Maximum = psu.Power;
                PowerProgressBar.Value = totalPower;
                PowerUsageText.Text = $"Энергопотребление: {totalPower}Вт из {psu.Power}Вт";
            }
            else
            {
                PowerProgressBar.Maximum = 1000; // Значение по умолчанию
                PowerProgressBar.Value = totalPower;
                PowerUsageText.Text = $"Энергопотребление: {totalPower}Вт";
            }

            // Обновляем общую стоимость
            TotalPriceText.Text = $"Общая стоимость: {totalPrice:C}";
        }

        private void Component_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePrices();
        }

        private void UpdatePrices()
        {
            try
            {
                decimal totalPrice = 0;

                if (CpuComboBox.SelectedItem is Cpu cpu)
                {
                    CpuPrice.Text = $"{cpu.Price:C}";
                    totalPrice += cpu.Price;
                }

                if (GpuComboBox.SelectedItem is Gpu gpu)
                {
                    GpuPrice.Text = $"{gpu.Price:C}";
                    totalPrice += gpu.Price;
                }

                if (MotherboardComboBox.SelectedItem is Motherboard mb)
                {
                    MotherboardPrice.Text = $"{mb.Price:C}";
                    totalPrice += mb.Price;
                }

                if (RamComboBox.SelectedItem is Ram ram)
                {
                    RamPrice.Text = $"{ram.Price:C}";
                    totalPrice += ram.Price;
                }

                if (StorageComboBox.SelectedItem is Storage storage)
                {
                    StoragePrice.Text = $"{storage.Price:C}";
                    totalPrice += storage.Price;
                }

                if (PsuComboBox.SelectedItem is Psu psu)
                {
                    PsuPrice.Text = $"{psu.Price:C}";
                    totalPrice += psu.Price;
                }

                if (CaseComboBox.SelectedItem is Case pcCase)
                {
                    CasePrice.Text = $"{pcCase.Price:C}";
                    totalPrice += pcCase.Price;
                }

                TotalPriceText.Text = $"{totalPrice:C}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении цен: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CreateBuild_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Введите название сборки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (CpuComboBox.SelectedItem == null ||
                    GpuComboBox.SelectedItem == null ||
                    MotherboardComboBox.SelectedItem == null ||
                    RamComboBox.SelectedItem == null ||
                    StorageComboBox.SelectedItem == null ||
                    PsuComboBox.SelectedItem == null ||
                    CaseComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите все компоненты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var cpu = (Cpu)CpuComboBox.SelectedItem;
                var gpu = (Gpu)GpuComboBox.SelectedItem;
                var motherboard = (Motherboard)MotherboardComboBox.SelectedItem;
                var ram = (Ram)RamComboBox.SelectedItem;
                var storage = (Storage)StorageComboBox.SelectedItem;
                var psu = (Psu)PsuComboBox.SelectedItem;
                var pcCase = (Case)CaseComboBox.SelectedItem;

                string imagePath = null;
                if (_selectedImagePath != null)
                {
                    imagePath = _imageService.SaveImage(_selectedImagePath);
                }

                _build = new Build
                {
                    Name = NameTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    UserId = _currentUser.Id,
                    CpuId = cpu.Id,
                    GpuId = gpu.Id,
                    MotherboardId = motherboard.Id,
                    RamId = ram.Id,
                    StorageId = storage.Id,
                    PsuId = psu.Id,
                    CaseId = pcCase.Id,
                    Status = BuildStatus.Pending,
                    CreatedDate = DateTime.Now,
                    Image = imagePath
                };

                var totalPrice = cpu.Price + gpu.Price + motherboard.Price + 
                               ram.Price + storage.Price + psu.Price + pcCase.Price;
                var markup = totalPrice * 0.05m;
                _build.BasePrice = totalPrice + markup;

                _context.Builds.Add(_build);
                await _context.SaveChangesAsync();

                LoadBuildDetails();

                MessageBox.Show("Сборка успешно создана и отправлена на модерацию!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                MessageBox.Show($"Ошибка при сохранении: {dbEx.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SetupComponentHoverEvents()
        {
            // CPU Hover
            CpuComboBox.MouseEnter += (s, e) => ShowComponentInfo(CpuComboBox.SelectedItem);

            // GPU Hover
            GpuComboBox.MouseEnter += (s, e) => ShowComponentInfo(GpuComboBox.SelectedItem);

            // Motherboard Hover
            MotherboardComboBox.MouseEnter += (s, e) => ShowComponentInfo(MotherboardComboBox.SelectedItem);

            // RAM Hover
            RamComboBox.MouseEnter += (s, e) => ShowComponentInfo(RamComboBox.SelectedItem);

            // Storage Hover
            StorageComboBox.MouseEnter += (s, e) => ShowComponentInfo(StorageComboBox.SelectedItem);

            // PSU Hover
            PsuComboBox.MouseEnter += (s, e) => ShowComponentInfo(PsuComboBox.SelectedItem);

            // Case Hover
            CaseComboBox.MouseEnter += (s, e) => ShowComponentInfo(CaseComboBox.SelectedItem);
        }

        private void ShowComponentInfo(object component)
        {
            if (ComponentDetailsPanel == null || ComponentNameText == null || ComponentDescriptionText == null)
                return;

            ComponentDetailsPanel.Children.Clear();

            switch (component)
            {
                case Cpu cpu when cpu != null:
                    ComponentNameText.Text = cpu.Name;
                    ComponentDescriptionText.Text = $"Процессор для игр и работы";
                    AddComponentDetail("Сокет", cpu.Socket);
                    AddComponentDetail("Ядра", cpu.Cores.ToString());
                    AddComponentDetail("Базовая частота", $"{cpu.BaseClockSpeed} ГГц");
                    AddComponentDetail("TDP", $"{cpu.TDP} Вт");
                    break;

                case Gpu gpu when gpu != null:
                    ComponentNameText.Text = gpu.Name;
                    ComponentDescriptionText.Text = $"Видеокарта для игр и графики";
                    AddComponentDetail("Память", $"{gpu.MemorySize} ГБ {gpu.MemoryType}");
                    AddComponentDetail("Частота ядра", $"{gpu.CoreClockSpeed} МГц");
                    AddComponentDetail("TDP", $"{gpu.TDP} Вт");
                    break;

                case Motherboard mb when mb != null:
                    ComponentNameText.Text = mb.Name;
                    ComponentDescriptionText.Text = $"Материнская плата для сборки ПК";
                    AddComponentDetail("Сокет", mb.Socket);
                    AddComponentDetail("Чипсет", mb.Chipset);
                    AddComponentDetail("Формат", mb.FormFactor);
                    break;

                case Ram ram when ram != null:
                    ComponentNameText.Text = ram.Name;
                    ComponentDescriptionText.Text = $"Оперативная память для системы";
                    AddComponentDetail("Объем", $"{ram.Capacity} ГБ");
                    AddComponentDetail("Тип", ram.Type);
                    AddComponentDetail("Частота", $"{ram.Speed} МГц");
                    break;

                case Storage storage when storage != null:
                    ComponentNameText.Text = storage.Name;
                    ComponentDescriptionText.Text = $"Накопитель для хранения данных";
                    AddComponentDetail("Тип", storage.Type);
                    AddComponentDetail("Объем", $"{storage.Capacity} ГБ");
                    AddComponentDetail("Скорость чтения", $"{storage.ReadSpeed} МБ/с");
                    break;

                case Psu psu when psu != null:
                    ComponentNameText.Text = psu.Name;
                    ComponentDescriptionText.Text = $"Блок питания для системы";
                    AddComponentDetail("Мощность", $"{psu.Power} Вт");
                    AddComponentDetail("Сертификат", psu.Efficiency);
                    AddComponentDetail("Модульный", psu.IsModular ? "Да" : "Нет");
                    break;

                case Case pcCase when pcCase != null:
                    ComponentNameText.Text = pcCase.Name;
                    ComponentDescriptionText.Text = $"Корпус для сборки ПК";
                    AddComponentDetail("Форм-фактор", pcCase.FormFactor);
                    AddComponentDetail("Цвет", pcCase.Color);
                    break;
            }
        }

        private void AddComponentDetail(string label, string value)
        {
            if (ComponentDetailsPanel == null)
                return;

            var detailPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var labelText = new TextBlock
            {
                Text = $"{label}: ",
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Width = 120
            };

            var valueText = new TextBlock
            {
                Text = value,
                Foreground = Brushes.LightGray
            };

            detailPanel.Children.Add(labelText);
            detailPanel.Children.Add(valueText);
            ComponentDetailsPanel.Children.Add(detailPanel);
        }

        private void HideComponentInfo(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Do nothing when mouse leaves the component
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
                    _selectedImagePath = openFileDialog.FileName;
                    BuildImage.Source = _imageService.LoadImage(_selectedImagePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выборе изображения: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private void LoadBuildDetails()
        {
            try
            {
                // Загрузка компонентов сборки
                var cpu = _context.Cpus.FirstOrDefault(c => c.Id == _build.CpuId);
                var gpu = _context.Gpus.FirstOrDefault(g => g.Id == _build.GpuId);
                var motherboard = _context.Motherboards.FirstOrDefault(m => m.Id == _build.MotherboardId);
                var ram = _context.Rams.FirstOrDefault(r => r.Id == _build.RamId);
                var storage = _context.Storages.FirstOrDefault(s => s.Id == _build.StorageId);
                var psu = _context.Psus.FirstOrDefault(p => p.Id == _build.PsuId);
                var computerCase = _context.Cases.FirstOrDefault(c => c.Id == _build.CaseId);

                // Проверка совместимости сокетов
                if (cpu != null && motherboard != null && cpu.Socket != motherboard.Socket)
                {
                    MessageBox.Show($"Внимание! В данной сборке процессор ({cpu.Socket}) не совместим с материнской платой ({motherboard.Socket})",
                        "Несовместимость компонентов", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // Расчет энергопотребления
                int totalPower = 0;
                if (cpu != null) totalPower += cpu.TDP;
                if (gpu != null) totalPower += gpu.TDP;

                // Обновление ProgressBar и текста энергопотребления
                if (psu != null)
                {
                    PowerProgressBar.Maximum = psu.Power;
                    PowerProgressBar.Value = totalPower;
                    
                    // Добавляем 20% запас мощности для надежности
                    int requiredPower = (int)(totalPower * 1.2);
                    
                    if (psu.Power < requiredPower)
                    {
                        MessageBox.Show(
                            $"Внимание! Мощности блока питания ({psu.Power}Вт) недостаточно для данной конфигурации.\n" +
                            $"Требуемая мощность с учетом запаса: {requiredPower}Вт.",
                            "Недостаточная мощность", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    
                    PowerUsageText.Text = $"Энергопотребление: {totalPower}Вт из {psu.Power}Вт";
                }
                else
                {
                    PowerProgressBar.Maximum = 1000;
                    PowerProgressBar.Value = totalPower;
                    PowerUsageText.Text = $"Энергопотребление: {totalPower}Вт";
                }

                // Отображение информации о компонентах
                if (cpu != null)
                {
                    CpuComboBox.SelectedItem = cpu;
                    CpuPrice.Text = cpu.Price.ToString("C");
                }

                if (gpu != null)
                {
                    GpuComboBox.SelectedItem = gpu;
                    GpuPrice.Text = gpu.Price.ToString("C");
                }

                if (motherboard != null)
                {
                    MotherboardComboBox.SelectedItem = motherboard;
                    MotherboardPrice.Text = motherboard.Price.ToString("C");
                }

                if (ram != null)
                {
                    RamComboBox.SelectedItem = ram;
                    RamPrice.Text = ram.Price.ToString("C");
                }

                if (storage != null)
                {
                    StorageComboBox.SelectedItem = storage;
                    StoragePrice.Text = storage.Price.ToString("C");
                }

                if (psu != null)
                {
                    PsuComboBox.SelectedItem = psu;
                    PsuPrice.Text = psu.Price.ToString("C");
                }

                if (computerCase != null)
                {
                    CaseComboBox.SelectedItem = computerCase;
                    CasePrice.Text = computerCase.Price.ToString("C");
                }

                // Отображение общей цены
                _build.CalculateTotalPrice();
                TotalPriceText.Text = $"Общая стоимость: {_build.TotalPrice:C}";

                // Отображение изображения сборки
                if (!string.IsNullOrEmpty(_build.Image) && File.Exists(_build.Image))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(_build.Image);
                    bitmap.EndInit();
                    BuildImage.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных сборки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
