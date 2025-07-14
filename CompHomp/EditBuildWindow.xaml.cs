using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using CompHomp.Data;
using CompHomp.Models;
using CompHomp.Services;

namespace CompHomp
{
    public partial class EditBuildWindow : Window
    {
        private readonly BuildService _buildService;
        private Build _build;

        // ObservableCollection для ComboBox
        private ObservableCollection<Cpu> _cpus = new ObservableCollection<Cpu>();
        private ObservableCollection<Gpu> _gpus = new ObservableCollection<Gpu>();
        private ObservableCollection<Motherboard> _motherboards = new ObservableCollection<Motherboard>();
        private ObservableCollection<Ram> _rams = new ObservableCollection<Ram>();
        private ObservableCollection<Storage> _storages = new ObservableCollection<Storage>();
        private ObservableCollection<Psu> _psus = new ObservableCollection<Psu>();
        private ObservableCollection<Case> _cases = new ObservableCollection<Case>();

        public EditBuildWindow(BuildService buildService, Build build = null)
        {
            InitializeComponent();
            _buildService = buildService;
            _build = build;

            // Очищаем Items перед установкой ItemsSource
            CpuComboBox.Items.Clear();
            GpuComboBox.Items.Clear();
            MotherboardComboBox.Items.Clear();
            RamComboBox.Items.Clear();
            StorageComboBox.Items.Clear();
            PsuComboBox.Items.Clear();
            CaseComboBox.Items.Clear();

            // Инициализируем ObservableCollection для ComboBox
            _cpus = new ObservableCollection<Cpu>();
            _gpus = new ObservableCollection<Gpu>();
            _motherboards = new ObservableCollection<Motherboard>();
            _rams = new ObservableCollection<Ram>();
            _storages = new ObservableCollection<Storage>();
            _psus = new ObservableCollection<Psu>();
            _cases = new ObservableCollection<Case>();

            // Устанавливаем ItemsSource
            CpuComboBox.ItemsSource = _cpus;
            GpuComboBox.ItemsSource = _gpus;
            MotherboardComboBox.ItemsSource = _motherboards;
            RamComboBox.ItemsSource = _rams;
            StorageComboBox.ItemsSource = _storages;
            PsuComboBox.ItemsSource = _psus;
            CaseComboBox.ItemsSource = _cases;

            // Устанавливаем DataContext для привязки Status
            if (_build != null)
            {
                this.DataContext = _build;
            }

            Loaded += async (s, e) => 
            {
                try 
                {
                    using (var context = new AppDbContext())
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            // Очищаем ObservableCollection
                            _cpus.Clear();
                            _gpus.Clear();
                            _motherboards.Clear();
                            _rams.Clear();
                            _storages.Clear();
                            _psus.Clear();
                            _cases.Clear();

                            // Загружаем компоненты
                            foreach (var cpu in context.Cpus) _cpus.Add(cpu);
                            foreach (var gpu in context.Gpus) _gpus.Add(gpu);
                            foreach (var motherboard in context.Motherboards) _motherboards.Add(motherboard);
                            foreach (var ram in context.Rams) _rams.Add(ram);
                            foreach (var storage in context.Storages) _storages.Add(storage);
                            foreach (var psu in context.Psus) _psus.Add(psu);
                            foreach (var computerCase in context.Cases) _cases.Add(computerCase);

                            // Добавляем обработчики событий для пересчета цены
                            CpuComboBox.SelectionChanged += RecalculatePrice;
                            GpuComboBox.SelectionChanged += RecalculatePrice;
                            MotherboardComboBox.SelectionChanged += RecalculatePrice;
                            RamComboBox.SelectionChanged += RecalculatePrice;
                            StorageComboBox.SelectionChanged += RecalculatePrice;
                            PsuComboBox.SelectionChanged += RecalculatePrice;
                            CaseComboBox.SelectionChanged += RecalculatePrice;
                        });

                        // Загрузка данных существующей сборки
                        if (_build != null)
                        {
                            await Dispatcher.InvokeAsync(() =>
                            {
                                txtName.Text = _build.Name;
                                txtDescription.Text = _build.Description;
                                txtBasePrice.Text = _build.BasePrice.ToString("F2");

                                // Установка выбранных компонентов
                                if (_build.CpuId.HasValue)
                                    CpuComboBox.SelectedItem = context.Cpus.Find(_build.CpuId);
                                if (_build.GpuId.HasValue)
                                    GpuComboBox.SelectedItem = context.Gpus.Find(_build.GpuId);
                                if (_build.MotherboardId.HasValue)
                                    MotherboardComboBox.SelectedItem = context.Motherboards.Find(_build.MotherboardId);
                                if (_build.RamId.HasValue)
                                    RamComboBox.SelectedItem = context.Rams.Find(_build.RamId);
                                if (_build.StorageId.HasValue)
                                    StorageComboBox.SelectedItem = context.Storages.Find(_build.StorageId);
                                if (_build.PsuId.HasValue)
                                    PsuComboBox.SelectedItem = context.Psus.Find(_build.PsuId);
                                if (_build.CaseId.HasValue)
                                    CaseComboBox.SelectedItem = context.Cases.Find(_build.CaseId);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке компонентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            SaveButton.Click += SaveBuild;
            CancelButton.Click += (s, e) => Close();
        }

        private async void LoadComponentsAsync(object sender, RoutedEventArgs e)
        {
            try 
            {
                using (var context = new AppDbContext())
                {
                    // Загрузка компонентов
                    await Dispatcher.InvokeAsync(() =>
                    {
                        // Очищаем ObservableCollection
                        _cpus.Clear();
                        _gpus.Clear();
                        _motherboards.Clear();
                        _rams.Clear();
                        _storages.Clear();
                        _psus.Clear();
                        _cases.Clear();

                        // Загружаем компоненты
                        foreach (var cpu in context.Cpus) _cpus.Add(cpu);
                        foreach (var gpu in context.Gpus) _gpus.Add(gpu);
                        foreach (var motherboard in context.Motherboards) _motherboards.Add(motherboard);
                        foreach (var ram in context.Rams) _rams.Add(ram);
                        foreach (var storage in context.Storages) _storages.Add(storage);
                        foreach (var psu in context.Psus) _psus.Add(psu);
                        foreach (var computerCase in context.Cases) _cases.Add(computerCase);

                        // Добавляем обработчики событий для пересчета цены
                        CpuComboBox.SelectionChanged += RecalculatePrice;
                        GpuComboBox.SelectionChanged += RecalculatePrice;
                        MotherboardComboBox.SelectionChanged += RecalculatePrice;
                        RamComboBox.SelectionChanged += RecalculatePrice;
                        StorageComboBox.SelectionChanged += RecalculatePrice;
                        PsuComboBox.SelectionChanged += RecalculatePrice;
                        CaseComboBox.SelectionChanged += RecalculatePrice;
                    });

                    // Загрузка данных существующей сборки
                    if (_build != null)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            txtName.Text = _build.Name;
                            txtDescription.Text = _build.Description;
                            txtBasePrice.Text = _build.BasePrice.ToString("F2");

                            // Установка выбранных компонентов
                            if (_build.CpuId.HasValue)
                                CpuComboBox.SelectedItem = context.Cpus.Find(_build.CpuId);
                            if (_build.GpuId.HasValue)
                                GpuComboBox.SelectedItem = context.Gpus.Find(_build.GpuId);
                            if (_build.MotherboardId.HasValue)
                                MotherboardComboBox.SelectedItem = context.Motherboards.Find(_build.MotherboardId);
                            if (_build.RamId.HasValue)
                                RamComboBox.SelectedItem = context.Rams.Find(_build.RamId);
                            if (_build.StorageId.HasValue)
                                StorageComboBox.SelectedItem = context.Storages.Find(_build.StorageId);
                            if (_build.PsuId.HasValue)
                                PsuComboBox.SelectedItem = context.Psus.Find(_build.PsuId);
                            if (_build.CaseId.HasValue)
                                CaseComboBox.SelectedItem = context.Cases.Find(_build.CaseId);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке компонентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RecalculatePrice(object sender, SelectionChangedEventArgs e)
        {
            try 
            {
                decimal totalPrice = 0;

                if (CpuComboBox.SelectedItem is Cpu cpu)
                    totalPrice += cpu.Price;
                if (GpuComboBox.SelectedItem is Gpu gpu)
                    totalPrice += gpu.Price;
                if (MotherboardComboBox.SelectedItem is Motherboard motherboard)
                    totalPrice += motherboard.Price;
                if (RamComboBox.SelectedItem is Ram ram)
                    totalPrice += ram.Price;
                if (StorageComboBox.SelectedItem is Storage storage)
                    totalPrice += storage.Price;
                if (PsuComboBox.SelectedItem is Psu psu)
                    totalPrice += psu.Price;
                if (CaseComboBox.SelectedItem is Case computerCase)
                    totalPrice += computerCase.Price;

                txtBasePrice.Text = totalPrice.ToString("F2");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете цены: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveBuild(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try 
            {
                // Сохраняем старые значения для сравнения
                var oldCpuId = _build.CpuId;
                var oldGpuId = _build.GpuId;
                var oldMotherboardId = _build.MotherboardId;
                var oldRamId = _build.RamId;
                var oldStorageId = _build.StorageId;
                var oldPsuId = _build.PsuId;
                var oldCaseId = _build.CaseId;

                // Обновляем данные сборки
                _build.Name = txtName.Text;
                _build.Description = txtDescription.Text;
                _build.BasePrice = decimal.Parse(txtBasePrice.Text);

                // Обновляем компоненты
                _build.CpuId = (CpuComboBox.SelectedItem as Cpu)?.Id;
                _build.GpuId = (GpuComboBox.SelectedItem as Gpu)?.Id;
                _build.MotherboardId = (MotherboardComboBox.SelectedItem as Motherboard)?.Id;
                _build.RamId = (RamComboBox.SelectedItem as Ram)?.Id;
                _build.StorageId = (StorageComboBox.SelectedItem as Storage)?.Id;
                _build.PsuId = (PsuComboBox.SelectedItem as Psu)?.Id;
                _build.CaseId = (CaseComboBox.SelectedItem as Case)?.Id;

                // Создаем словарь изменений
                var changes = new Dictionary<string, (string OldComponent, string NewComponent)>();

                using (var context = new AppDbContext())
                {
                    // Проверяем изменения процессора
                    if (oldCpuId != _build.CpuId)
                    {
                        var oldCpu = oldCpuId.HasValue ? context.Cpus.Find(oldCpuId)?.Name ?? "Не выбран" : "Не выбран";
                        var newCpu = _build.CpuId.HasValue ? context.Cpus.Find(_build.CpuId)?.Name ?? "Не выбран" : "Не выбран";
                        changes["Процессор"] = (oldCpu, newCpu);
                    }

                    // Проверяем изменения видеокарты
                    if (oldGpuId != _build.GpuId)
                    {
                        var oldGpu = oldGpuId.HasValue ? context.Gpus.Find(oldGpuId)?.Name ?? "Не выбран" : "Не выбран";
                        var newGpu = _build.GpuId.HasValue ? context.Gpus.Find(_build.GpuId)?.Name ?? "Не выбран" : "Не выбран";
                        changes["Видеокарта"] = (oldGpu, newGpu);
                    }

                    // Проверяем изменения материнской платы
                    if (oldMotherboardId != _build.MotherboardId)
                    {
                        var oldMb = oldMotherboardId.HasValue ? context.Motherboards.Find(oldMotherboardId)?.Name ?? "Не выбран" : "Не выбран";
                        var newMb = _build.MotherboardId.HasValue ? context.Motherboards.Find(_build.MotherboardId)?.Name ?? "Не выбран" : "Не выбран";
                        changes["Материнская плата"] = (oldMb, newMb);
                    }

                    // Проверяем изменения оперативной памяти
                    if (oldRamId != _build.RamId)
                    {
                        var oldRam = oldRamId.HasValue ? context.Rams.Find(oldRamId)?.Name ?? "Не выбран" : "Не выбран";
                        var newRam = _build.RamId.HasValue ? context.Rams.Find(_build.RamId)?.Name ?? "Не выбран" : "Не выбран";
                        changes["Оперативная память"] = (oldRam, newRam);
                    }

                    // Проверяем изменения накопителя
                    if (oldStorageId != _build.StorageId)
                    {
                        var oldStorage = oldStorageId.HasValue ? context.Storages.Find(oldStorageId)?.Name ?? "Не выбран" : "Не выбран";
                        var newStorage = _build.StorageId.HasValue ? context.Storages.Find(_build.StorageId)?.Name ?? "Не выбран" : "Не выбран";
                        changes["Накопитель"] = (oldStorage, newStorage);
                    }

                    // Проверяем изменения блока питания
                    if (oldPsuId != _build.PsuId)
                    {
                        var oldPsu = oldPsuId.HasValue ? context.Psus.Find(oldPsuId)?.Name ?? "Не выбран" : "Не выбран";
                        var newPsu = _build.PsuId.HasValue ? context.Psus.Find(_build.PsuId)?.Name ?? "Не выбран" : "Не выбран";
                        changes["Блок питания"] = (oldPsu, newPsu);
                    }

                    // Проверяем изменения корпуса
                    if (oldCaseId != _build.CaseId)
                    {
                        var oldCase = oldCaseId.HasValue ? context.Cases.Find(oldCaseId)?.Name ?? "Не выбран" : "Не выбран";
                        var newCase = _build.CaseId.HasValue ? context.Cases.Find(_build.CaseId)?.Name ?? "Не выбран" : "Не выбран";
                        changes["Корпус"] = (oldCase, newCase);
                    }
                }

                // Сохраняем или обновляем сборку
                if (_build.Id == 0)
                    await _buildService.CreateBuildAsync(_build);
                else
                    await _buildService.UpdateBuildAsync(_build, changes);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении сборки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Название не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}
