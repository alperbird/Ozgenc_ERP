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
    public class SupplierRepository : ITedarikci
    {
        private readonly AppDbContext _context;

        public SupplierRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddSupplier(F_Supplier supplier)
        {
            await _context.F_Tedarikci.AddAsync(supplier);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateSupplier(F_Supplier supplier)
        {
            _context.Update(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSupplier(int id)
        {
            var supplier = await GetSupplierById(id);

            if (supplier != null)
            {
                _context.F_Tedarikci.Remove(supplier);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<F_Supplier>> GetAllSupplier()
        {
            return await _context.F_Tedarikci.ToListAsync();
        }

        public async Task<F_Supplier?> GetSupplierById(int id)
        {
            return await _context.F_Tedarikci.FirstOrDefaultAsync(s => s.Id == id);
        }

    }
}
