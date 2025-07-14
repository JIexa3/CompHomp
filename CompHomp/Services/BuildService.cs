using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CompHomp.Data;
using CompHomp.Models;

namespace CompHomp.Services
{
    public class BuildService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public BuildService(AppDbContext context)
        {
            _context = context;
            _emailService = new EmailService();
        }

        public BuildService() : this(new AppDbContext())
        {
        }

        public async Task<List<Build>> GetAllBuildsAsync()
        {
            return await _context.Builds
                .Include(b => b.User)
                .Include(b => b.Cpu)
                .Include(b => b.Gpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Ram)
                .Include(b => b.Storage)
                .Include(b => b.Psu)
                .Include(b => b.Case)
                .ToListAsync();
        }

        public async Task<Build> GetBuildById(int id)
        {
            var build = await _context.Builds
                .Include(b => b.User)
                .Include(b => b.Cpu)
                .Include(b => b.Gpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Ram)
                .Include(b => b.Storage)
                .Include(b => b.Psu)
                .Include(b => b.Case)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (build == null)
            {
                throw new ArgumentException($"Сборка с ID {id} не найдена.");
            }

            return build;
        }

        public async Task<Build> AddBuild(Build build)
        {
            _context.Builds.Add(build);
            await _context.SaveChangesAsync();
            return build;
        }

        public async Task CreateBuildAsync(Build build)
        {
            if (string.IsNullOrWhiteSpace(build.Name))
                throw new ArgumentException("Название сборки не может быть пустым");

            // Загружаем компоненты
            if (build.CpuId.HasValue)
                build.Cpu = await _context.Cpus.FindAsync(build.CpuId);
            if (build.GpuId.HasValue)
                build.Gpu = await _context.Gpus.FindAsync(build.GpuId);
            if (build.MotherboardId.HasValue)
                build.Motherboard = await _context.Motherboards.FindAsync(build.MotherboardId);
            if (build.RamId.HasValue)
                build.Ram = await _context.Rams.FindAsync(build.RamId);
            if (build.StorageId.HasValue)
                build.Storage = await _context.Storages.FindAsync(build.StorageId);
            if (build.PsuId.HasValue)
                build.Psu = await _context.Psus.FindAsync(build.PsuId);
            if (build.CaseId.HasValue)
                build.Case = await _context.Cases.FindAsync(build.CaseId);

            // Рассчитываем цены
            build.CalculateTotalPrice();

            build.CreatedDate = DateTime.Now;
            build.Status = BuildStatus.Pending;

            _context.Builds.Add(build);

            // Добавляем запись в лог
            var user = await _context.Users.FindAsync(build.UserId);
            var logEntry = new LogEntry
            {
                Username = user?.Login ?? "Система",
                LogType = "Модерация",
                Message = $"Сборка '{build.Name}' отправлена на модерацию.",
                Timestamp = DateTime.Now,
                Details = $"Сборка '{build.Name}' отправлена на модерацию. Базовая цена: {build.BasePrice:C2}, Итоговая цена: {build.TotalPrice:C2}"
            };
            _context.LogEntries.Add(logEntry);

            await _context.SaveChangesAsync();
        }

        public void CreateBuild(Build build)
        {
            if (string.IsNullOrWhiteSpace(build.Name))
                throw new ArgumentException("Название сборки не может быть пустым");

            // Загружаем компоненты
            if (build.CpuId.HasValue)
                build.Cpu = _context.Cpus.Find(build.CpuId);
            if (build.GpuId.HasValue)
                build.Gpu = _context.Gpus.Find(build.GpuId);
            if (build.MotherboardId.HasValue)
                build.Motherboard = _context.Motherboards.Find(build.MotherboardId);
            if (build.RamId.HasValue)
                build.Ram = _context.Rams.Find(build.RamId);
            if (build.StorageId.HasValue)
                build.Storage = _context.Storages.Find(build.StorageId);
            if (build.PsuId.HasValue)
                build.Psu = _context.Psus.Find(build.PsuId);
            if (build.CaseId.HasValue)
                build.Case = _context.Cases.Find(build.CaseId);

            // Рассчитываем цены
            build.CalculateTotalPrice();

            build.CreatedDate = DateTime.Now;
            build.Status = BuildStatus.Pending;

            _context.Builds.Add(build);

            // Добавляем запись в лог
            var user = _context.Users.Find(build.UserId);
            var logEntry = new LogEntry
            {
                Username = user?.Login ?? "Система",
                LogType = "Модерация",
                Message = $"Сборка '{build.Name}' отправлена на модерацию.",
                Timestamp = DateTime.Now,
                Details = $"Сборка '{build.Name}' отправлена на модерацию. Базовая цена: {build.BasePrice:C2}, Итоговая цена: {build.TotalPrice:C2}"
            };
            _context.LogEntries.Add(logEntry);

            _context.SaveChanges();
        }

