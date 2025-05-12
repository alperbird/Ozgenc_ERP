using DataAccessLayer.Data;
using DataAccessLayer.Interfaces;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserAsync(string username, string password)
        {
            return await _context.Users.
                Include(u => u.Role).
                FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.
                Include(u => u.Role).
                ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.
                Include(u => u.Role).
                FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
