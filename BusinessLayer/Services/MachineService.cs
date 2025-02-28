using DataAccessLayer.Repositories;
using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class MachineService
    {
        private readonly MachineRepository _repo;

        public MachineService(MachineRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Machine>> GetAllMachineAsync()
        {
            var machines = await _repo.GetAllAsync();
            return machines ?? new List<Machine>(); // Eğer null gelirse boş liste dön
        }

        }
}

