using System;
using System.Text;
using CompHomp.Data;
using CompHomp.Models;

namespace CompHomp.Services
{
    public class SystemSettingsService
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;
        private readonly User _currentUser;

        public SystemSettingsService(AppDbContext context, IAuditService auditService, User currentUser)
        {
            _context = context;
            _auditService = auditService;
            _currentUser = currentUser;
        }

        public SystemSettingsService(AppDbContext context)
        {
            _context = context;
            _auditService = new AuditService(context);
        }

        public SystemSettingsService() : this(new AppDbContext())
        {
        }

        public AppDbContext GetContext()
        {
            return _context;
        }

        public SystemSettings GetSystemSettings()
        {
            var settings = _context.SystemSettings.FirstOrDefault();
            return settings ?? new SystemSettings
            {
                MaxLoginAttempts = 5,
                LockoutDurationMinutes = 15,
                MinPasswordLength = 8,
                RequireComplexPassword = true,
                AuditLogRetentionDays = 30
            };
        }

        public void UpdateSystemSettings(SystemSettings settings)
        {
            ValidateSettings(settings);

            var existingSettings = _context.SystemSettings.FirstOrDefault();
            var oldSettings = existingSettings != null ? new SystemSettings
            {
                MaxLoginAttempts = existingSettings.MaxLoginAttempts,
                LockoutDurationMinutes = existingSettings.LockoutDurationMinutes,
                MinPasswordLength = existingSettings.MinPasswordLength,
                RequireComplexPassword = existingSettings.RequireComplexPassword,
                AuditLogRetentionDays = existingSettings.AuditLogRetentionDays
            } : null;

            if (existingSettings == null)
            {
                _context.SystemSettings.Add(settings);
                if (_auditService != null && _currentUser != null)
                {
                    _auditService.LogEvent(
                        _currentUser.Id,
                        "Создание настроек",
                        "Созданы начальные системные настройки:\n" +
                        $"Макс. попыток входа: {settings.MaxLoginAttempts}\n" +
                        $"Длительность блокировки: {settings.LockoutDurationMinutes}\n" +
                        $"Мин. длина пароля: {settings.MinPasswordLength}\n" +
                        $"Сложный пароль: {settings.RequireComplexPassword}\n" +
                        $"Хранение логов: {settings.AuditLogRetentionDays}"
                    );
                }
            }
            else
            {
                // Сначала обновляем период хранения логов, если он изменился
                if (oldSettings.AuditLogRetentionDays != settings.AuditLogRetentionDays)
                {
                    _auditService?.UpdateRetentionPeriod(settings.AuditLogRetentionDays, _currentUser);
                }

                // Затем обновляем остальные настройки
                existingSettings.MaxLoginAttempts = settings.MaxLoginAttempts;
                existingSettings.LockoutDurationMinutes = settings.LockoutDurationMinutes;
                existingSettings.MinPasswordLength = settings.MinPasswordLength;
                existingSettings.RequireComplexPassword = settings.RequireComplexPassword;
                existingSettings.AuditLogRetentionDays = settings.AuditLogRetentionDays;

                if (_auditService != null && _currentUser != null)
                {
                    var changes = new StringBuilder();
                    if (oldSettings.MaxLoginAttempts != settings.MaxLoginAttempts)
                        changes.AppendLine($"Макс. попыток входа: {oldSettings.MaxLoginAttempts} -> {settings.MaxLoginAttempts}");
                    if (oldSettings.LockoutDurationMinutes != settings.LockoutDurationMinutes)
                        changes.AppendLine($"Длительность блокировки: {oldSettings.LockoutDurationMinutes} -> {settings.LockoutDurationMinutes}");
                    if (oldSettings.MinPasswordLength != settings.MinPasswordLength)
                        changes.AppendLine($"Мин. длина пароля: {oldSettings.MinPasswordLength} -> {settings.MinPasswordLength}");
                    if (oldSettings.RequireComplexPassword != settings.RequireComplexPassword)
                        changes.AppendLine($"Сложный пароль: {oldSettings.RequireComplexPassword} -> {settings.RequireComplexPassword}");

                    if (changes.Length > 0)
                    {
                        _auditService.LogEvent(
                            _currentUser.Id,
                            "Изменение настроек",
                            $"Изменены системные настройки:\n{changes}"
                        );
                    }
                }
            }

            _context.SaveChanges();
        }

        private void ValidateSettings(SystemSettings settings)
        {
            if (settings.MaxLoginAttempts <= 0)
                throw new ArgumentException("Максимальное количество попыток входа должно быть положительным");

            if (settings.LockoutDurationMinutes < 0)
                throw new ArgumentException("Период блокировки не может быть отрицательным");

            if (settings.MinPasswordLength <= 0)
                throw new ArgumentException("Минимальная длина пароля должна быть положительной");

            if (settings.AuditLogRetentionDays <= 0)
                throw new ArgumentException("Период хранения журнала аудита должен быть положительным");
        }
    }
}
