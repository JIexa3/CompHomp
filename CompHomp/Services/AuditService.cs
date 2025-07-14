using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.EntityFrameworkCore;
using CompHomp.Data;
using CompHomp.Models;

namespace CompHomp.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;
        private int _retentionDays;

        public AuditService(AppDbContext context)
        {
            _context = context;
            var settings = context.SystemSettings.FirstOrDefault();
            _retentionDays = settings?.AuditLogRetentionDays ?? 30;
        }

        public AuditService() : this(new AppDbContext())
        {
        }

        public List<AuditLog> GetAuditLogs(int page = 1, int pageSize = 50)
        {
            return _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetAuditLogCount()
        {
            return _context.AuditLogs.Count();
        }

        public void LogEvent(int userId, string eventType, string description)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId == 0 ? null : userId,
                    Username = userId == 0 ? "System" : _context.Users.FirstOrDefault(u => u.Id == userId)?.Login ?? "Unknown",
                    EventType = eventType,
                    Description = description,
                    Timestamp = DateTime.Now
                };

                _context.AuditLogs.Add(auditLog);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LogEvent: {ex.Message}");
                throw;
            }
        }

        public void UpdateRetentionPeriod(int days, User currentUser)
        {
            if (days <= 0)
                throw new ArgumentException("Период хранения должен быть положительным числом");

            if (days != _retentionDays)
            {
                var oldRetentionDays = _retentionDays;
                _retentionDays = days;

                LogEvent(currentUser.Id, "Изменение настроек аудита", 
                    $"Изменен период хранения логов: {oldRetentionDays} -> {days} дней");

                ClearOldLogs(currentUser);
            }
        }

        private void ClearOldLogs(User currentUser)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-_retentionDays);
                var totalDeleted = 0;

                // Удаляем старые записи порциями для экономии памяти
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var auditLogsCount = _context.AuditLogs
                            .Where(l => l.Timestamp < cutoffDate)
                            .ExecuteDelete();

                        var authLogsCount = _context.UserAuthLogs
                            .Where(l => l.Timestamp < cutoffDate)
                            .ExecuteDelete();

                        var buildLogsCount = _context.LogEntries
                            .Where(l => l.Timestamp < cutoffDate)
                            .ExecuteDelete();

                        totalDeleted = auditLogsCount + authLogsCount + buildLogsCount;

                        if (totalDeleted > 0)
                        {
                            LogEvent(currentUser.Id, "Очистка логов", 
                                $"Удалены устаревшие записи (старше {_retentionDays} дней): {totalDeleted}");
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ClearOldLogs: {ex.Message}");
                throw;
            }
        }

        public void ExportAuditLogsToCSV(string filePath)
        {
            try
            {
                var logs = _context.AuditLogs
                    .OrderByDescending(l => l.Timestamp)
                    .ToList();

                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Дата,Пользователь,Событие,Описание");
                    foreach (var log in logs)
                    {
                        writer.WriteLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.Username},{log.EventType},{log.Description}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ExportAuditLogsToCSV: {ex.Message}");
                throw;
            }
        }
    }
}
