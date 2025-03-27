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
    public class OrderRepository: IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders.
                Include(o => o.Customer).
                Include(o => o.OrderItems).
                ThenInclude(oi => oi.Product).
                OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders.
                Include(o => o.Customer).
                Include(o => o.OrderItems).
                ThenInclude(oi => oi.Product).
                FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task AddAsync(Order o)
        {
            await _context.Orders.AddAsync(o);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order o)
        {

            // Eski siparişleri çekiyoruz...
            var existingOrder = await _context.Orders
                .Include(oi => oi.OrderItems)
                .FirstOrDefaultAsync(oi => oi.Id == o.Id);

            if (existingOrder == null)
            {
                throw new Exception("Sipariş bulunamadı!");
            }

            // Eski sipariş bilgileri...
            existingOrder.CustomerId = o.CustomerId;
            existingOrder.OrderDate = o.OrderDate;
            existingOrder.ShippingFee = o.ShippingFee;
            existingOrder.AdjustmentPercentage = o.AdjustmentPercentage;
            existingOrder.AdjustmentType = o.AdjustmentType;

            // Eski orderitemleri sil...
            _context.OrderItems.RemoveRange(existingOrder.OrderItems);

            //Yeni orderitemleri ekle...
            existingOrder.OrderItems = o.OrderItems;

            _context.Orders.Update(o);
            await _context.SaveChangesAsync();
        }
    }
}
