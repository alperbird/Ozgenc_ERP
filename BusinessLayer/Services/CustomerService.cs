using DataAccessLayer.Repositories;
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

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task AddCustomer(Customer c)
        {
            await _repo.AddCustomer(c);
        }

        public async Task UpdateCustomer(Customer c)
        {
            await _repo.UpdateCustomer(c);
        }

        public async Task DeleteCustomer(int id)
        {
            await _repo.DeleteCustomer(id);
        }

    }
}
