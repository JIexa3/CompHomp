using CompHomp.Models;
using CompHomp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CompHomp
{
    /// <summary>
    /// Логика взаимодействия для PurchaseDetailsWindow.xaml
    /// </summary>
    public partial class PurchaseDetailsWindow : Window, INotifyPropertyChanged
    {
        private readonly AppDbContext _context;

        private int _orderId;
        private DateTime _orderDate;
        private string _status;
        private decimal _totalPrice;
        private List<OrderItem> _orderItems;

        public int OrderId 
        { 
            get => _orderId;
            set
            {
                _orderId = value;
                OnPropertyChanged();
            }
        }

        public DateTime OrderDate 
        { 
            get => _orderDate;
            set
            {
                _orderDate = value;
                OnPropertyChanged();
            }
        }

        public string Status 
        { 
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalPrice 
        { 
            get => _totalPrice;
            set
            {
                _totalPrice = value;
                OnPropertyChanged();
            }
        }

        public List<OrderItem> OrderItems 
        { 
            get => _orderItems;
            set
            {
                _orderItems = value;
                OnPropertyChanged();
            }
        }

        public PurchaseDetailsWindow(AppDbContext context, int orderId)
        {
            InitializeComponent();
            _context = context;
            DataContext = this;
            LoadOrderDetails(orderId);
        }

        private async void LoadOrderDetails(int orderId)
        {
            try
            {
                var purchase = await _context.PurchaseHistories
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Build)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (purchase == null)
                {
                    MessageBox.Show("Заказ не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                OrderId = purchase.Id;
                OrderDate = purchase.PurchaseDate;
                Status = purchase.OrderStatus;
                TotalPrice = purchase.TotalAmount;
                OrderItems = purchase.Items.Select(i => new OrderItem 
                { 
                    Build = i.Build,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList();

                OnPropertyChanged(nameof(OrderId));
                OnPropertyChanged(nameof(OrderDate));
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(OrderItems));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private string GetFormattedOrderStatus(string status)
        {
            return status switch
            {
                "В обработке" => "В обработке",
                "Подтвержден" => "Подтвержден",
                "Отменен" => "Отменен",
                "Завершен" => "Завершен",
                _ => status
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
