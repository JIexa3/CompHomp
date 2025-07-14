using System;
using System.Windows;
using System.Windows.Media.Imaging;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp
{
    public partial class BuildDetailsWindow : Window
    {
        private readonly Build _build;

        public BuildDetailsWindow(Build build)
        {
            InitializeComponent();
            _build = build ?? throw new ArgumentNullException(nameof(build));
            
            // Пересчитываем цену перед отображением
            _build.CalculateTotalPrice();
            
            LoadBuildDetails();
        }

        private void LoadBuildDetails()
        {
            try
            {
                // Основная информация
                BuildNameText.Text = _build.Name;
                DescriptionText.Text = _build.Description ?? "Описание отсутствует";
                AuthorText.Text = _build.User?.Login ?? "Неизвестно";
                CreationDateText.Text = _build.CreatedDate.ToString("dd.MM.yyyy HH:mm");
                TotalPriceText.Text = $"{_build.TotalPrice:C} (включая наценку {_build.BasePrice:C})";

                // Загрузка изображения
                if (!string.IsNullOrEmpty(_build.Image))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(_build.Image, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        BuildImage.Source = bitmap;
                    }
                    catch (Exception)
                    {
                        BuildImage.Source = new BitmapImage(new Uri("/CompHomp;component/res/computer.png", UriKind.Relative));
                    }
                }
                else
                {
                    BuildImage.Source = new BitmapImage(new Uri("/CompHomp;component/res/computer.png", UriKind.Relative));
                }

                // Компоненты
                if (_build.Cpu != null)
                {
                    CpuText.Text = $"{_build.Cpu.Name}\nСокет: {_build.Cpu.Socket}\nЯдра: {_build.Cpu.Cores}\nБазовая частота: {_build.Cpu.BaseClockSpeed} ГГц\nTDP: {_build.Cpu.TDP}Вт\nЦена: {_build.Cpu.Price:C}";
                }

                if (_build.Gpu != null)
                {
                    GpuText.Text = $"{_build.Gpu.Name}\nПамять: {_build.Gpu.MemorySize}ГБ {_build.Gpu.MemoryType}\nЧастота ядра: {_build.Gpu.CoreClockSpeed} МГц\nTDP: {_build.Gpu.TDP}Вт\nЦена: {_build.Gpu.Price:C}";
                }

                if (_build.Ram != null)
                {
                    RamText.Text = $"{_build.Ram.Name}\n{_build.Ram.Capacity}ГБ {_build.Ram.Type}\nЧастота: {_build.Ram.Speed} МГц\nМодулей: {_build.Ram.ModulesCount}\nЦена: {_build.Ram.Price:C}";
                }

                if (_build.Motherboard != null)
                {
                    MotherboardText.Text = $"{_build.Motherboard.Name}\nСокет: {_build.Motherboard.Socket}\nЧипсет: {_build.Motherboard.Chipset}\nФормат: {_build.Motherboard.FormFactor}\nЦена: {_build.Motherboard.Price:C}";
                }

                if (_build.Storage != null)
                {
                    StorageText.Text = $"{_build.Storage.Name}\nТип: {_build.Storage.Type}\nОбъём: {_build.Storage.Capacity}ГБ\nФормат: {_build.Storage.FormFactor}\nСкорость чтения: {_build.Storage.ReadSpeed} МБ/с\nЦена: {_build.Storage.Price:C}";
                }

                if (_build.Psu != null)
                {
                    PsuText.Text = $"{_build.Psu.Name}\nМощность: {_build.Psu.Power}Вт\nСертификат: {_build.Psu.Efficiency}\nМодульный: {(_build.Psu.IsModular ? "Да" : "Нет")}\nЦена: {_build.Psu.Price:C}";
                }

                if (_build.Case != null)
                {
                    CaseText.Text = $"{_build.Case.Name}\nЦена: {_build.Case.Price:C}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