        public async Task UpdateBuildAsync(Build newBuild, Dictionary<string, (string OldComponent, string NewComponent)> changes = null)
        {
            var oldBuild = await _context.Builds
                .Include(b => b.User)
                .Include(b => b.Cpu)
                .Include(b => b.Gpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Ram)
                .Include(b => b.Storage)
                .Include(b => b.Psu)
                .Include(b => b.Case)
                .FirstOrDefaultAsync(b => b.Id == newBuild.Id);

            if (oldBuild == null)
                throw new InvalidOperationException("Сборка не найдена");

            // Если изменения не переданы, создаем словарь и отслеживаем изменения
            if (changes == null)
            {
                changes = new Dictionary<string, (string OldComponent, string NewComponent)>();

                if ((oldBuild.Cpu?.Name ?? "Не выбран") != (newBuild.Cpu?.Name ?? "Не выбран"))
                {
                    changes["Процессор"] = (oldBuild.Cpu?.Name ?? "Не выбран", newBuild.Cpu?.Name ?? "Не выбран");
                }
                if ((oldBuild.Gpu?.Name ?? "Не выбран") != (newBuild.Gpu?.Name ?? "Не выбран"))
                {
                    changes["Видеокарта"] = (oldBuild.Gpu?.Name ?? "Не выбран", newBuild.Gpu?.Name ?? "Не выбран");
                }
                if ((oldBuild.Motherboard?.Name ?? "Не выбран") != (newBuild.Motherboard?.Name ?? "Не выбран"))
                {
                    changes["Материнская плата"] = (oldBuild.Motherboard?.Name ?? "Не выбран", newBuild.Motherboard?.Name ?? "Не выбран");
                }
                if ((oldBuild.Ram?.Name ?? "Не выбран") != (newBuild.Ram?.Name ?? "Не выбран"))
                {
                    changes["Оперативная память"] = (oldBuild.Ram?.Name ?? "Не выбран", newBuild.Ram?.Name ?? "Не выбран");
                }
                if ((oldBuild.Storage?.Name ?? "Не выбран") != (newBuild.Storage?.Name ?? "Не выбран"))
                {
                    changes["Накопитель"] = (oldBuild.Storage?.Name ?? "Не выбран", newBuild.Storage?.Name ?? "Не выбран");
                }
                if ((oldBuild.Psu?.Name ?? "Не выбран") != (newBuild.Psu?.Name ?? "Не выбран"))
                {
                    changes["Блок питания"] = (oldBuild.Psu?.Name ?? "Не выбран", newBuild.Psu?.Name ?? "Не выбран");
                }
                if ((oldBuild.Case?.Name ?? "Не выбран") != (newBuild.Case?.Name ?? "Не выбран"))
                {
                    changes["Корпус"] = (oldBuild.Case?.Name ?? "Не выбран", newBuild.Case?.Name ?? "Не выбран");
                }
            }

            // Обновляем сборку
            _context.Entry(oldBuild).CurrentValues.SetValues(newBuild);
            await _context.SaveChangesAsync();

            // Отправляем уведомления
            if (oldBuild.User?.Email != null)
            {
                // Если изменился статус
                if (oldBuild.Status != newBuild.Status)
                {
                    await _emailService.SendBuildStatusChangeNotificationAsync(
                        oldBuild.User.Email,
                        oldBuild.Name,
                        newBuild.Status);
                }

                // Если были изменения в компонентах
                if (changes.Any())
                {
                    await _emailService.SendBuildComponentsChangeNotificationAsync(
                        oldBuild.User.Email,
                        oldBuild.Name,
                        changes);
                }
            }
        }

