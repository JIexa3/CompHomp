using System.Collections.Generic;
using System.Linq;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp.Services
{
    public class MotherboardService
    {
        private readonly AppDbContext _context;

        public MotherboardService(AppDbContext context)
        {
            _context = context;
        }

        public List<Motherboard> GetAll()
        {
            return _context.Motherboards.ToList();
        }

        public void Add(Motherboard motherboard)
        {
            _context.Motherboards.Add(motherboard);
            _context.SaveChanges();
        }

        public void Update(Motherboard motherboard)
        {
            _context.Motherboards.Update(motherboard);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var motherboard = _context.Motherboards.Find(id);
            if (motherboard != null)
            {
                _context.Motherboards.Remove(motherboard);
                _context.SaveChanges();
            }
        }
    }
}
