using BusinessLayer.Interfaces;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralStockController : ControllerBase
    {
        private readonly IStokService _service;

        public GeneralStockController(IStokService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var stocks = await _service.GetAllStoks();
                return Ok(stocks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata!: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var stock = await _service.GetStokById(id);
            if (stock == null)
            {
                return NotFound($"Stok bulunamadı! Id: {id}");
            }
            return Ok(stock);
        }

        [HttpPost]
        public async Task<IActionResult> AddStock([FromBody] GeneralStockCreateDto s)
        {
            var newStock = new GeneralStock
            {
                GeneralPartId = s.GeneralPartId,
                SupplierId = s.SupplierId,
                WarehouseId = s.WarehouseId,
                OrderDate = s.OrderDate,
                DeliveryDate = s.DeliveryDate,
                OrderQuantity = s.OrderQuantity,
                ReceivedQuantity = s.ReceivedQuantity
            };

            await _service.AddStok(newStock);
            return CreatedAtAction(nameof(GetById), new { id = newStock.Id }, newStock);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock([FromBody] GeneralStockUpdateDto up)
        {
            var existingStock = await _service.GetStokById(up.Id);

            if (existingStock == null)
            {
                return NotFound($"Stok bulunamadı! Id: {up.Id}");
            }

            existingStock.GeneralPartId = up.GeneralPartId;
            existingStock.SupplierId = up.SupplierId;
            existingStock.WarehouseId = up.WarehouseId;
            existingStock.OrderDate = up.OrderDate;
            existingStock.DeliveryDate = up.DeliveryDate;
            existingStock.OrderQuantity = up.OrderQuantity;
            existingStock.ReceivedQuantity = up.ReceivedQuantity;
            await _service.UpdateStok(existingStock);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStock(int id)
        {
            var stock = await _service.GetStokById(id);

            if (stock == null)
                return NotFound();

            await _service.DeleteStok(id);
            return NoContent();
        }
    }
}
