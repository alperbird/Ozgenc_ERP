using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrderAsync();
        Task<Order> GetOrderBtIdAsync(int id);
        Task AddOrderAsync(Order o);
        Task UpdateOrderAsync(Order o);
        Task<List<Order>> SearchOrderAsync(string? keyword);
    }
}
