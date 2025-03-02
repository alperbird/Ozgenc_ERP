using DataAccessLayer.Repositories;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class ProductService
    {
        private readonly ProductRepository _repository;

        public ProductService(ProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Product>> GetAllProductAsync() => await _repository.GetAllAsync();

        public async Task<Product?> GetProductByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task<List<Product>> SearchProductsAsync(string query) => await _repository.SearchProductsAsync(query);

        public async Task AddProductAsync(Product product, List<int>? makineIdleri)
        {
            await _repository.AddProduct(product, makineIdleri); // Eksik olan parametreyi ekledik!
        }



        public async Task UpdateProductAsync(Product product)
        {
            var existingProduct = await _repository.GetByIdAsync(product.Id); // ✅ GetByIdAsync kullanıldı

            if (existingProduct != null)
            {
                existingProduct.Sinif = product.Sinif;
                existingProduct.ParcaAdi = product.ParcaAdi;
                existingProduct.FiyatTL = product.FiyatTL;
                existingProduct.FiyatEUR = product.FiyatEUR;

                // **Many-to-Many Güncelleme**
                existingProduct.ProductMachines.Clear();
                if (product.ProductMachines != null)
                {
                    foreach (var pm in product.ProductMachines)
                    {
                        pm.ProductId = product.Id;
                        existingProduct.ProductMachines.Add(pm);
                    }
                }

                await _repository.UpdateProduct(existingProduct);
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _repository.DeleteProduct(id);
        }
    }
    
}
