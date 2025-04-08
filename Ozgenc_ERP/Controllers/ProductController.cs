using BusinessLayer.Services;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
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
                ResimYolu = p.ResimYolu,
                Aciklama = p.Aciklama,
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
                FiyatTL = productDto.FiyatTL,
                FiyatEUR = productDto.FiyatEUR,
                Aciklama = productDto.Aciklama,
                ResimYolu = productDto.ResimYolu
            };

            await _service.AddProductAsync(product, productDto.MakineIdleri);

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        [HttpPost("upload-photo")]
        public async Task<IActionResult> ResimYukle(IFormFile file)
        {
            if(file == null || file.Length == 0)
            {
                Console.WriteLine("FOTOĞRAF YOK AMK!");
                return BadRequest("Fotoğraf yok veya yüklenemedi.");
            }

            var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "YedekParcaAssets");

            if (!Directory.Exists(imageFolder)) //eğer klasör yoksa, bu klasörü oluştur.
            {
                Directory.CreateDirectory(imageFolder);
            }

            //orijinal dosya adını al ve klasörün içine kaydet
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(imageFolder, fileName);

            //aynı isimde dosya varsa, dosya adının sonuna sayı ekle
            int count = 1;
            string finalPath = filePath;
            while(System.IO.File.Exists(finalPath))
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string ext = Path.GetExtension(fileName);
                finalPath = Path.Combine(imageFolder, $"{fileNameWithoutExt}_{count}{ext}");
                count++;
            }

            //dosyayı klasöre kaydet
            using (var stream = new FileStream(finalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            Console.WriteLine($"Fotoğraf başarıyla kaydedildi: {finalPath}");

            //resim yolu olarak kaydedilecek olan dosyanın adını al (veritabanına kaydedilecek format)
            var relativePath = Path.Combine("YedekParcaAssets", Path.GetFileName(finalPath)).Replace("\\","/");

            return Ok(new {photoPath = relativePath});
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
                Console.WriteLine($"Gelen Resim Yolu: {productDto.ResimYolu}");

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

    }

}
