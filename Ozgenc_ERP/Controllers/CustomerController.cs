using BusinessLayer.Services;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
            var customer = await _service.GetAllAsync();
            var dto = customer.Select(c => new CustomerCreateDto
            {
                Id = c.Id,
                CustomerName = c.CustomerName,
                Address = c.Address,
                DeliveryAddress = c.DeliveryAddress,
                Contact = c.Contact,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                TaxNo = c.TaxNo,
                Date = c.Date
            }).ToList();

            return new JsonResult(dto, new JsonSerializerOptions { PropertyNamingPolicy = null});
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
        public async Task<ActionResult<Customer>> AddCustomer(CustomerCreateDto c) // Insert
        {
            var newCustomer = new Customer
            {
                CustomerName = c.CustomerName,
                Address = c.Address,
                DeliveryAddress = c.DeliveryAddress,
                Contact = c.Contact,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                TaxNo = c.TaxNo,
                Date = c.Date
            };
            
            await _service.AddAsync(newCustomer);
            return CreatedAtAction(nameof(GetById), new { id = newCustomer.Id }, newCustomer);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCustomer(int id, Customer c) // Update
        {
            var existingCustomer = await _service.GetByIdAsync(id);
            if(existingCustomer == null)
            {
                return NotFound();
            }

            existingCustomer.CustomerName = c.CustomerName;
            existingCustomer.Address = c.Address;
            existingCustomer.DeliveryAddress = c.DeliveryAddress;
            existingCustomer.Contact = c.Contact;
            existingCustomer.Email = c.Email;
            existingCustomer.PhoneNumber = c.PhoneNumber;
            existingCustomer.TaxNo = c.TaxNo;

            await _service.UpdateAsync(existingCustomer);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCustomer(int id) // Delete
        {
            var customer = await _service.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
