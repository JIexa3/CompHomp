using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp.Services
{
    public class SalesService
    {
        private readonly AppDbContext _context;

        public SalesService()
        {
            _context = new AppDbContext();
        }

        public List<Sale> GetSales()
        {
            return _context.Sales
                .OrderByDescending(s => s.Date)
                .ToList();
        }

        public List<Sale> GetSalesByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Sales
                .Where(s => s.Date >= startDate && s.Date <= endDate)
                .OrderByDescending(s => s.Date)
                .ToList();
        }

        public Sale GetSaleById(int id)
        {
            return _context.Sales.Find(id);
        }

        public void AddSale(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            sale.Date = DateTime.Now;
            _context.Sales.Add(sale);
            _context.SaveChanges();
        }

        public void UpdateSale(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            var existingSale = _context.Sales.Find(sale.Id);
            if (existingSale == null)
                throw new Exception($"Продажа с ID {sale.Id} не найдена");

            _context.Entry(existingSale).CurrentValues.SetValues(sale);
            _context.SaveChanges();
        }

        public void DeleteSale(int id)
        {
            var sale = _context.Sales.Find(id);
            if (sale != null)
            {
                _context.Sales.Remove(sale);
                _context.SaveChanges();
            }
        }

        public decimal GetTotalSalesAmount(DateTime startDate, DateTime endDate)
        {
            return _context.Sales
                .Where(s => s.Date >= startDate && s.Date <= endDate)
                .Sum(s => s.Amount);
        }

        public int GetTotalSalesCount(DateTime startDate, DateTime endDate)
        {
            return _context.Sales
                .Count(s => s.Date >= startDate && s.Date <= endDate);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
