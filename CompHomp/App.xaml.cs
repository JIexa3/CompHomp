using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CompHomp.Data;
using CompHomp.Services;
using Microsoft.EntityFrameworkCore;

namespace CompHomp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Регистрация DbContext
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(
                            Environment.GetEnvironmentVariable("COMPHOMP_CONNECTION_STRING")
                            ?? "Data Source=HOME-PC;Initial Catalog=CompHompDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False",
                            sqlOptions => sqlOptions.EnableRetryOnFailure()
                        )
                    );

                    // Регистрация сервисов
                    services.AddTransient<CartService>();
                    services.AddTransient<OrderService>();
                    services.AddTransient<AuditService>();
                    services.AddTransient<SystemSettingsService>();
                    services.AddTransient<AuthService>();
                    services.AddTransient<EmailService>();
                    services.AddTransient<UserMigrationService>();

                    // Регистрация окон
                    services.AddTransient<MainWindow>();
                    services.AddTransient<SalesStatisticsWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await _host.StartAsync();

            // Ensure database is created and migrated
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
            }
            
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}
