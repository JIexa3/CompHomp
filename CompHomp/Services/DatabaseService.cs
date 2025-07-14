using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace CompHomp.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InitializeDatabaseWithTestData()
        {
            try
            {
                // Путь к SQL-скрипту
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InitialData.sql");
                
                // Чтение содержимого скрипта
                string script = File.ReadAllText(scriptPath);

                // Создание подключения
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Создание команды
                    SqlCommand command = new SqlCommand(script, connection);

                    // Выполнение скрипта
                    command.ExecuteNonQuery();
                }

                Console.WriteLine("Тестовые данные успешно загружены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке тестовых данных: {ex.Message}");
            }
        }

        public bool CheckDatabaseConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения к базе данных: {ex.Message}");
                return false;
            }
        }
    }
}
