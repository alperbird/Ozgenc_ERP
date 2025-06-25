using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IParcalar
    {
        Task<List<F_GeneralPart>> GetAllGeneralPart();
        Task<F_GeneralPart?> GetGeneralPartById(int id);
        Task AddGeneralPart(F_GeneralPart part);
        Task UpdateGeneralPart(F_GeneralPart part);
        Task DeleteGeneralPart(int id);
    }
}