        public void UpdateBuild(Build build)
        {
            // Загружаем компоненты
            if (build.CpuId.HasValue)
                build.Cpu = _context.Cpus.Find(build.CpuId);
            if (build.GpuId.HasValue)
                build.Gpu = _context.Gpus.Find(build.GpuId);
            if (build.MotherboardId.HasValue)
                build.Motherboard = _context.Motherboards.Find(build.MotherboardId);
            if (build.RamId.HasValue)
                build.Ram = _context.Rams.Find(build.RamId);
            if (build.StorageId.HasValue)
                build.Storage = _context.Storages.Find(build.StorageId);
            if (build.PsuId.HasValue)
                build.Psu = _context.Psus.Find(build.PsuId);
            if (build.CaseId.HasValue)
                build.Case = _context.Cases.Find(build.CaseId);

            // Рассчитываем цены
            build.CalculateTotalPrice();

            _context.Builds.Update(build);
            _context.SaveChanges();
        }

        public void DeleteBuild(int id)
        {
            var build = _context.Builds.Find(id);
            if (build == null)
            {
                throw new ArgumentException($"Сборка с ID {id} не найдена.");
            }

            _context.Builds.Remove(build);
            _context.SaveChanges();
        }

        public decimal CalculateBuildTotalPrice(Build build)
        {
            decimal totalPrice = build.BasePrice;

            if (build.CpuId.HasValue)
                totalPrice += _context.Cpus.Find(build.CpuId).Price;
            if (build.GpuId.HasValue)
                totalPrice += _context.Gpus.Find(build.GpuId).Price;
            if (build.MotherboardId.HasValue)
                totalPrice += _context.Motherboards.Find(build.MotherboardId).Price;
            if (build.RamId.HasValue)
                totalPrice += _context.Rams.Find(build.RamId).Price;
            if (build.StorageId.HasValue)
                totalPrice += _context.Storages.Find(build.StorageId).Price;
            if (build.PsuId.HasValue)
                totalPrice += _context.Psus.Find(build.PsuId).Price;
            if (build.CaseId.HasValue)
                totalPrice += _context.Cases.Find(build.CaseId).Price;

            return totalPrice;
        }

