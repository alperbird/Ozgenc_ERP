using BusinessLayer.Services;
using EntityLayer.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly StockService _service;

        public StockController(StockService service)
        {
            _service = service;
        }

        [HttpPost("change")]
        public async Task<IActionResult> ChangeStock([FromBody] StockChangeDto dto)
        {
            var result = await _service.ChangeStockAsync(dto.ProductId, dto.Amount);
            return result ? Ok() : NotFound("Ürün bulunamadı.");
        }
    }
}
