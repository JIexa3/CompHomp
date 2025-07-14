using CompHomp.Data;
using CompHomp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompHomp.Services
{
    public class SaleStatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;

        public SaleStatisticsService(AppDbContext context)
        {
            _context = context;
        }

        public List<CompHomp.Models.SaleStatistics> GetDailySalesStatistics(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.PurchaseHistories.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(ph => ph.PurchaseDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ph => ph.PurchaseDate <= endDate.Value);

            return query
                .GroupBy(ph => ph.PurchaseDate.Date)
                .Select(g => new CompHomp.Models.SaleStatistics
                {
                    SaleDate = g.Key,
                    TotalRevenue = g.Sum(ph => ph.TotalAmount),
                    TotalSalesCount = g.Count(),
                    AverageOrderValue = g.Average(ph => ph.TotalAmount)
                })
                .OrderBy(s => s.SaleDate)
                .ToList();
        }

        public TotalSalesStatistics GetTotalSalesStatistics(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.PurchaseHistories.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(ph => ph.PurchaseDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ph => ph.PurchaseDate <= endDate.Value);

            var totalAmount = query.Sum(ph => ph.TotalAmount);
            var totalCount = query.Count();

            return new TotalSalesStatistics
            {
                TotalRevenue = totalAmount,
                TotalSalesCount = totalCount,
                AverageOrderValue = totalCount > 0 ? totalAmount / totalCount : 0
            };
        }

        public List<CompHomp.Models.SaleStatistics> GetComponentTypeSalesStatistics()
        {
            var statistics = _context.PurchaseHistories
                .SelectMany(ph => ph.Items)
                .GroupBy(item => item.Build.IsCustom ? "Пользовательская сборка" : "Стандартная сборка")
                .Select(g => new CompHomp.Models.SaleStatistics
                {
                    ComponentType = g.Key,
                    TotalRevenue = g.Sum(item => item.Build.TotalPrice * item.Quantity),
                    TotalSalesCount = g.Sum(item => item.Quantity),
                    AverageOrderValue = g.Average(item => item.Build.TotalPrice)
                })
                .ToList();

            return statistics;
        }

        public List<CompHomp.Models.SaleStatistics> GetBuildSalesStatistics(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.PurchaseHistories
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(ph => ph.PurchaseDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ph => ph.PurchaseDate <= endDate.Value);

            var buildStatistics = query
                .SelectMany(ph => ph.Items)
                .Where(item => item.BuildId != 0) 
                .GroupBy(item => item.Build.Name)
                .Select(g => new CompHomp.Models.SaleStatistics
                {
                    BuildName = g.Key,
                    TotalRevenue = g.Sum(item => item.Build.TotalPrice * item.Quantity),
                    TotalSalesCount = g.Sum(item => item.Quantity),
                    AverageOrderValue = g.Average(item => item.Build.TotalPrice)
                })
                .OrderByDescending(s => s.TotalRevenue)
                .ToList();

            return buildStatistics;
        }
    }
}
