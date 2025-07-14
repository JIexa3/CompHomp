using CompHomp.Models;
using System;
using System.Linq;
using CompHomp.Data;
using Microsoft.EntityFrameworkCore;

namespace CompHomp.Services
{
    public class UserMigrationService
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public UserMigrationService(AppDbContext context)
        {
            _context = context;
            _authService = new AuthService(context);
        }

        public void MigrateUserPasswords()
        {
            try
            {
                // Check if the database exists and create it if it doesn't
                _context.Database.EnsureCreated();

                // Check if Users table exists by trying to get its count
                if (_context.Users.Any())
                {
                    var users = _context.Users.ToList();
                    foreach (var existingUser in users)
                    {
                        // Если пароль еще не захеширован
                        if (!IsPasswordHashed(existingUser.Password))
                        {
                            existingUser.Password = AuthService.HashPassword(existingUser.Password);
                            
                            // Обновляем роль, если она не установлена
                            if (existingUser.Role == 0)
                            {
                                existingUser.Role = UserRole.Customer;
                            }
                        }
                    }
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - allow the application to continue
                Console.WriteLine($"Error during user password migration: {ex.Message}");
            }
        }

        private bool IsPasswordHashed(string password)
        {
            // BCrypt хеш всегда начинается с "$2a$", "$2b$" или "$2y$"
            return !string.IsNullOrEmpty(password) && 
                   (password.StartsWith("$2a$") || 
                    password.StartsWith("$2b$") || 
                    password.StartsWith("$2y$"));
        }
    }
}
