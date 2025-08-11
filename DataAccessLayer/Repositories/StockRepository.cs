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
    public class StockRepository : IStok
    {
        private readonly AppDbContext _context;

        public StockRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddStock(GeneralStock stock)
        {
            await _context.F_StokGiris.AddAsync(stock);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateStock(GeneralStock stock)
        {
            _context.F_StokGiris.Update(stock);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStock(int id)
        {
            var stock = await _context.F_StokGiris.FindAsync(id);

            if (stock != null)
            {
                _context.F_StokGiris.Remove(stock);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<GeneralStock>> GetAllStock()
        {
            return await _context.F_StokGiris
                .Include(s => s.GeneralPart)
                .Include(s => s.Supplier)
                .Include(s => s.Warehose)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<GeneralStock?> GetStockById(int id)
        {
            return await _context.F_StokGiris
                .FirstOrDefaultAsync(s => s.Id == id);
        }

    }
}
