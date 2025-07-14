using System.Collections.Generic;
using System.Linq;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp.Services
{
    public class StorageService
    {
        private readonly AppDbContext _context;

        public StorageService()
        {
            _context = new AppDbContext();
        }

        public StorageService(AppDbContext context)
        {
            _context = context;
        }

        public List<Storage> GetAll()
        {
            return _context.Storages.ToList();
        }

        public void Add(Storage storage)
        {
            _context.Storages.Add(storage);
            _context.SaveChanges();
        }

        public void Update(Storage storage)
        {
            _context.Storages.Update(storage);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var storage = _context.Storages.Find(id);
            if (storage != null)
            {
                _context.Storages.Remove(storage);
                _context.SaveChanges();
            }
        }
    }
}
