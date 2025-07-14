using System.Collections.Generic;
using System.Linq;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp.Services
{
    public class CpuService
    {
        private readonly AppDbContext _context;

        public CpuService(AppDbContext context)
        {
            _context = context;
        }

        public List<Cpu> GetAll()
        {
            return _context.Cpus.ToList();
        }

        public void Add(Cpu cpu)
        {
            _context.Cpus.Add(cpu);
            _context.SaveChanges();
        }

        public void Update(Cpu cpu)
        {
            _context.Cpus.Update(cpu);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var cpu = _context.Cpus.Find(id);
            if (cpu != null)
            {
                _context.Cpus.Remove(cpu);
                _context.SaveChanges();
            }
        }
    }
}
