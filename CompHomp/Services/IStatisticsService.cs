using System;
using System.Collections.Generic;
using CompHomp.Models;

namespace CompHomp.Services
{
    public interface IStatisticsService
    {
        List<SaleStatistics> GetDailySalesStatistics(DateTime? startDate, DateTime? endDate);
        TotalSalesStatistics GetTotalSalesStatistics(DateTime? startDate, DateTime? endDate);
        List<SaleStatistics> GetComponentTypeSalesStatistics();
        
        // Единственный метод для получения статистики по сборкам
        List<SaleStatistics> GetBuildSalesStatistics(DateTime? startDate, DateTime? endDate);
    }
}
