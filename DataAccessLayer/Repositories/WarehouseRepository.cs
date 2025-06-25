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
    public class WarehouseRepository : IDepo
    {
        private readonly AppDbContext _context;

        public WarehouseRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddWarehouse(F_Warehouse warehouse)
        {
            await _context.F_Depo.AddAsync(warehouse);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateWarehouse(F_Warehouse warehouse)
        {
            _context.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteWarehouse(int id)
        {
            var warehouse = await GetWarehouseById(id);

            if (warehouse != null)
            {
                _context.F_Depo.Remove(warehouse);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<F_Warehouse>> GetAllWarehouses()
        {
            return await _context.F_Depo.ToListAsync();
        }

        public async Task<F_Warehouse?> GetWarehouseById(int id)
        {
            return await _context.F_Depo.FirstOrDefaultAsync(w => w.Id == id);
        }

    }
}
