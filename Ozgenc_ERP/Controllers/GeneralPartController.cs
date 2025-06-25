using BusinessLayer.Interfaces;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralPartController : ControllerBase
    {
        private readonly IParcalarService _service;

        public GeneralPartController(IParcalarService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var parts = await _service.GetAllParcalar();
                return Ok(parts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata!: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var part = await _service.GetParcaById(id);
            if (part == null)
            {
                return NotFound($"Parça bulunamadı: {id}");
            }
            return Ok(part);
        }

        [HttpPost]
        public async Task<IActionResult> AddParca([FromBody] GenelParcaCreateDto p)
        {
            var parca = new F_GeneralPart
            {
                F_MalzemeAdi = p.F_MalzemeAdi,
            };

            await _service.AddParca(parca);
            return CreatedAtAction(nameof(GetById), new { id = parca.Id }, parca);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateParca(int id, [FromBody] GenelParcaUpdateDto p)
        {
            if (id != p.Id)
            {
                return BadRequest("Parça Id uyuşmuyor!"); //KOD 400
            }
            var existingPart = await _service.GetParcaById(id);
            if (existingPart == null)
            {
                return NotFound("Parça bulunamadı!");
            }

            existingPart.Id = p.Id; 
            existingPart.F_MalzemeAdi = p.F_MalzemeAdi;
            await _service.UpdateParca(existingPart);
            return NoContent(); //KOD 204
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParca(int id)
        {
            var parca = await _service.GetParcaById(id);

            if (parca == null)
            {
                return NotFound($"Parça bulunamadı: {id}");
            }

            await _service.DeleteParca(id);
            return Ok(parca); //KOD 200
        }
    }
}
