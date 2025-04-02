using BusinessLayer.Interfaces;
using DataAccessLayer.Interfaces;
using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository _repo;

        public CountryService(ICountryRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Country>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Country> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task AddCountryAsync(Country c)
        {
            await _repo.AddAsync(c);
        }

        public async Task UpdateCountryAsync(Country c)
        {
            await _repo.UpdateAsync(c);
        }

        public async Task DeleteCountryAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
