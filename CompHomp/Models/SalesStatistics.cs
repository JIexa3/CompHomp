using System;

namespace CompHomp.Models
{
    public class SaleStatistics
    {
        public int Id { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalSalesCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string ComponentType { get; set; }
        public string BuildName { get; set; } 
    }

    public class TotalSalesStatistics
    {
        public decimal TotalRevenue { get; set; }
        public int TotalSalesCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
