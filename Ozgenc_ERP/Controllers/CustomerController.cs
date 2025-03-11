using BusinessLayer.Services;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _service;
        public CustomerController(CustomerService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<ActionResult<List<Customer>>> GetAll() // Listeleme
        {
            return await _service.GetAllAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetById(int id) // Detay
        {
            var customer = await _service.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return customer;
        }
        [HttpPost]
        public async Task<ActionResult<Customer>> AddCustomer(Customer c) // Insert
        {
            await _service.AddAsync(c);
            return CreatedAtAction(nameof(GetById), new { id = c.Id }, c);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCustomer(int id, Customer c) // Update
        {
            if (id != c.Id) return BadRequest("ID eşleşmiyor!");
            await _service.UpdateAsync(c);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCustomer(int id) // Delete
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
