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
    public class SupplierService : ITedarikciService
    {
        private readonly ITedarikci _repo;

        public SupplierService(ITedarikci repo)
        {
            _repo = repo;
        }

        public async Task AddTedarikci(F_Supplier tedarikci)
        {
            await _repo.AddSupplier(tedarikci);
        }

        public async Task UpdateTedarikci(F_Supplier tedarikci)
        {
            await _repo.UpdateSupplier(tedarikci);
        }

        public async Task DeleteTedarikci(int id)
        {
            await _repo.DeleteSupplier(id);
        }

        public async Task<List<F_Supplier>> GetAllTedarikcis()
        {
            return await _repo.GetAllSupplier();
        }

        public async Task<F_Supplier?> GetTedarikciById(int id)
        {
            return await _repo.GetSupplierById(id);
        }

    }
}