        public async Task<List<Build>> GetBuildsByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _context.Builds
                .Include(b => b.User)
                .Include(b => b.Cpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Gpu)
                .Include(b => b.Ram)
                .Include(b => b.Storage)
                .Include(b => b.Psu)
                .Include(b => b.Case)
                .Where(b => b.CreatedDate >= startDate && b.CreatedDate <= endDate)
                .ToListAsync();
        }

        public async Task<List<Build>> GetBuildsForModeration()
        {
            var builds = await _context.Builds
                .Include(b => b.User)
                .Where(b => b.Status == BuildStatus.Pending)
                .ToListAsync();

            foreach (var build in builds)
            {
                build.BasePrice = 0;

                if (build.CpuId.HasValue)
                    build.BasePrice += (await _context.Cpus.FindAsync(build.CpuId)).Price;
                if (build.GpuId.HasValue)
                    build.BasePrice += (await _context.Gpus.FindAsync(build.GpuId)).Price;
                if (build.MotherboardId.HasValue)
                    build.BasePrice += (await _context.Motherboards.FindAsync(build.MotherboardId)).Price;
                if (build.RamId.HasValue)
                    build.BasePrice += (await _context.Rams.FindAsync(build.RamId)).Price;
                if (build.StorageId.HasValue)
                    build.BasePrice += (await _context.Storages.FindAsync(build.StorageId)).Price;
                if (build.PsuId.HasValue)
                    build.BasePrice += (await _context.Psus.FindAsync(build.PsuId)).Price;
                if (build.CaseId.HasValue)
                    build.BasePrice += (await _context.Cases.FindAsync(build.CaseId)).Price;
            }

            return builds;
        }

        public async Task ApproveBuild(int buildId)
        {
            var build = await _context.Builds
                .Include(b => b.User)
                .Include(b => b.Cpu)
                .Include(b => b.Gpu)
                .Include(b => b.Motherboard)
                .Include(b => b.Ram)
                .Include(b => b.Storage)
                .Include(b => b.Psu)
                .Include(b => b.Case)
                .FirstOrDefaultAsync(b => b.Id == buildId);

            if (build == null)
                throw new InvalidOperationException("Сборка не найдена");

            if (build.Status != BuildStatus.Pending)
                throw new InvalidOperationException("Сборка уже прошла модерацию");

            // Пересчитываем базовую цену
            build.BasePrice = 0;

            if (build.CpuId.HasValue)
                build.BasePrice += _context.Cpus.Find(build.CpuId).Price;
            if (build.GpuId.HasValue)
                build.BasePrice += _context.Gpus.Find(build.GpuId).Price;
            if (build.MotherboardId.HasValue)
                build.BasePrice += _context.Motherboards.Find(build.MotherboardId).Price;
            if (build.RamId.HasValue)
                build.BasePrice += _context.Rams.Find(build.RamId).Price;
            if (build.StorageId.HasValue)
                build.BasePrice += _context.Storages.Find(build.StorageId).Price;
            if (build.PsuId.HasValue)
                build.BasePrice += _context.Psus.Find(build.PsuId).Price;
            if (build.CaseId.HasValue)
                build.BasePrice += _context.Cases.Find(build.CaseId).Price;

            // Добавляем наценку 5%
            build.BasePrice = build.BasePrice * 1.05m;

            // Обновляем TotalPrice
            build.TotalPrice = build.BasePrice;

            build.Status = BuildStatus.Approved;
            build.ApprovedDate = DateTime.Now;

            // Добавляем запись в лог
            var logEntry = new LogEntry
            {
                Username = build.User?.Login ?? "Система",
                LogType = "Модерация",
                Message = $"Сборка '{build.Name}' одобрена модератором.",
                Timestamp = DateTime.Now,
                Details = $"Сборка '{build.Name}' (ID: {build.Id}) была одобрена. Базовая цена: {build.BasePrice:C2}, Итоговая цена: {build.TotalPrice:C2}"
            };
            _context.LogEntries.Add(logEntry);
            
            await _context.SaveChangesAsync();

            // Отправляем уведомление асинхронно
            if (build.User?.Email != null)
            {
                await _emailService.SendBuildStatusChangeNotificationAsync(
                    build.User.Email,
                    build.Name,
                    BuildStatus.Approved);
            }
        }

        public async Task RejectBuild(int buildId)
        {
            var build = await _context.Builds
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == buildId);

            if (build == null)
                throw new InvalidOperationException("Сборка не найдена");

            if (build.Status != BuildStatus.Pending)
                throw new InvalidOperationException("Сборка уже прошла модерацию");

            build.Status = BuildStatus.Rejected;
            build.ApprovedDate = DateTime.Now;

            // Добавляем запись в лог
            var logEntry = new LogEntry
            {
                Username = build.User?.Login ?? "Система",
                LogType = "Модерация",
                Message = $"Сборка '{build.Name}' отклонена модератором.",
                Timestamp = DateTime.Now,
                Details = $"Сборка '{build.Name}' (ID: {build.Id}) была отклонена."
            };
            _context.LogEntries.Add(logEntry);

            await _context.SaveChangesAsync();

            // Отправляем уведомление асинхронно
            if (build.User?.Email != null)
            {
                await _emailService.SendBuildStatusChangeNotificationAsync(
                    build.User.Email,
                    build.Name,
                    BuildStatus.Rejected);
            }
        }

        public List<object> GetBuildComponents(Build build)
        {
            var components = new List<object>();

            if (build.CpuId.HasValue)
                components.Add(_context.Cpus.Find(build.CpuId));
            if (build.GpuId.HasValue)
                components.Add(_context.Gpus.Find(build.GpuId));
            if (build.MotherboardId.HasValue)
                components.Add(_context.Motherboards.Find(build.MotherboardId));
            if (build.RamId.HasValue)
                components.Add(_context.Rams.Find(build.RamId));
            if (build.StorageId.HasValue)
                components.Add(_context.Storages.Find(build.StorageId));
            if (build.PsuId.HasValue)
                components.Add(_context.Psus.Find(build.PsuId));
            if (build.CaseId.HasValue)
                components.Add(_context.Cases.Find(build.CaseId));

            return components;
        }
    }
}
