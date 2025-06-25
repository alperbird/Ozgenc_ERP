using BusinessLayer.Interfaces;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IDepoService _service;

        public WarehouseController(IDepoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepo()
        {
            try
            {
                var depolar = await _service.GetWarehouses();
                return Ok(depolar);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata!: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepoById(int id)
        {
            var depo = await _service.GetWarehouseId(id);
            if (depo == null)
            {
                return NotFound("Depo bulunamadı!"); //404 döndürecek
            }
            return Ok(depo);
        }

        [HttpPost]
        public async Task<IActionResult> AddDepo([FromBody] DepoCreateDto d)
        {
            if (d == null)
            {
                return BadRequest("Depo bilgileri eksik!"); // kodu 400
            }
            var depo = new F_Warehouse
            {
                DepoAdi = d.DepoAdi
            };
            await _service.AddWarehouse(depo);
            return CreatedAtAction(nameof(GetDepoById), new { id = depo.Id }, depo); //Kod 201
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepo([FromBody] DepoUpdateDto d)
        {
            if (d == null)
            {
                return BadRequest("Depo bilgileri eksik!"); //KOD 400
            }
            var depo = await _service.GetWarehouseId(d.Id);
            if (depo == null)
            {
                return NotFound("Depo bulunamadı!"); //KOD 404
            }

            depo.Id = d.Id;
            depo.DepoAdi = d.DepoAdi;
            await _service.UpdateWarehouse(depo);
            return NoContent(); //KOD 204
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepo(int id)
        {
            var depo = await _service.GetWarehouseId(id);
            if (depo == null)
            {
                return NotFound("Depo bulunamadı!"); //KOD 404
            }
            await _service.DeleteWarehouse(id);
            return NoContent(); //KOD 204
        }

    }
}
