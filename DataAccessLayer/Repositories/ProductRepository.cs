using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.Data;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;




namespace DataAccessLayer.Repositories
{
    public class ProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync() {
            return await _context.Products
                .Include(p => p.ProductMachines!)
                .ThenInclude(pm => pm.Machine)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductMachines)
                .ThenInclude(pm => pm.Machine)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> SearchProductsAsync(string query)
        {
            return await _context.Products
                .AsNoTracking() //Veriyi hafızaya çekmeden işle!
                .Where(p => p.ParcaAdi.Contains(query) ||
                            p.ProductMachines.Any(pm => pm.Machine.MakineAdi.Contains(query)) ||
                            p.Sinif.Contains(query))
                .ToListAsync();
        }


        public async Task AddProduct(Product p, List<int>? makineIdleri)
        {
            await _context.Products.AddAsync(p);
            await _context.SaveChangesAsync(); // Önce Product eklenmeli ki ID oluşsun!

            if (makineIdleri != null && makineIdleri.Any())
            {
                var productMachines = makineIdleri.Select(id => new ProductMachine
                {
                    ProductId = p.Id,  // Yeni eklenen ürünün ID’sini al!
                    MakineId = id
                }).ToList();

                await _context.ProductMachines.AddRangeAsync(productMachines);
                await _context.SaveChangesAsync(); // Many-to-Many tablosuna kaydet!
            }
        }




        public async Task UpdateProduct(Product p)
        {
            var existingProduct = await _context.Products
                .Include(p => p.ProductMachines)
                .FirstOrDefaultAsync(prod => prod.Id == p.Id);

            if (existingProduct != null)
            {
                // **Eski Many-to-Many ilişkilerini temizle**
                _context.ProductMachines.RemoveRange(existingProduct.ProductMachines);

                // **Yeni Many-to-Many ilişkilerini ekle**
                if (p.ProductMachines != null)
                {
                    foreach (var pm in p.ProductMachines)
                    {
                        pm.ProductId = p.Id;
                        _context.ProductMachines.Add(pm);
                    }
                }

                _context.Entry(existingProduct).CurrentValues.SetValues(p);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<bool> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductMachines) // Many-to-Many'yi çekiyoruz
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return false; // Ürün yoksa hata ver
            }

            _context.ProductMachines.RemoveRange(product.ProductMachines); // Bağlı makineleri sil
            _context.Products.Remove(product); // Son olarak ürünü sil
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
