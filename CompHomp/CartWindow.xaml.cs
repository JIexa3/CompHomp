using CompHomp.Models;
using CompHomp.Services;
using CompHomp.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CompHomp
{
    /// <summary>
    /// Логика взаимодействия для CartWindow.xaml
    /// </summary>
    public partial class CartWindow : Window
    {
        private readonly CartService _cartService;
        private readonly ObservableCollection<CartItem> _cartItems;
        private readonly int _userId;

        public CartWindow(User currentUser)
        {
            InitializeComponent();
            _userId = currentUser.Id;
            _cartService = new CartService(new AppDbContext());
            _cartItems = new ObservableCollection<CartItem>();
            CartItemsListView.ItemsSource = _cartItems;
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            try
            {
                var items = _cartService.GetCartItems(_userId);
                _cartItems.Clear();
                foreach (var item in items)
                {
                    _cartItems.Add(item);
                }
                UpdateTotalPrice();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotalPrice()
        {
            decimal total = _cartItems.Sum(item => item.Build.TotalPrice * item.Quantity);
            TotalPriceText.Text = $"{total:N0} ₽";
        }

        private async void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is CartItem item)
            {
                try
                {
                    await _cartService.UpdateQuantity(item.Id, item.Quantity + 1);
                    LoadCartItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка изменения количества: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is CartItem item)
            {
                try
                {
                    if (item.Quantity > 1)
                    {
                        await _cartService.UpdateQuantity(item.Id, item.Quantity - 1);
                        LoadCartItems();
                    }
                    else
                    {
                        await RemoveItem(item.Id);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка изменения количества: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is CartItem item)
            {
                await RemoveItem(item.Id);
            }
        }

        private async Task RemoveItem(int itemId)
        {
            try
            {
                await _cartService.RemoveFromCart(itemId);
                LoadCartItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Purchase_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите совершить покупку?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    (bool success, string message) = await _cartService.ProcessPurchase(_userId);
                    if (success)
                    {
                        MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadCartItems();
                    }
                    else
                    {
                        MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при оформлении покупки: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}