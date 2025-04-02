using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface ICountryRepository
    {
        Task<List<Country>> GetAllAsync();

        Task<Country?> GetByIdAsync(int id);

        Task AddAsync(Country c);

        Task UpdateAsync(Country c);

        Task DeleteAsync(int id);
    }
}
