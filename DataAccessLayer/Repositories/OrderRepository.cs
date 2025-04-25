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
                ThenInclude(s => s.CountryName).
                Include(o => o.OrderItems).
                ThenInclude(oi => oi.Product).
                OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders.
                Include(o => o.Customer).
                ThenInclude(s => s.CountryName).
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
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == o.Id);

            if (existingOrder == null)
                throw new Exception("Sipariş bulunamadı!");

            // Order bilgileri
            existingOrder.CustomerId = o.CustomerId;
            existingOrder.OrderDate = o.OrderDate;
            existingOrder.ShippingFee = o.ShippingFee;
            existingOrder.AdjustmentPercentage = o.AdjustmentPercentage;
            existingOrder.AdjustmentType = o.AdjustmentType;

            if (existingOrder.Customer != null && o.Customer != null)
            {
                existingOrder.Customer.Address = o.Customer.Address;
                existingOrder.Customer.DeliveryAddress = o.Customer.DeliveryAddress;
            }

            // Silinecek OrderItem'ları sadece Id'leri 0'dan farklı olanlar üzerinden sil
            var itemsToRemove = existingOrder.OrderItems.Where(x => x.Id > 0).ToList();
            _context.OrderItems.RemoveRange(itemsToRemove);

            // OrderItem'ları sıfırla ve yeni listeyi ekle
            existingOrder.OrderItems = o.OrderItems.Select(x => new OrderItem
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                CustomProductName = x.CustomProductName
            }).ToList();

            await _context.SaveChangesAsync();
        }


        public async Task<List<Order>> SearchAsync(string? keyword)
        {
            var query = _context.Orders
        .Include(o => o.Customer).ThenInclude(c => c.CountryName)
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

        public async Task<List<Order>> GetPagedOrderAsync(int page, int pageSize)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public IQueryable<Order> Query()
        {
            return _context.Orders.AsQueryable();
        }

    }
}
