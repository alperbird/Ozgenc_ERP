using DataAccessLayer.Repositories;
using EntityLayer.DTOs;
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

        public async Task UpdateProductAsync(ProductUpdateDto productDto)
        {
            var existingProduct = await _repository.GetByIdWithProductMachinesAsync(productDto.Id);

            if (existingProduct != null)
            {
                existingProduct.Sinif = productDto.Sinif;
                existingProduct.ParcaAdi = productDto.ParcaAdi;
                existingProduct.FiyatTL = productDto.FiyatTL;
                existingProduct.FiyatEUR = productDto.FiyatEUR;
                existingProduct.Aciklama = productDto.Aciklama;
                existingProduct.ResimYolu = productDto.ResimYolu;

                Console.WriteLine("[LOG] Güncelleme Başladı -> Ürün ID: " + productDto.Id);
                Console.WriteLine("[LOG] Eski ilişkili makineler: " + string.Join(", ", existingProduct.ProductMachines.Select(pm => pm.MakineId)));

                try
                {
                    // **Burada hata olup olmadığını logla**
                    Console.WriteLine("[LOG] Silinecek makineler: " + string.Join(", ", existingProduct.ProductMachines.Select(pm => pm.MakineId)));
                    _repository.RemoveProductMachines(existingProduct.ProductMachines.ToList());
                    Console.WriteLine("[LOG] Makineler başarıyla kaldırıldı.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[HATA] Makineleri kaldırırken hata oluştu: " + ex.Message);
                    throw;
                }

                var newProductMachines = productDto.MakineIdleri
                    .Select(id => new ProductMachine { ProductId = productDto.Id, MakineId = id })
                    .ToList();

                await _repository.AddProductMachines(newProductMachines);
                Console.WriteLine("[LOG] Yeni makineler eklendi: " + string.Join(", ", newProductMachines.Select(pm => pm.MakineId)));

                await _repository.UpdateProduct(existingProduct);
                Console.WriteLine("[LOG] Güncelleme başarıyla tamamlandı -> Ürün ID: " + productDto.Id);
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _repository.DeleteProduct(id);
        }

        public async Task<List<ProductStockDto>> GetStockListAsync()
        {
            var products = await _repository.GetAllAsync();

            return products.Select(p => new ProductStockDto
            {
                Id = p.Id,
                ParcaAdi = p.ParcaAdi,
                Stok = p.Stok
            }).ToList();
        }

        public async Task UpdateStockAsync(ProductStockDto dto)
        {
            var product = await _repository.GetByIdAsync(dto.Id);

            if (product == null)
            {
                throw new Exception("Ürün bulunamadı.");
            }

            product.Stok += dto.Stok;
            await _repository.UpdateProduct(product);
        }

        public async Task ChangeStockAsync(ProductStockChangeDto dto)
        {
            var product = await _repository.GetByIdAsync(dto.Id);

            if (product == null)
            {
                throw new Exception("Ürün bulunamadı.");
            }

            product.Stok = dto.QuantityChange;
            await _repository.UpdateProduct(product);
        }
    }
    
}
