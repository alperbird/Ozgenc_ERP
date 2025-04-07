using DataAccessLayer.Repositories;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class CustomerService
    {
        private readonly CustomerRepository _repo;

        public CustomerService(CustomerRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CustomerDto>> GetAllAsync()
        {
            var customers = await _repo.GetAllAsync();

            return customers.Select(c => new CustomerDto
            {
                Id = c.Id,
                CustomerName = c.CustomerName,
                CountryName = c.Country != null ? c.Country.CountryName : "",
                CountryId = c.CountryId,
                Address = c.Address,
                DeliveryAddress = c.DeliveryAddress,
                Contact = c.Contact,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                TaxNo = c.TaxNo,
                Date = c.Date,
                LastUpdated = c.LastUpdated
            }
                ).ToList();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task AddAsync(Customer c)
        {
            await _repo.AddAsync(c);
        }

        public async Task UpdateAsync(Customer c)
        {
            await _repo.UpdateAsync(c);
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }

    }
}
