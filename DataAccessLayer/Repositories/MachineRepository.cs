using DataAccessLayer.Data;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    
    public class MachineRepository
    {
        private readonly AppDbContext _context;

        public MachineRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Machine>> GetAllAsync()
        {
            return await _context.Machines
                .AsNoTracking()
                .Select(m => new Machine { Id = m.Id, MakineAdi = m.MakineAdi }) // Sadece ihtiyacımız olan alanlar!
                .ToListAsync();
        }
    }
}
