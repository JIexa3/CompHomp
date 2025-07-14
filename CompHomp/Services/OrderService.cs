using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp.Services
{
    public class OrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public List<PurchaseHistory> GetUserOrders(int userId)
        {
            return _context.PurchaseHistories
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.PurchaseDate)
                .ToList();
        }

        public PurchaseHistory GetOrderById(int orderId)
        {
            return _context.PurchaseHistories
                .Include(o => o.Items)
                    .ThenInclude(i => i.Build)
                .FirstOrDefault(o => o.Id == orderId);
        }

        public void AddOrder(PurchaseHistory purchase)
        {
            _context.PurchaseHistories.Add(purchase);
            _context.SaveChanges();
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            var purchase = _context.PurchaseHistories.Find(orderId);
            if (purchase != null)
            {
                purchase.OrderStatus = status;
                _context.SaveChanges();
            }
        }

        public class SalesStatistics
        {
            public decimal TotalRevenue { get; set; }
            public int TotalSalesCount { get; set; }
            public List<DailySale> DailySales { get; set; }
            public List<SaleDetail> SalesDetails { get; set; }
        }

        public class DailySale
        {
            public DateTime Date { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public class SaleDetail
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public SalesStatistics GetSalesStatistics(DateTime startDate, DateTime endDate)
        {
            var salesQuery = _context.PurchaseHistories
                .Where(o => o.PurchaseDate >= startDate && o.PurchaseDate <= endDate);

            var dailySales = salesQuery
                .GroupBy(o => o.PurchaseDate.Date)
                .Select(g => new DailySale
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(d => d.Date)
                .ToList();

            var salesDetails = salesQuery
                .SelectMany(o => o.Items)
                .GroupBy(i => i.Build.Name)
                .Select(g => new SaleDetail
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(i => i.Quantity),
                    TotalAmount = g.Sum(i => i.Quantity * i.Build.BasePrice)
                })
                .ToList();

            return new SalesStatistics
            {
                TotalRevenue = salesQuery.Sum(o => o.TotalAmount),
                TotalSalesCount = salesQuery.Count(),
                DailySales = dailySales,
                SalesDetails = salesDetails
            };
        }
    }
}
