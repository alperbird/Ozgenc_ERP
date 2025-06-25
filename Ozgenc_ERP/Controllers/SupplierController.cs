using BusinessLayer.Interfaces;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ITedarikciService _service;

        public SupplierController(ITedarikciService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetTedarik()
        {
            try
            {
                var tedarikcis = await _service.GetAllTedarikcis();
                return Ok(tedarikcis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata!: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTedarikById(int id)
        {
            var tedarikci = await _service.GetTedarikciById(id);
            if (tedarikci == null)
            {
                return NotFound($"Tedarikçi bulunamadı: {id}");
            }
            return Ok(tedarikci);
        }

        [HttpPost]
        public async Task<IActionResult> AddTedarikci([FromBody] TedarikciCreateDto t)
        {
            var tedarikci = new F_Supplier
            {
                F_TedarikciAdi = t.F_TedarikciAdi
            };

            await _service.AddTedarikci(tedarikci);
            return CreatedAtAction(nameof(GetTedarikById), new { id = tedarikci.Id }, tedarikci);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTedarikci(int id, [FromBody] TedarikciUpdateDto t)
        {
            if (id != t.Id)
            {
                return BadRequest("Tedarikçi Id uyuşmuyor!"); //KOD 400
            }
            var existingTedarikci = await _service.GetTedarikciById(t.Id);
            if (existingTedarikci == null)
            {
                return NotFound("Tedarikçi bulunamadı!");
            }

            existingTedarikci.Id = t.Id;
            existingTedarikci.F_TedarikciAdi = t.F_TedarikciAdi;
            await _service.UpdateTedarikci(existingTedarikci);
            return NoContent(); //KOD 204
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTedarikci(int id)
        {
            var tedarikci = await _service.GetTedarikciById(id);
            if (tedarikci == null)
            {
                return NotFound($"Tedarikçi bulunamadı: {id}");
            }
            await _service.DeleteTedarikci(id);
            return NoContent(); //KOD 204
        }
    }
}
