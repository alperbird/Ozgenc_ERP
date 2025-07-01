using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Entities;

namespace BusinessLayer.Interfaces
{
    public interface IStokService
    {
        Task<bool> AddStok(GeneralStock stok);
        Task UpdateStok(GeneralStock stok);
        Task DeleteStok(int id);
        Task<List<GeneralStock>> GetAllStoks();
        Task<GeneralStock?> GetStokById(int id);

        // lazım olursa diye duruyor açarız bi ara
        //Task<List<GeneralStock>> GetStoksByParcaId(int parcaId);
        //Task<List<GeneralStock>> GetStoksByWarehouseId(int warehouseId);
    }
}
