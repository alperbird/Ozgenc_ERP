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
    public class WarehouseService : IDepoService
    {
        private readonly IDepo _repo;

        public WarehouseService(IDepo repo)
        {
            _repo = repo;
        }

        public async Task AddWarehouse(F_Warehouse warehouse)
        {
            await _repo.AddWarehouse(warehouse);
        }

        public async Task UpdateWarehouse(F_Warehouse warehouse)
        {
            await _repo.UpdateWarehouse(warehouse);
        }

        public async Task DeleteWarehouse(int id)
        {
            await _repo.DeleteWarehouse(id);
        }

        public async Task<List<F_Warehouse>> GetWarehouses()
        {
            return await _repo.GetAllWarehouses();
        }

        public async Task<F_Warehouse?> GetWarehouseId(int id)
        {
            return await _repo.GetWarehouseById(id);
        }

    }
}
