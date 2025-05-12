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
    public class RoleService: IRoleService
    {
        private readonly IRoleRepository _repo;

        public RoleService(IRoleRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _repo.GetAllRolesAsync();
        }
    }
}
