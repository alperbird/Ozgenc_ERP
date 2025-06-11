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
                .AsNoTracking()
                .Include(p => p.ProductMachines)
                .ThenInclude(pm => pm.Machine)  // Makine bilgilerini dahil et
                .Where(p => p.ParcaAdi.Contains(query) ||
                            p.Sinif.Contains(query) ||
                            p.ProductMachines.Any(pm => pm.Machine.MakineAdi.Contains(query)))
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
                existingProduct.Sinif = p.Sinif;
                existingProduct.ParcaAdi = p.ParcaAdi;
                existingProduct.FiyatTL = p.FiyatTL;
                existingProduct.FiyatEUR = p.FiyatEUR;
                existingProduct.Aciklama = p.Aciklama;
                existingProduct.ResimYolu = p.ResimYolu;
                existingProduct.ResimData = p.ResimData;

                // **ProductMachines Güncelle**
                if (p.ProductMachines != null)
                {
                    // **Yeni ilişkileri oluştururken ProductId ekle!**
                    existingProduct.ProductMachines = p.ProductMachines
                        .Select(pm => new ProductMachine
                        {
                            Id = 0, // **Yeni kaydı IDENTITY olarak ekle**
                            ProductId = p.Id, // **BURADA ProductId'yi ekliyoruz**
                            MakineId = pm.MakineId
                        })
                        .ToList();
                }

                await _context.SaveChangesAsync();
            }
        }

        #region ProductMachine İşlemleri (Create-Update)
        public async Task ClearProductMachinesAsync(int productId)
        {
            var existingProductMachines = _context.ProductMachines.Where(pm => pm.ProductId == productId).ToList();
            _context.ProductMachines.RemoveRange(existingProductMachines);
            await _context.SaveChangesAsync();
        }


        public async Task<Product> GetByIdWithProductMachinesAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.ProductMachines)
                .ThenInclude(pm => pm.Machine) // **Makine nesnesini de getiriyoruz!**
                .FirstOrDefaultAsync(p => p.Id == productId);
        }


        public void RemoveProductMachines(List<ProductMachine> machinesToRemove)
        {
            _context.ProductMachines.RemoveRange(machinesToRemove);
        }

        public async Task AddProductMachines(List<ProductMachine> machinesToAdd)
        {
            foreach (var pm in machinesToAdd)
            {
                pm.Id = 0; // **ID'leri sıfırla**
                _context.ProductMachines.Add(pm);
            }
            await _context.SaveChangesAsync();
        }

        #endregion

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
