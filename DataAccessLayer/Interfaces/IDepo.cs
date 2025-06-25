using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IDepo
    {
        Task<List<F_Warehouse>> GetAllWarehouses();
        Task<F_Warehouse?> GetWarehouseById(int id);
        Task AddWarehouse(F_Warehouse warehouse);
        Task UpdateWarehouse(F_Warehouse warehouse);
        Task DeleteWarehouse(int id);

    }
}
