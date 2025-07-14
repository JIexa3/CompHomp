using Microsoft.EntityFrameworkCore;
using CompHomp.Models;

namespace CompHomp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Build> Builds { get; set; }
        public DbSet<PurchaseHistory> PurchaseHistories { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        
        // Компоненты
        public DbSet<Cpu> Cpus { get; set; }
        public DbSet<Gpu> Gpus { get; set; }
        public DbSet<Ram> Rams { get; set; }
        public DbSet<Storage> Storages { get; set; }
        public DbSet<Motherboard> Motherboards { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Psu> Psus { get; set; }

        // Дополнительные сущности
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleStatistics> SaleStatistics { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<UserAuthLog> UserAuthLogs { get; set; }
        public DbSet<PurchaseHistoryItem> PurchaseHistoryItems { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<BuildRating> BuildRatings { get; set; }
        public DbSet<BuildComment> BuildComments { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public AppDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    Environment.GetEnvironmentVariable("COMPHOMP_CONNECTION_STRING") 
                    ?? "Data Source=HOME-PC;Initial Catalog=CompHompDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False",
                    options => options.EnableRetryOnFailure()
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.Property(e => e.Username).HasColumnName("Username");
                entity.Property(e => e.EventType).HasColumnName("Action");
                entity.Property(e => e.Description).HasColumnName("Details");
            });

            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    Login = "admin", 
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"), 
                    Email = "admin@comphomp.com", 
                    Role = UserRole.Admin,
                    RegistrationDate = DateTime.Now,
                    IsActive = true,
                    IsBlocked = false,
                    IsDeleted = false,
                    LoginAttempts = 0
                },
                new User 
                { 
                    Id = 2, 
                    Login = "user1", 
                    Password = BCrypt.Net.BCrypt.HashPassword("user123"), 
                    Email = "user1@comphomp.com", 
                    Role = UserRole.Customer,
                    RegistrationDate = DateTime.Now,
                    IsActive = true,
                    IsBlocked = false,
                    IsDeleted = false,
                    LoginAttempts = 0
                }
            );

            modelBuilder.Entity<Cpu>().HasData(
                new Cpu 
                { 
                    Id = 1, 
                    Name = "Intel Core i7-12700K", 
                    Price = 45000.00m, 
                    Socket = "LGA1700", 
                    Cores = 12, 
                    BaseClockSpeed = 3.6, 
                    TDP = 125
                },
                new Cpu 
                { 
                    Id = 2, 
                    Name = "AMD Ryzen 9 5950X", 
                    Price = 75000.00m, 
                    Socket = "AM4", 
                    Cores = 16, 
                    BaseClockSpeed = 3.4, 
                    TDP = 105
                }
            );

            modelBuilder.Entity<Gpu>().HasData(
                new Gpu 
                { 
                    Id = 1, 
                    Name = "NVIDIA GeForce RTX 3080", 
                    Price = 80000.00m, 
                    MemorySize = 10, 
                    MemoryType = "GDDR6X", 
                    CoreClockSpeed = 1440, 
                    TDP = 320
                },
                new Gpu 
                { 
                    Id = 2, 
                    Name = "AMD Radeon RX 6800 XT", 
                    Price = 70000.00m, 
                    MemorySize = 16, 
                    MemoryType = "GDDR6", 
                    CoreClockSpeed = 1825, 
                    TDP = 300
                }
            );

            modelBuilder.Entity<Ram>().HasData(
                new Ram 
                { 
                    Id = 1, 
                    Name = "Corsair Vengeance LPX 32GB", 
                    Price = 15000.00m, 
                    Capacity = 32, 
                    Type = "DDR4", 
                    Speed = 3200, 
                    ModulesCount = 2
                },
                new Ram 
                { 
                    Id = 2, 
                    Name = "G.Skill Ripjaws V 64GB", 
                    Price = 25000.00m, 
                    Capacity = 64, 
                    Type = "DDR4", 
                    Speed = 3600, 
                    ModulesCount = 4
                }
            );

            modelBuilder.Entity<Motherboard>().HasData(
                new Motherboard 
                { 
                    Id = 1, 
                    Name = "ASUS ROG Strix Z690-E Gaming", 
                    Price = 35000.00m, 
                    Socket = "LGA1700", 
                    Chipset = "Intel Z690", 
                    FormFactor = "ATX", 
                    MemoryType = "DDR4"
                },
                new Motherboard 
                { 
                    Id = 2, 
                    Name = "MSI MPG B550 Gaming Carbon WiFi", 
                    Price = 25000.00m, 
                    Socket = "AM4", 
                    Chipset = "AMD B550", 
                    FormFactor = "ATX", 
                    MemoryType = "DDR4"
                }
            );

            modelBuilder.Entity<Storage>().HasData(
                new Storage 
                { 
                    Id = 1, 
                    Name = "Samsung 980 PRO 1TB NVMe SSD", 
                    Price = 18000.00m, 
                    Type = "NVMe", 
                    Capacity = 1000, 
                    FormFactor = "M.2", 
                    ReadSpeed = 7000
                },
                new Storage 
                { 
                    Id = 2, 
                    Name = "Western Digital Black SN850 2TB NVMe SSD", 
                    Price = 30000.00m, 
                    Type = "NVMe", 
                    Capacity = 2000, 
                    FormFactor = "M.2", 
                    ReadSpeed = 7000
                }
            );

            modelBuilder.Entity<Psu>().HasData(
                new Psu 
                { 
                    Id = 1, 
                    Name = "Corsair RM850x", 
                    Price = 15000.00m, 
                    Power = 850, 
                    FormFactor = "ATX", 
                    Efficiency = "80+ Gold", 
                    IsModular = true
                },
                new Psu 
                { 
                    Id = 2, 
                    Name = "EVGA SuperNOVA 1000 P6", 
                    Price = 20000.00m, 
                    Power = 1000, 
                    FormFactor = "ATX", 
                    Efficiency = "80+ Platinum", 
                    IsModular = true
                }
            );

            modelBuilder.Entity<Case>().HasData(
                new Case 
                { 
                    Id = 1, 
                    Name = "NZXT H510", 
                    FormFactor = "Mid Tower", 
                    MotherboardSupport = "ATX", 
                    Price = 10000.00m, 
                    MaxGPULength = 360, 
                    Color = "Black"
                },
                new Case 
                { 
                    Id = 2, 
                    Name = "Fractal Design Meshify C", 
                    FormFactor = "Mid Tower", 
                    MotherboardSupport = "ATX", 
                    Price = 12000.00m, 
                    MaxGPULength = 315, 
                    Color = "Black"
                }
            );

            modelBuilder.Entity<Build>().HasData(
                new Build 
                { 
                    Id = 1, 
                    Name = "Gaming Beast", 
                    Description = "High-performance gaming rig", 
                    BasePrice = 250000.00m, 
                    UserId = 1,
                    CpuId = 1,
                    GpuId = 1,
                    MotherboardId = 1,
                    RamId = 1,
                    StorageId = 1,
                    PsuId = 1,
                    CaseId = 1,
                    IsCustom = false,
                    Status = BuildStatus.Approved,
                    CreatedDate = DateTime.Now
                },
                new Build 
                { 
                    Id = 2, 
                    Name = "Workstation Pro", 
                    Description = "Powerful workstation for professionals", 
                    BasePrice = 350000.00m, 
                    UserId = 1,
                    CpuId = 2,
                    GpuId = 2,
                    MotherboardId = 2,
                    RamId = 2,
                    StorageId = 2,
                    PsuId = 2,
                    CaseId = 2,
                    IsCustom = false,
                    Status = BuildStatus.Approved,
                    CreatedDate = DateTime.Now
                }
            );

            modelBuilder.Entity<Build>()
                .HasOne(b => b.User)
                .WithMany(u => u.Builds)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sales)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseHistoryItem>()
                .HasOne(phi => phi.Build)
                .WithMany(b => b.PurchaseHistoryItems)
                .HasForeignKey(phi => phi.BuildId);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.User)
                .WithMany()
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseHistory>()
                .HasMany(ph => ph.Items)
                .WithOne(phi => phi.PurchaseHistory)
                .HasForeignKey(phi => phi.PurchaseHistoryId);

            modelBuilder.Entity<PurchaseHistoryItem>()
                .HasOne(phi => phi.Build)
                .WithMany(b => b.PurchaseHistoryItems)
                .HasForeignKey(phi => phi.BuildId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
