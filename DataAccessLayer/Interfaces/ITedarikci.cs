using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface ITedarikci
    {
        Task<List<F_Supplier>> GetAllSupplier();
        Task<F_Supplier?> GetSupplierById(int id);
        Task AddSupplier(F_Supplier supplier);
        Task UpdateSupplier(F_Supplier supplier);
        Task DeleteSupplier(int id);

    }
}
