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
    public class GeneralPartService : IParcalarService
    {
        private readonly IParcalar _repo;

        public GeneralPartService(IParcalar repo)
        {
            _repo = repo;
        }

        public async Task AddParca(F_GeneralPart parca)
        {
            await _repo.AddGeneralPart(parca);
        }

        public async Task UpdateParca(F_GeneralPart parca)
        {
            await _repo.UpdateGeneralPart(parca);
        }

        public async Task DeleteParca(int id)
        {
            await _repo.DeleteGeneralPart(id);
        }

        public async Task<List<F_GeneralPart>> GetAllParcalar()
        {
            return await _repo.GetAllGeneralPart();
        }

        public async Task<F_GeneralPart?> GetParcaById(int id)
        {
            return await _repo.GetGeneralPartById(id);
        }

    }
}
