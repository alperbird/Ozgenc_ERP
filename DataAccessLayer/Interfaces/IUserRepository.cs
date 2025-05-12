using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserAsync(string username, string password);
        Task<User?> GetUserByIdAsync(int userId);
        Task<List<User>> GetAllUsersAsync();
        Task<bool> AddUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
    }
}
