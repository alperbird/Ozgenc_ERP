using BusinessLayer.Interfaces;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _service;

        public CountryController(ICountryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var countries = await _service.GetAllAsync();
                return Ok(countries);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Hata!: {ex.Message}");
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var country = await _service.GetByIdAsync(id);
            return Ok(country);
        }

        [HttpPost]
        public async Task<IActionResult> AddCountry([FromBody] CountryCreateDto c)
        {
            var country = new Country
            {
                CountryName = c.CountryName
            };

            await _service.AddCountryAsync(country);
            return Ok(country);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCountry(int id, [FromBody] CountryUpdateDto c)
        {
            if (id != c.Id)
            {
                return BadRequest("Ülke Id uyuşmuyor!"); //400 döndürecek
            }

            var existingCountry = await _service.GetByIdAsync(c.Id);

            if (existingCountry == null)
            {
                return NotFound("Ülke bulunamadı!");
            }

            existingCountry.Id = c.Id;
            existingCountry.CountryName = c.CountryName;

            await _service.UpdateCountryAsync(existingCountry);
            return Ok(existingCountry);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound("Ülke bulunamadı!");
            }

            await _service.DeleteCountryAsync(id);
            return Ok(result);
        }
    }
}
