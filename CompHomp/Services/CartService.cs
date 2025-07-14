using CompHomp.Data;
using CompHomp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompHomp.Services
{
    public class CartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context) => _context = context;

        public List<CartItem> GetCartItems(int userId) => 
            _context.CartItems
                .Include(c => c.Build)
                .Where(c => c.UserId == userId)
                .ToList();

        public async Task AddToCart(int userId, int buildId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.BuildId == buildId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new CartItem
                {
                    UserId = userId,
                    BuildId = buildId,
                    Quantity = 1,
                    DateAdded = System.DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateQuantity(int cartItemId, int quantity)
        {
            if (quantity <= 0) 
            {
                await RemoveFromCart(cartItemId);
                return;
            }

            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCart(int userId)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public async Task<(bool success, string message)> ProcessPurchase(int userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var items = await _context.CartItems
                        .Include(c => c.Build)
                        .Where(c => c.UserId == userId)
                        .ToListAsync();

                    if (!items.Any())
                        return (false, "Корзина пуста");

                    // Проверяем существование пользователя и получаем его данные
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    if (user == null)
                    {
                        return (false, $"Пользователь с ID {userId} не найден в базе данных");
                    }

                    // Проверяем, что все сборки существуют
                    var buildIds = items.Select(i => i.BuildId).ToList();
                    var existingBuilds = await _context.Builds
                        .Where(b => buildIds.Contains(b.Id))
                        .Select(b => new { b.Id, b.Name })
                        .ToListAsync();

                    if (existingBuilds.Count != buildIds.Count)
                    {
                        var missingBuildIds = buildIds.Except(existingBuilds.Select(b => b.Id));
                        return (false, $"Некоторые сборки не найдены: {string.Join(", ", missingBuildIds)}");
                    }

                    decimal totalAmount = items.Sum(item => item.Build.TotalPrice * item.Quantity);

                    var purchaseHistory = new PurchaseHistory
                    {
                        UserId = userId,
                        PurchaseDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        OrderStatus = "Оплачен"
                    };

                    _context.PurchaseHistories.Add(purchaseHistory);
                    await _context.SaveChangesAsync();

                    var purchaseItems = items.Select(item => new PurchaseHistoryItem
                    {
                        PurchaseHistoryId = purchaseHistory.Id,
                        BuildId = item.BuildId,
                        Quantity = item.Quantity,
                        Price = item.Build.TotalPrice
                    }).ToList();

                    _context.PurchaseHistoryItems.AddRange(purchaseItems);
                    _context.CartItems.RemoveRange(items);
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return (true, "Покупка успешно оформлена");
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();
                    var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                    return (false, $"Ошибка при сохранении данных: {innerMessage}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Ошибка: {ex.Message}");
                }
            });
        }
    }
}
