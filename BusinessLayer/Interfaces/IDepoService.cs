using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IDepoService
    {
        Task AddWarehouse(F_Warehouse warehouse);
        Task UpdateWarehouse(F_Warehouse warehouse);
        Task DeleteWarehouse(int id);
        Task<List<F_Warehouse>> GetWarehouses();
        Task<F_Warehouse?> GetWarehouseId(int id);
    }
}
