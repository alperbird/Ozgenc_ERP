using DataAccessLayer.Data;
using DataAccessLayer.Interfaces;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class GeneralPartRepository : IParcalar
    {
        private readonly AppDbContext _context;

        public GeneralPartRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddGeneralPart(F_GeneralPart part)
        {
            await _context.F_MalzemeAdlari.AddAsync(part);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateGeneralPart(F_GeneralPart part)
        {
            _context.F_MalzemeAdlari.Update(part);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGeneralPart(int id)
        {
            var part = await GetGeneralPartById(id);

            if (part != null)
            {
                _context.F_MalzemeAdlari.Remove(part);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<F_GeneralPart>> GetAllGeneralPart()
        {
            return await _context.F_MalzemeAdlari.ToListAsync();
        }

        public async Task<F_GeneralPart?> GetGeneralPartById(int id)
        {
            return await _context.F_MalzemeAdlari.FirstOrDefaultAsync(p => p.Id == id);
        }

    }
}
