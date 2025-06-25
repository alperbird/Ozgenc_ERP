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
    public class GeneralStockService : IStokService
    {
        private readonly IStok _repo;

        public GeneralStockService(IStok repo)
        {
            _repo = repo;
        }

        public async Task AddStok(GeneralStock stok)
        {
            await _repo.AddStock(stok);
        }

        public async Task UpdateStok(GeneralStock stok)
        {
           await _repo.UpdateStock(stok);
        }

        public async Task DeleteStok(int id)
        {
            await _repo.DeleteStock(id);
        }

        public async Task<List<GeneralStock>> GetAllStoks()
        {
            return await _repo.GetAllStock();
        }

        public async Task<GeneralStock?> GetStokById(int id)
        {
            return await _repo.GetStockById(id);
        }

    }
}
