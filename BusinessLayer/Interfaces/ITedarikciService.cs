using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Entities;

namespace BusinessLayer.Interfaces
{
    public interface ITedarikciService
    {
        Task AddTedarikci(F_Supplier tedarikci);
        Task UpdateTedarikci(F_Supplier tedarikci);
        Task DeleteTedarikci(int id);
        Task<List<F_Supplier>> GetAllTedarikcis();
        Task<F_Supplier?> GetTedarikciById(int id);
    }
}
