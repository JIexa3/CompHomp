using System.Collections.Generic;
using System.Linq;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp.Services
{
    public class PsuService
    {
        private readonly AppDbContext _context;

        public PsuService(AppDbContext context)
        {
            _context = context;
        }

        public List<Psu> GetAll()
        {
            return _context.Psus.ToList();
        }

        public void Add(Psu psu)
        {
            _context.Psus.Add(psu);
            _context.SaveChanges();
        }

        public void Update(Psu psu)
        {
            _context.Psus.Update(psu);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var psu = _context.Psus.Find(id);
            if (psu != null)
            {
                _context.Psus.Remove(psu);
                _context.SaveChanges();
            }
        }
    }
}
