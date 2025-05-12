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
    public class UserService: IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<User?> GetUserAsync(string username, string password)
        {
            return await _repo.GetUserAsync(username, password);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _repo.GetUserByIdAsync(userId);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _repo.GetAllUsersAsync();
        }

        public async Task<bool> AddUserAsync(User user)
        {
            return await _repo.AddUserAsync(user);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            return await _repo.UpdateUserAsync(user);
        }
    }
}
