using System.Collections.Generic;
using System.Linq;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp.Services
{
    public class CaseService
    {
        private readonly AppDbContext _context;

        public CaseService()
        {
            _context = new AppDbContext();
        }

        public CaseService(AppDbContext context)
        {
            _context = context;
        }

        public List<Case> GetAll()
        {
            return _context.Cases.ToList();
        }

        public void Add(Case case_)
        {
            _context.Cases.Add(case_);
            _context.SaveChanges();
        }

        public void Update(Case case_)
        {
            _context.Cases.Update(case_);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var case_ = _context.Cases.Find(id);
            if (case_ != null)
            {
                _context.Cases.Remove(case_);
                _context.SaveChanges();
            }
        }
    }
}
