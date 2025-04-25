using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllAsync();

        Task<Order?> GetByIdAsync(int id);

        Task AddAsync(Order o);

        Task UpdateAsync(Order o);

        Task<List<Order>> SearchAsync(string? keyword);

        Task<List<Order>> GetPagedOrderAsync(int page, int pageSize);

        IQueryable<Order> Query();
    }
}
