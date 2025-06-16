using BusinessLayer.Services;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult> GetAllProducts()
        {
            var products = await _service.GetAllProductAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Sinif = p.Sinif,
                ParcaAdi = p.ParcaAdi,
                ParcaIngilizce = p.ParcaIngilizce,
                FiyatTL = p.FiyatTL,
                FiyatEUR = p.FiyatEUR,
                ResimYolu = p.ResimYolu,
                ResimData = p.ResimData,
                Aciklama = p.Aciklama,
                Stok = p.Stok,
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
        public async Task<IActionResult> Create([FromBody] ProductCreateDto productDto)
        {
            Console.WriteLine($"Gelen Resim Yolu: {productDto.ResimYolu}");

            if (productDto == null)
                return BadRequest(new { message = "Ürün bilgisi boş olamaz!" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                Sinif = productDto.Sinif,
                ParcaAdi = productDto.ParcaAdi,
                ParcaIngilizce = productDto.ParcaIngilizce,
                FiyatTL = productDto.FiyatTL,
                FiyatEUR = productDto.FiyatEUR,
                Aciklama = productDto.Aciklama,
                ResimYolu = productDto.ResimYolu,
                ResimData = productDto.ResimData,
            };

            await _service.AddProductAsync(product, productDto.MakineIdleri);

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }


        [HttpPut("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto productDto)
        {

            if (productDto == null || id != productDto.Id)
            {
                return BadRequest(new { message = "Güncellenecek ürün geçersiz!" });
            }

            try
            {
                await _service.UpdateProductAsync(productDto);
                //Console.WriteLine($"Gelen Resim Yolu: {productDto.ResimYolu} / {productDto.ResimData}");

                return Ok(new
                {
                    message = "Ürün başarıyla güncellendi!",
                    updatedProduct = productDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Güncelleme sırasında hata oluştu!", error = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteProductAsync(id);

            if (!result)
            {
                return NotFound(new { message = "Ürün bulunamadı veya silinemedi!" });
            }

            return NoContent(); // 204 - Başarılı ama içerik yok
        }

        [HttpGet("stocklist")]
        public async Task<IActionResult> GetStockList()
        {
            var result = await _service.GetStockListAsync();
            return Ok(result);
        }

        [HttpPut("update-stock")]
        public async Task<IActionResult> Update([FromBody] ProductStockDto dto)
        {
            await _service.UpdateStockAsync(dto);
            return Ok(new { message = "Stok güncellendi!" });
        }

        [HttpPut("change-stock")]
        public async Task<IActionResult> QuantityUpdate([FromBody] ProductStockChangeDto dto)
        {
            await _service.ChangeStockAsync(dto);
            return Ok(new { message = "Stok güncellendi!" });
        }
    }

}
