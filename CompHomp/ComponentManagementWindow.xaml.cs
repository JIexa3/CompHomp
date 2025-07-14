using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using CompHomp.Models;
using CompHomp.Services;
using CompHomp.Data;
using System.Windows.Input;

namespace CompHomp
{
    public partial class ComponentManagementWindow : Window
    {
        private static ComponentManagementWindow _instance;
        private readonly CpuService _cpuService;
        private readonly MotherboardService _motherboardService;
        private readonly GpuService _gpuService;
        private readonly RamService _ramService;
        private readonly StorageService _storageService;
        private readonly PsuService _psuService;
        private readonly CaseService _caseService;
        private readonly AppDbContext _context;

        private ObservableCollection<Cpu> _cpuItems;
        private ObservableCollection<Gpu> _gpuItems;
        private ObservableCollection<Motherboard> _motherboardItems;
        private ObservableCollection<Ram> _ramItems;
        private ObservableCollection<Storage> _storageItems;
        private ObservableCollection<Psu> _psuItems;
        private ObservableCollection<Case> _caseItems;

        private static readonly object _lockObject = new object();
        private Window _currentAddEditWindow = null;
        private bool _isAddEditWindowOpen = false;

        public static ComponentManagementWindow GetInstance()
        {
            lock (_lockObject)
            {
                if (_instance == null || !_instance.IsLoaded)
                {
                    _instance = new ComponentManagementWindow();
                }
                return _instance;
            }
        }

        private ComponentManagementWindow()
        {
            InitializeComponent();
            
            _context = new AppDbContext();
            _cpuService = new CpuService(_context);
            _motherboardService = new MotherboardService(_context);
            _gpuService = new GpuService(_context);
            _ramService = new RamService(_context);
            _storageService = new StorageService(_context);
            _psuService = new PsuService(_context);
            _caseService = new CaseService(_context);

            ComponentTypeList.SelectedIndex = 0;
            ComponentTypeList.SelectionChanged += ComponentType_Selected;
            AddButton.Click += AddButton_Click;
            EditButton.Click += EditButton_Click;
            DeleteButton.Click += DeleteButton_Click;
            ComponentsGrid.SelectionChanged += ComponentsGrid_SelectionChanged;

            LoadComponents();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            lock (_lockObject)
            {
                if (_instance == this)
                {
                    _instance = null;
                }
            }
            _context.Dispose();
        }

        private void ComponentType_Selected(object sender, SelectionChangedEventArgs e)
        {
            LoadComponents();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAddEditWindowOpen)
            {
                return;
            }

            try 
            {
                switch (ComponentTypeList.SelectedIndex)
                {
                    case 0: // CPU
                        ShowAddEditComponentWindow<Cpu>("Добавление процессора");
                        break;
                    case 1: // GPU
                        ShowAddEditComponentWindow<Gpu>("Добавление видеокарты");
                        break;
                    case 2: // Motherboard
                        ShowAddEditComponentWindow<Motherboard>("Добавление материнской платы");
                        break;
                    case 3: // RAM
                        ShowAddEditComponentWindow<Ram>("Добавление оперативной памяти");
                        break;
                    case 4: // Storage
                        ShowAddEditComponentWindow<Storage>("Добавление накопителя");
                        break;
                    case 5: // PSU
                        ShowAddEditComponentWindow<Psu>("Добавление блока питания");
                        break;
                    case 6: // Case
                        ShowAddEditComponentWindow<Case>("Добавление корпуса");
                        break;
                }
                
                LoadComponents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAddEditWindowOpen)
            {
                return;
            }

