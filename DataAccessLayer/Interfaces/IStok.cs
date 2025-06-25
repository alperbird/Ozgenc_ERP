using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IStok
    {
        Task<List<GeneralStock>> GetAllStock();
        Task<GeneralStock?> GetStockById(int id);
        Task AddStock(GeneralStock stock);
        Task UpdateStock(GeneralStock stock);
        Task DeleteStock(int id);

    }
}
