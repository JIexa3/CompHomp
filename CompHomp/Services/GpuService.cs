using System.Collections.Generic;
using System.Linq;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp.Services
{
    public class GpuService
    {
        private readonly AppDbContext _context;

        public GpuService(AppDbContext context)
        {
            _context = context;
        }

        public List<Gpu> GetAll()
        {
            return _context.Gpus.ToList();
        }

        public void Add(Gpu gpu)
        {
            _context.Gpus.Add(gpu);
            _context.SaveChanges();
        }

        public void Update(Gpu gpu)
        {
            _context.Gpus.Update(gpu);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var gpu = _context.Gpus.Find(id);
            if (gpu != null)
            {
                _context.Gpus.Remove(gpu);
                _context.SaveChanges();
            }
        }
    }
}
