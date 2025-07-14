using System;
using System.Collections.Generic;
using CompHomp.Models;

namespace CompHomp.Services
{
    public interface IAuditService
    {
        List<AuditLog> GetAuditLogs(int page = 1, int pageSize = 50);
        void ExportAuditLogsToCSV(string filePath);
        int GetAuditLogCount();
        void UpdateRetentionPeriod(int days, User currentUser);
        void LogEvent(int userId, string eventType, string description);
    }
}
