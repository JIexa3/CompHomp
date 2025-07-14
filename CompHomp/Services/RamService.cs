using System.Collections.Generic;
using System.Linq;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp.Services
{
    public class RamService
    {
        private readonly AppDbContext _context;

        public RamService(AppDbContext context)
        {
            _context = context;
        }

        public List<Ram> GetAll()
        {
            return _context.Rams.ToList();
        }

        public void Add(Ram ram)
        {
            _context.Rams.Add(ram);
            _context.SaveChanges();
        }

        public void Update(Ram ram)
        {
            _context.Rams.Update(ram);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var ram = _context.Rams.Find(id);
            if (ram != null)
            {
                _context.Rams.Remove(ram);
                _context.SaveChanges();
            }
        }
    }
}
