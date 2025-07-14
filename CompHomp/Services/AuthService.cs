using CompHomp.Models;
using System;
using System.Linq;
using System.Text;
using BCrypt.Net;
using CompHomp.Data;

namespace CompHomp.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;

        public AuthService(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public AuthService(AppDbContext context)
        {
            _context = context;
            _auditService = new AuditService(context);
        }

        public User Authenticate(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = _context.Users.FirstOrDefault(u => u.Login == login);
            
            if (user == null)
            {
                // Логируем неудачную попытку входа с несуществующим пользователем
                var unknownUser = new User { Id = 0, Login = "system" };
                _auditService.LogEvent(
                    unknownUser.Id,
                    "Ошибка входа",
                    $"Попытка входа с несуществующим логином: {login}"
                );
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                // Логируем неудачную попытку входа с неверным паролем
                _auditService.LogEvent(
                    user.Id,
                    "Ошибка входа",
                    $"Неудачная попытка входа: неверный пароль"
                );
                return null;
            }

            // Логируем успешный вход
            _auditService.LogEvent(
                user.Id,
                "Вход",
                $"Успешный вход в систему"
            );

            return user;
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public User RegisterUser(string login, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || 
                string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Все поля должны быть заполнены");

            var existingUser = _context.Users.FirstOrDefault(u => 
                u.Login == login || u.Email == email);

            if (existingUser != null)
            {
                // Логируем попытку регистрации с существующим логином/email
                var systemUser = new User { Id = 0, Login = "system" };
                _auditService.LogEvent(
                    systemUser.Id,
                    "Ошибка регистрации",
                    $"Попытка регистрации с существующим логином или email: {login}, {email}"
                );
                throw new InvalidOperationException("Пользователь с таким логином или email уже существует");
            }

            var newUser = new User
            {
                Login = login,
                Email = email,
                Password = HashPassword(password),
                Role = UserRole.Customer,
                RegistrationDate = DateTime.Now
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Логируем успешную регистрацию
            _auditService.LogEvent(
                newUser.Id,
                "Регистрация",
                $"Зарегистрирован новый пользователь: {login}"
            );

            return newUser;
        }
    }
}
