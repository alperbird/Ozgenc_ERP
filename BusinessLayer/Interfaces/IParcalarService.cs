using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IParcalarService
    {
        Task AddParca(F_GeneralPart parca);
        Task UpdateParca(F_GeneralPart parca);
        Task DeleteParca(int id);
        Task<List<F_GeneralPart>> GetAllParcalar();
        Task<F_GeneralPart?> GetParcaById(int id);
    }
}
