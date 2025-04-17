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
    public class OrderRepository : IOrderRepository
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
                ThenInclude(s => s.Country).
                Include(o => o.OrderItems).
                ThenInclude(oi => oi.Product).
                OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders.
                Include(o => o.Customer).
                ThenInclude(s => s.Country).
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
            var existingOrder = await _context.Orders
                .Include(oi => oi.OrderItems)
                .FirstOrDefaultAsync(oi => oi.Id == o.Id);

            if (existingOrder == null)
                throw new Exception("Sipariş bulunamadı!");

            //Alanları güncelle
            existingOrder.CustomerId = o.CustomerId;
            existingOrder.OrderDate = o.OrderDate;
            existingOrder.ShippingFee = o.ShippingFee;
            existingOrder.AdjustmentPercentage = o.AdjustmentPercentage;
            existingOrder.AdjustmentType = o.AdjustmentType;

            // OrderItem'ları sıfırla ve yeni gelenlerle doldur
            _context.OrderItems.RemoveRange(existingOrder.OrderItems);
            existingOrder.OrderItems = o.OrderItems;

            _context.Orders.Update(existingOrder);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Order>> SearchAsync(string? keyword)
        {
            var query = _context.Orders
        .Include(o => o.Customer).ThenInclude(c => c.Country)
        .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
        .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(o =>
                    o.OrderItems.Any(item =>
                        (item.CustomProductName ?? item.Product.ParcaAdi)
                            .ToLower().Contains(keyword.ToLower())
                    )
                );
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }
    }
}
