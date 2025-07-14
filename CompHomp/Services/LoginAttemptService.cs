using System;
using System.Linq;
using CompHomp.Data;
using CompHomp.Models;

namespace CompHomp.Services
{
    public class LoginAttemptService
    {
        private readonly AppDbContext _context;
        private readonly SystemSettingsService _settingsService;

        public LoginAttemptService(AppDbContext context)
        {
            _context = context;
            _settingsService = new SystemSettingsService(context);
        }

        public int GetAttempts(string login)
        {
            var attempt = _context.LoginAttempts.FirstOrDefault(a => a.Login == login);
            return attempt?.AttemptsCount ?? 0;
        }

        public void IncrementAttempts(string login)
        {
            var attempt = _context.LoginAttempts.FirstOrDefault(a => a.Login == login);
            if (attempt == null)
            {
                attempt = new LoginAttempt
                {
                    Login = login,
                    AttemptsCount = 1,
                    LastAttemptTime = DateTime.Now
                };
                _context.LoginAttempts.Add(attempt);
            }
            else
            {
                attempt.AttemptsCount++;
                attempt.LastAttemptTime = DateTime.Now;
            }
            _context.SaveChanges();
        }

        public void ResetAttempts(string login)
        {
            var attempt = _context.LoginAttempts.FirstOrDefault(a => a.Login == login);
            if (attempt != null)
            {
                attempt.AttemptsCount = 0;
                attempt.LastAttemptTime = null;
                _context.SaveChanges();
            }
        }

        public bool IsBlocked(string login)
        {
            var attempt = _context.LoginAttempts.FirstOrDefault(a => a.Login == login);
            if (attempt == null) return false;

            var settings = _settingsService.GetSystemSettings();

            if (attempt.AttemptsCount >= settings.MaxLoginAttempts)
            {
                // Проверяем, не истекло ли время блокировки
                if (attempt.LastAttemptTime.HasValue &&
                    DateTime.Now < attempt.LastAttemptTime.Value.AddMinutes(settings.LockoutDurationMinutes))
                {
                    // Не даем делать новые попытки, пока не истечет время блокировки
                    return true;
                }

                // Если время блокировки истекло, сбрасываем счетчик
                ResetAttempts(login);
            }

            return false;
        }

        public string GetBlockedMessage(string login)
        {
            var attempt = _context.LoginAttempts.FirstOrDefault(a => a.Login == login);
            if (attempt == null) return null;

            var settings = _settingsService.GetSystemSettings();

            if (attempt.AttemptsCount >= settings.MaxLoginAttempts && attempt.LastAttemptTime.HasValue)
            {
                var blockEndTime = attempt.LastAttemptTime.Value.AddMinutes(settings.LockoutDurationMinutes);
                var remainingTime = blockEndTime - DateTime.Now;

                if (remainingTime.TotalMinutes > 0)
                {
                    return $"Превышено количество попыток входа. Попробуйте снова через {(int)remainingTime.TotalMinutes} минут";
                }
            }

            return null;
        }
    }
}