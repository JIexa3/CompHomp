using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CompHomp.Models;
using CompHomp.Data;
using System.Windows;

namespace CompHomp.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly LoginAttemptService _loginAttemptService;
        private readonly IAuditService _auditService;

        public UserService(AppDbContext context)
        {
            _context = context;
            _loginAttemptService = new LoginAttemptService(context);
            _auditService = new AuditService(context);
        }

        public UserService(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _loginAttemptService = new LoginAttemptService(context);
            _auditService = auditService;
        }

        public UserService() : this(new AppDbContext())
        {
        }

        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        public User GetUserById(int id)
        {
            return _context.Users.Find(id);
        }

        public User GetUserByLogin(string login)
        {
            return _context.Users.FirstOrDefault(u => u.Login == login);
        }

        public void AddUser(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Login))
                throw new ArgumentException("Логин не может быть пустым");

            if (string.IsNullOrWhiteSpace(user.Password))
                throw new ArgumentException("Пароль не может быть пустым");

            // Проверяем существование пользователя, включая удаленных
            var existingUser = _context.Users.FirstOrDefault(u => u.Login == user.Login);
            if (existingUser != null)
            {
                if (existingUser.IsDeleted)
                    throw new InvalidOperationException("Этот логин принадлежал удаленному аккаунту. Пожалуйста, используйте другой логин.");
                else
                    throw new InvalidOperationException("Пользователь с таким логином уже существует");
            }

            // Проверяем email, включая удаленных пользователей
            existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (existingUser != null)
            {
                if (existingUser.IsDeleted)
                    throw new InvalidOperationException("Этот email принадлежал удаленному аккаунту. Пожалуйста, используйте другой email.");
                else
                    throw new InvalidOperationException("Пользователь с таким email уже существует");
            }

            // Проверяем сложность пароля
            var (isValid, errorMessage) = User.ValidatePassword(user.Password, _context);
            if (!isValid)
                throw new ArgumentException(errorMessage);

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.IsDeleted = false;
            user.RegistrationDate = DateTime.Now;
            user.LoginAttempts = 0;

            _context.Users.Add(user);
            _context.SaveChanges();

            _auditService.LogEvent(user.Id, "Регистрация", $"Добавлен новый пользователь: {user.Login}");
        }

        public void CreateUser(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.Login))
                throw new ArgumentException("Имя пользователя не может быть пустым");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("Email не может быть пустым");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым");

            if (_context.Users.Any(u => u.Login == user.Login))
                throw new InvalidOperationException("Пользователь с таким именем уже существует");

            if (_context.Users.Any(u => u.Email == user.Email))
                throw new InvalidOperationException("Пользователь с таким email уже существует");

            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            _context.Users.Add(user);
            _context.SaveChanges();

            _auditService.LogEvent(user.Id, "Регистрация", $"Создан новый пользователь: {user.Login}");
        }

        public void UpdateUser(User user)
        {
            var existingUser = _context.Users.Find(user.Id);
            if (existingUser == null)
                throw new InvalidOperationException("Пользователь не найден");

            var otherUserWithSameLogin = _context.Users
                .FirstOrDefault(u => u.Login == user.Login && u.Id != user.Id);
            if (otherUserWithSameLogin != null)
                throw new InvalidOperationException("Пользователь с таким логином уже существует");

            var otherUserWithSameEmail = _context.Users
                .FirstOrDefault(u => u.Email == user.Email && u.Id != user.Id);
            if (otherUserWithSameEmail != null)
                throw new InvalidOperationException("Пользователь с таким email уже существует");

            var changes = new System.Text.StringBuilder();
            if (existingUser.Login != user.Login)
                changes.AppendLine($"Логин: {existingUser.Login} -> {user.Login}");
            if (existingUser.Email != user.Email)
                changes.AppendLine($"Email: {existingUser.Email} -> {user.Email}");
            if (existingUser.Role != user.Role)
                changes.AppendLine($"Роль: {existingUser.Role} -> {user.Role}");
            if (existingUser.IsBlocked != user.IsBlocked)
                changes.AppendLine($"Блокировка: {existingUser.IsBlocked} -> {user.IsBlocked}");

            existingUser.Login = user.Login;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.IsBlocked = user.IsBlocked;

            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                changes.AppendLine("Пароль был изменен");
            }

            _context.SaveChanges();

            if (changes.Length > 0)
            {
                _auditService.LogEvent(user.Id, "Изменение пользователя", changes.ToString());
            }
        }

        public void UpdateUserStatus(int userId, bool isActive)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
                throw new InvalidOperationException("Пользователь не найден");

            if (user.Role == UserRole.Admin && 
                _context.Users.Count(u => u.Role == UserRole.Admin && !u.IsBlocked) <= 1 && !isActive)
            {
                throw new InvalidOperationException("Нельзя блокировать последнего активного администратора");
            }

            user.IsBlocked = !isActive;
            _context.SaveChanges();

            _auditService.LogEvent(userId, "Изменение статуса", 
                $"Статус пользователя {user.Login} изменен: {(user.IsBlocked ? "заблокирован" : "разблокирован")}");
        }

        public void DeleteUser(int id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var user = _context.Users.Find(id);
                        if (user == null)
                            throw new InvalidOperationException("Пользователь не найден");

                        if (user.Role == UserRole.Admin && 
                            _context.Users.Count(u => u.Role == UserRole.Admin && !u.IsDeleted) <= 1)
                        {
                            throw new InvalidOperationException("Нельзя удалить последнего администратора");
                        }

                        // Помечаем пользователя как удаленного
                        user.IsDeleted = true;
                        user.IsActive = false;
                        user.IsBlocked = true;
                        
                        _context.SaveChanges();
                        _auditService.LogEvent(user.Id, "Удаление пользователя", $"Пользователь {user.Login} был удален");
                        
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new InvalidOperationException($"Ошибка при удалении пользователя: {ex.Message}");
                    }
                }
            });
        }

        public void BlockUser(int id, bool block)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                throw new InvalidOperationException("Пользователь не найден");

            if (user.Role == UserRole.Admin && 
                _context.Users.Count(u => u.Role == UserRole.Admin && !u.IsBlocked) <= 1)
            {
                throw new InvalidOperationException("Нельзя заблокировать последнего активного администратора");
            }

            user.IsBlocked = block;
            _context.SaveChanges();

            _auditService.LogEvent(id, "Блокировка", 
                $"Пользователь {(block ? "заблокирован" : "разблокирован")}");
        }

        public bool ValidatePassword(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                LogAuthAttempt(0, login, "EmptyLogin", false);
                MessageBox.Show("Введите логин", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                LogAuthAttempt(0, login, "EmptyPassword", false);
                MessageBox.Show("Введите пароль", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Login.Trim() == login.Trim());

                if (user == null)
                {
                    LogAuthAttempt(0, login, "UserNotFound", false);
                    MessageBox.Show("Пользователь не найден", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (user.IsDeleted)
                {
                    LogAuthAttempt(user.Id, login, "DeletedAccount", false);
                    MessageBox.Show("Этот аккаунт был удален", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (user.IsBlocked)
                {
                    LogAuthAttempt(user.Id, login, "UserBlocked", false);
                    MessageBox.Show("Аккаунт заблокирован", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (_loginAttemptService.IsBlocked(login))
                {
                    var blockedMessage = _loginAttemptService.GetBlockedMessage(login);
                    MessageBox.Show(blockedMessage, "Превышено количество попыток", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LogAuthAttempt(user.Id, login, "TooManyAttempts", false);
                    return false;
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

                if (!isPasswordValid)
                {
                    _loginAttemptService.IncrementAttempts(login);
                    LogAuthAttempt(user.Id, login, "InvalidPassword", false);
                    MessageBox.Show("Неверный пароль", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Успешный вход
                _loginAttemptService.ResetAttempts(login);
                LogAuthAttempt(user.Id, login, "Success", true);
                
                // Обновляем информацию о последнем входе
                user.LastLoginAttempt = DateTime.Now;
                user.LoginAttempts = 0;
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка аутентификации: {ex.Message}");
                LogAuthAttempt(0, login, $"Exception: {ex.GetType().Name}", false);
                MessageBox.Show("Произошла ошибка при попытке входа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void LogAuthAttempt(int userId, string login, string status, bool success)
        {
            try 
            {
                var logEntry = new UserAuthLog
                {
                    Login = login,
                    Status = status,
                    Success = success,
                    Timestamp = DateTime.Now
                };

                _context.UserAuthLogs.Add(logEntry);
                _context.SaveChanges();

                var description = success 
                    ? $"Успешный вход в систему" 
                    : $"Неудачная попытка входа: {status}";

                _auditService.LogEvent(
                    userId == 0 ? 1 : userId,  
                    success ? "Вход" : "Ошибка входа",
                    description
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка логирования: {ex.Message}");
            }
        }

        private int _maxLoginAttempts = 5;
        private int _lockoutDurationMinutes = 15;
        private int _minPasswordLength = 8;
        private bool _requireComplexPassword = true;

        public void UpdateSecuritySettings(int maxLoginAttempts, int lockoutDurationMinutes, int minPasswordLength, bool requireComplexPassword)
        {
            _maxLoginAttempts = maxLoginAttempts;
            _lockoutDurationMinutes = lockoutDurationMinutes;
            _minPasswordLength = minPasswordLength;
            _requireComplexPassword = requireComplexPassword;
        }

        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < _minPasswordLength)
                return false;

            if (_requireComplexPassword)
            {
                bool hasUpper = password.Any(char.IsUpper);
                bool hasLower = password.Any(char.IsLower);
                bool hasDigit = password.Any(char.IsDigit);
                bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

                return hasUpper && hasLower && hasDigit && hasSpecial;
            }

            return true;
        }

        public bool CheckLoginAttempts(string login)
        {
            var user = GetUserByLogin(login);
            if (user == null) return true;

            if (user.LoginAttempts >= _maxLoginAttempts)
            {
                if (user.LastLoginAttempt.HasValue && 
                    DateTime.Now < user.LastLoginAttempt.Value.AddMinutes(_lockoutDurationMinutes))
                {
                    return false;
                }
                
                user.LoginAttempts = 0;
                _context.SaveChanges();
            }

            return true;
        }

        public void IncrementLoginAttempts(string login)
        {
            var user = GetUserByLogin(login);
            if (user != null)
            {
                user.LoginAttempts++;
                user.LastLoginAttempt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public void ResetLoginAttempts(string login)
        {
            var user = GetUserByLogin(login);
            if (user != null)
            {
                user.LoginAttempts = 0;
                user.LastLoginAttempt = null;
                _context.SaveChanges();
            }
        }

        public bool CreateUser(string username, string email, string password, UserRole role, bool isActive)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("Имя пользователя не может быть пустым");

                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email не может быть пустым");

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("Пароль не может быть пустым");

                // Проверяем существование пользователя, включая удаленных
                var existingUser = _context.Users.FirstOrDefault(u => u.Login == username);
                if (existingUser != null)
                {
                    if (existingUser.IsDeleted)
                        throw new InvalidOperationException("Этот логин принадлежал удаленному аккаунту. Пожалуйста, используйте другой логин.");
                    else
                        throw new InvalidOperationException("Пользователь с таким именем уже существует");
                }

                // Проверяем email, включая удаленных пользователей
                existingUser = _context.Users.FirstOrDefault(u => u.Email == email);
                if (existingUser != null)
                {
                    if (existingUser.IsDeleted)
                        throw new InvalidOperationException("Этот email принадлежал удаленному аккаунту. Пожалуйста, используйте другой email.");
                    else
                        throw new InvalidOperationException("Пользователь с таким email уже существует");
                }

                // Проверяем сложность пароля
                var (isValid, errorMessage) = User.ValidatePassword(password, _context);
                if (!isValid)
                    throw new ArgumentException(errorMessage);

                var user = new User
                {
                    Login = username,
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = role,
                    IsActive = isActive,
                    IsBlocked = false,
                    IsDeleted = false,
                    RegistrationDate = DateTime.Now,
                    LoginAttempts = 0
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                _auditService.LogEvent(user.Id, "Регистрация", $"Создан новый пользователь: {user.Login}");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка при создании пользователя", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