            try 
            {
                var selectedItem = ComponentsGrid.SelectedItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Выберите компонент для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                switch (ComponentTypeList.SelectedIndex)
                {
                    case 0: // CPU
                        ShowAddEditComponentWindow<Cpu>("Редактирование процессора", selectedItem as Cpu);
                        break;
                    case 1: // GPU
                        ShowAddEditComponentWindow<Gpu>("Редактирование видеокарты", selectedItem as Gpu);
                        break;
                    case 2: // Motherboard
                        ShowAddEditComponentWindow<Motherboard>("Редактирование материнской платы", selectedItem as Motherboard);
                        break;
                    case 3: // RAM
                        ShowAddEditComponentWindow<Ram>("Редактирование оперативной памяти", selectedItem as Ram);
                        break;
                    case 4: // Storage
                        ShowAddEditComponentWindow<Storage>("Редактирование накопителя", selectedItem as Storage);
                        break;
                    case 5: // PSU
                        ShowAddEditComponentWindow<Psu>("Редактирование блока питания", selectedItem as Psu);
                        break;
                    case 6: // Case
                        ShowAddEditComponentWindow<Case>("Редактирование корпуса", selectedItem as Case);
                        break;
                }
                
                LoadComponents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                var selectedItem = ComponentsGrid.SelectedItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Выберите компонент для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранный компонент?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    switch (ComponentTypeList.SelectedIndex)
                    {
                        case 0: // CPU
                            var cpu = selectedItem as Cpu;
                            DeleteComponent<Cpu>(_cpuService, cpu);
                            break;
                        case 1: // GPU
                            var gpu = selectedItem as Gpu;
                            DeleteComponent<Gpu>(_gpuService, gpu);
                            break;
                        case 2: // Motherboard
                            var motherboard = selectedItem as Motherboard;
                            DeleteComponent<Motherboard>(_motherboardService, motherboard);
                            break;
                        case 3: // RAM
                            var ram = selectedItem as Ram;
                            DeleteComponent<Ram>(_ramService, ram);
                            break;
                        case 4: // Storage
                            var storage = selectedItem as Storage;
                            DeleteComponent<Storage>(_storageService, storage);
                            break;
                        case 5: // PSU
                            var psu = selectedItem as Psu;
                            DeleteComponent<Psu>(_psuService, psu);
                            break;
                        case 6: // Case
                            var computerCase = selectedItem as Case;
                            DeleteComponent<Case>(_caseService, computerCase);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении компонента: {ex.Message}\n\nПодробности: {ex.InnerException?.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteComponent<T>(dynamic service, T component) where T : class
        {
            try 
            {
                // Получаем ID компонента
                var componentId = (int)component.GetType().GetProperty("Id").GetValue(component);

                // Проверяем наличие связей перед удалением
                var buildsWithComponent = _context.Builds
                    .Where(b => 
                        (typeof(T) == typeof(Cpu) && b.CpuId == componentId) ||
                        (typeof(T) == typeof(Gpu) && b.GpuId == componentId) ||
                        (typeof(T) == typeof(Motherboard) && b.MotherboardId == componentId) ||
                        (typeof(T) == typeof(Ram) && b.RamId == componentId) ||
                        (typeof(T) == typeof(Storage) && b.StorageId == componentId) ||
                        (typeof(T) == typeof(Psu) && b.PsuId == componentId) ||
                        (typeof(T) == typeof(Case) && b.CaseId == componentId)
                    ).ToList();

                if (buildsWithComponent.Any())
                {
                    MessageBox.Show($"Невозможно удалить компонент. Он используется в {buildsWithComponent.Count} сборках.", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Удаляем компонент
                service.Delete(componentId);

                // Обновляем список
                LoadComponents();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при удалении компонента типа {typeof(T).Name}", ex);
            }
        }

        private void RefreshComponentList<T>(dynamic service)
        {
            switch (ComponentTypeList.SelectedIndex)
            {
                case 0: // CPU
                    _cpuItems = new ObservableCollection<Cpu>(service.GetAll());
                    ComponentsGrid.ItemsSource = _cpuItems;
                    break;
                case 1: // GPU
                    _gpuItems = new ObservableCollection<Gpu>(service.GetAll());
                    ComponentsGrid.ItemsSource = _gpuItems;
                    break;
                case 2: // Motherboard
                    _motherboardItems = new ObservableCollection<Motherboard>(service.GetAll());
                    ComponentsGrid.ItemsSource = _motherboardItems;
                    break;
                case 3: // RAM
                    _ramItems = new ObservableCollection<Ram>(service.GetAll());
                    ComponentsGrid.ItemsSource = _ramItems;
                    break;
                case 4: // Storage
                    _storageItems = new ObservableCollection<Storage>(service.GetAll());
                    ComponentsGrid.ItemsSource = _storageItems;
                    break;
                case 5: // PSU
                    _psuItems = new ObservableCollection<Psu>(service.GetAll());
                    ComponentsGrid.ItemsSource = _psuItems;
                    break;
                case 6: // Case
                    _caseItems = new ObservableCollection<Case>(service.GetAll());
                    ComponentsGrid.ItemsSource = _caseItems;
                    break;
            }
            SetupComponentColumns<T>();
        }

        private void SetupComponentColumns<T>()
        {
            ComponentsGrid.Columns.Clear();
            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != "Id")
                .ToList();

            foreach (var property in properties)
            {
                string header = GetRussianColumnName(property.Name);
                var binding = new Binding(property.Name);
                
                ComponentsGrid.Columns.Add(new DataGridTextColumn 
                { 
                    Header = header, 
                    Binding = binding 
                });
            }
        }

        private string GetRussianColumnName(string propertyName)
        {
            return propertyName switch
            {
                "Name" => "Название",
                "Price" => "Цена (₽)",
                "Socket" => "Сокет",
                "Cores" => "Количество ядер",
                "BaseClockSpeed" => "Базовая частота (ГГц)",
                "CoreClockSpeed" => "Частота ядра (МГц)",
                "TDP" => "TDP (Вт)",
                "Manufacturer" => "Производитель",
                "VideoMemory" => "Видеопамять (ГБ)",
                "FormFactor" => "Форм-фактор",
                "MemoryType" => "Тип памяти",
                "Frequency" => "Частота (МГц)",
                "MemorySize" => "Объем памяти (ГБ)",
                "Capacity" => "Емкость (ГБ)",
                "StorageType" => "Тип накопителя",
                "ReadSpeed" => "Скорость чтения (МБ/с)",
                "WriteSpeed" => "Скорость записи (МБ/с)",
                "Power" => "Мощность (Вт)",
                "Efficiency" => "Эффективность",
                "ModularPsu" => "Модульный",
                "Color" => "Цвет",
                "Size" => "Размер",
                _ => propertyName
            };
        }

        private string GetRussianPropertyName(string propertyName)
        {
            return GetRussianColumnName(propertyName);
        }

        private void ShowAddEditComponentWindow<T>(string title, T existingComponent = null) where T : class, new()
        {
            // Если окно уже открыто, просто активируем его
            if (_isAddEditWindowOpen)
            {
                return;
            }

            try 
            {
                // Закрываем существующее окно, если оно открыто
                if (_currentAddEditWindow != null)
                {
                    _currentAddEditWindow.Close();
                    _currentAddEditWindow = null;
                }

                var window = new Window
                {
                    Title = title,
                    Width = 500,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    Background = Brushes.Transparent,
                    Owner = this
                };

                // Устанавливаем флаг открытого окна
                _isAddEditWindowOpen = true;

                // Сохраняем ссылку на текущее окно
                _currentAddEditWindow = window;

                window.Closed += (s, e) => 
                {
                    _isAddEditWindowOpen = false;
                    if (_currentAddEditWindow == window)
                    {
                        _currentAddEditWindow = null;
                    }
                };

                // Создаем Border для красивого оформления
                var mainBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    CornerRadius = new CornerRadius(10),
                    Margin = new Thickness(10),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200))
                };

                var mainGrid = new Grid();
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Заголовок
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Контент
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Кнопки

                // Заголовок окна
                var headerGrid = new Grid 
                { 
                    Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                    Height = 40
                };
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var titleBlock = new TextBlock 
                { 
                    Text = title,
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(15, 0, 0, 0)
                };
                Grid.SetColumn(titleBlock, 0);
                headerGrid.Children.Add(titleBlock);

                // Кнопка закрытия
                var closeButton = new Button
                {
                    Content = "✕",
                    Width = 40,
                    Height = 40,
                    Background = Brushes.Transparent,
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontSize = 16
                };
                closeButton.Click += (s, e) => window.Close();
                Grid.SetColumn(closeButton, 2);
                headerGrid.Children.Add(closeButton);

                // Возможность перетаскивания окна
                headerGrid.MouseLeftButtonDown += (s, e) => 
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                        window.DragMove();
                };

                Grid.SetRow(headerGrid, 0);
                mainGrid.Children.Add(headerGrid);

                // Скроллируемый контент
                var scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Margin = new Thickness(15)
                };

                var contentGrid = new Grid { Margin = new Thickness(10) };
                var properties = typeof(T).GetProperties().Where(p => p.CanWrite && p.Name != "Id").ToList();
    
                for (int i = 0; i < properties.Count; i++)
                {
                    contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Метка
                    contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Поле ввода
                }

                var fields = new Dictionary<string, TextBox>();

                for (int i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    var label = GetRussianPropertyName(property.Name);

                    var textBlock = new TextBlock 
                    { 
                        Text = label, 
                        FontSize = 14,
                        Foreground = Brushes.Black,
                        Margin = new Thickness(0, 10, 0, 5) 
                    };
                    Grid.SetRow(textBlock, i * 2);

                    var textBox = new TextBox 
                    { 
                        Margin = new Thickness(0, 0, 0, 10),
                        Padding = new Thickness(10),
                        FontSize = 14,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                        BorderThickness = new Thickness(1),
                        Height = 40
                    };
                    Grid.SetRow(textBox, i * 2 + 1);

                    // Предварительное заполнение, если компонент существует
                    if (existingComponent != null)
                    {
                        var value = property.GetValue(existingComponent);
                        textBox.Text = value?.ToString() ?? "";
                    }

                    // Валидация для числовых полей
                    if (property.PropertyType == typeof(int) || 
                        property.PropertyType == typeof(double) || 
                        property.PropertyType == typeof(decimal))
                    {
                        textBox.PreviewTextInput += (s, e) => 
                        {
                            e.Handled = !char.IsDigit(e.Text[0]) && e.Text[0] != '.';
                        };
                    }

                    contentGrid.Children.Add(textBlock);
                    contentGrid.Children.Add(textBox);
                    fields[label] = textBox;
                }

                scrollViewer.Content = contentGrid;
                Grid.SetRow(scrollViewer, 1);
                mainGrid.Children.Add(scrollViewer);

                // Панель кнопок
                var buttonPanel = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal, 
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 15)
                };

