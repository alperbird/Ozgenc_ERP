using BusinessLayer.Services;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductController(ProductService service)
        {
            _service = service;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<List<ProductDto>>> GetAllProducts()
        {
            var products = await _service.GetAllProductAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Sinif = p.Sinif,
                ParcaAdi = p.ParcaAdi,
                FiyatTL = p.FiyatTL,
                FiyatEUR = p.FiyatEUR,
                MakineAdi = p.ProductMachines != null && p.ProductMachines.Any()
            ? string.Join(", ", p.ProductMachines.Select(pm => pm.Machine.MakineAdi))
            : "Makine Yok",
                ProductMachines = p.ProductMachines?.Select(pm => new ProductMachineDto
                {
                    Id = pm.Id,
                    ProductId = pm.ProductId,
                    MakineId = pm.MakineId,
                    Machine = new MachineDto { Id = pm.Machine.Id, MakineAdi = pm.Machine.MakineAdi }
                }).ToList()
            }).ToList();

            return Ok(productDtos);
        }


        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<Product>> Get(int id)
        {
            var product = await _service.GetProductByIdAsync(id);
            return product is null ? NotFound(new { message = "Ürün bulunamadı!" }) : Ok(product);
        }

        [HttpGet("search")]
        [Produces("application/json")]
        public async Task<IActionResult> SearchProducts([FromQuery] string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(await _service.GetAllProductAsync()); // Eğer boşsa, tüm listeyi döndür

            var result = await _service.SearchProductsAsync(query);
            return Ok(result);
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Create([FromBody] ProductDto productDto)
        {
            if (productDto == null)
            {
                return BadRequest(new { message = "Ürün bilgisi boş olamaz!" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Model doğrulama hatalarını dön
            }

            // DTO'yu Model'e çeviriyoruz
            var product = new Product
            {
                Sinif = productDto.Sinif,
                ParcaAdi = productDto.ParcaAdi,
                FiyatTL = productDto.FiyatTL,
                FiyatEUR = productDto.FiyatEUR,
                ProductMachines = productDto.ProductMachines?.Select(pm => new ProductMachine
                {
                    MakineId = pm.MakineId,
                    ProductId = pm.ProductId
                }).ToList()
            };

            await _service.AddProductAsync(product);

            // Eklenen ürünü geri dönmek için DTO formatına çevir
            var createdProductDto = new ProductDto
            {
                Id = product.Id,
                Sinif = product.Sinif,
                ParcaAdi = product.ParcaAdi,
                FiyatTL = product.FiyatTL,
                FiyatEUR = product.FiyatEUR,
                ProductMachines = product.ProductMachines?.Select(pm => new ProductMachineDto
                {
                    Id = pm.Id,
                    ProductId = pm.ProductId,
                    MakineId = pm.MakineId,
                    Machine = new MachineDto { Id = pm.Machine?.Id ?? 0, MakineAdi = pm.Machine?.MakineAdi }
                }).ToList()
            };

            return CreatedAtAction(nameof(Get), new { id = createdProductDto.Id }, createdProductDto);
        }


        [HttpPut("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (product == null || id != product.Id)
            {
                return BadRequest(new { message = "Güncellenecek ürün geçersiz!" });
            }

            await _service.UpdateProductAsync(product);
            return Ok(new { message = "Ürün başarıyla güncellendi!" });
        }

        [HttpDelete("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _service.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Silinecek ürün bulunamadı!" });
            }

            await _service.DeleteProductAsync(id);
            return Ok(new { message = "Ürün başarıyla silindi!" });
        }
    }

}