                var saveButton = new Button 
                { 
                    Content = "Сохранить", 
                    Width = 150, 
                    Height = 40,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)), // Синий цвет
                    Foreground = Brushes.White,
                    FontSize = 14,
                    BorderThickness = new Thickness(0),
                    Style = (Style)Application.Current.Resources["RoundButtonStyle"]
                };

                var cancelButton = new Button 
                { 
                    Content = "Отмена", 
                    Width = 150, 
                    Height = 40,
                    Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)), // Красный цвет
                    Foreground = Brushes.White,
                    FontSize = 14,
                    BorderThickness = new Thickness(0),
                    Style = (Style)Application.Current.Resources["RoundButtonStyle"]
                };

                cancelButton.Click += (s, e) => window.Close();

                saveButton.Click += (s, args) =>
                {
                    try 
                    {
                        var component = existingComponent ?? new T();

                        for (int i = 0; i < properties.Count; i++)
                        {
                            var property = properties[i];
                            var label = GetRussianPropertyName(property.Name);
                            var value = fields[label].Text;

                            // Преобразование типов
                            object convertedValue = Convert.ChangeType(value, property.PropertyType);
                            property.SetValue(component, convertedValue);
                        }

                        // Сохранение компонента
                        dynamic service = GetServiceForType<T>();
                        if (existingComponent == null)
                            service.Add(component);
                        else
                            service.Update(component);

                        window.Close();
                        LoadComponents();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                buttonPanel.Children.Add(saveButton);
                buttonPanel.Children.Add(cancelButton);

                Grid.SetRow(buttonPanel, 2);
                mainGrid.Children.Add(buttonPanel);

                mainBorder.Child = mainGrid;
                window.Content = mainBorder;
                window.ShowDialog();
            }
            finally
            {
                _isAddEditWindowOpen = false;
            }
        }

        private dynamic GetServiceForType<T>()
        {
            if (typeof(T) == typeof(Cpu)) return _cpuService;
            if (typeof(T) == typeof(Gpu)) return _gpuService;
            if (typeof(T) == typeof(Motherboard)) return _motherboardService;
            if (typeof(T) == typeof(Ram)) return _ramService;
            if (typeof(T) == typeof(Storage)) return _storageService;
            if (typeof(T) == typeof(Psu)) return _psuService;
            if (typeof(T) == typeof(Case)) return _caseService;
            
            throw new ArgumentException("Неизвестный тип компонента");
        }

        private void ComponentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var isItemSelected = ComponentsGrid.SelectedItem != null;
            EditButton.IsEnabled = isItemSelected;
            DeleteButton.IsEnabled = isItemSelected;
        }

        private void LoadComponents()
        {
            // Очищаем существующие столбцы
            ComponentsGrid.Columns.Clear();

            switch (ComponentTypeList.SelectedIndex)
            {
                case 0: // CPU
                    _cpuItems = new ObservableCollection<Cpu>(_cpuService.GetAll());
                    ComponentsGrid.ItemsSource = _cpuItems;
                    SetupComponentColumns<Cpu>();
                    break;
                case 1: // GPU
                    _gpuItems = new ObservableCollection<Gpu>(_gpuService.GetAll());
                    ComponentsGrid.ItemsSource = _gpuItems;
                    SetupComponentColumns<Gpu>();
                    break;
                case 2: // Motherboard
                    _motherboardItems = new ObservableCollection<Motherboard>(_motherboardService.GetAll());
                    ComponentsGrid.ItemsSource = _motherboardItems;
                    SetupComponentColumns<Motherboard>();
                    break;
                case 3: // RAM
                    _ramItems = new ObservableCollection<Ram>(_ramService.GetAll());
                    ComponentsGrid.ItemsSource = _ramItems;
                    SetupComponentColumns<Ram>();
                    break;
                case 4: // Storage
                    _storageItems = new ObservableCollection<Storage>(_storageService.GetAll());
                    ComponentsGrid.ItemsSource = _storageItems;
                    SetupComponentColumns<Storage>();
                    break;
                case 5: // PSU
                    _psuItems = new ObservableCollection<Psu>(_psuService.GetAll());
                    ComponentsGrid.ItemsSource = _psuItems;
                    SetupComponentColumns<Psu>();
                    break;
                case 6: // Case
                    _caseItems = new ObservableCollection<Case>(_caseService.GetAll());
                    ComponentsGrid.ItemsSource = _caseItems;
                    SetupComponentColumns<Case>();
                    break;
            }
        }
    }
}
