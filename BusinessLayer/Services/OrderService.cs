using BusinessLayer.Interfaces;
using DataAccessLayer.Interfaces;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using EntityLayer.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<Order>> GetAllOrderAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task<Order> GetOrderBtIdAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        public async Task AddOrderAsync(Order o)
        {
            o.TotalPrice = CalculateTotalPrice(o);
            await _orderRepository.AddAsync(o);
        }

        public async Task UpdateOrderAsync(Order o)
        {
            o.TotalPrice = CalculateTotalPrice(o);
            await _orderRepository.UpdateAsync(o);
        }

        private decimal CalculateTotalPrice(Order o)
        {
            decimal itemsTotal = o.OrderItems.Sum(x => x.UnitPrice * x.Quantity); //birim fiyat + adet hesaplaması
            decimal adjustment = 0;

            if(o.AdjustmentType == PriceAdjustmentType.Discount)
            {
                adjustment = itemsTotal * (o.AdjustmentPercentage ?? 0) / 100 * -1; //indirim yüzdesi hesaplaması
            }

            else if (o.AdjustmentType == PriceAdjustmentType.Increase)
            {
                adjustment = itemsTotal * (o.AdjustmentPercentage ?? 0) / 100; //zam yüzdesi hesaplaması
            }

            decimal shipping = o.ShippingFee ?? 0; //kargo ücretinin olduğu kısım. Null gelirse değeri sıfır olarak ata.

            return itemsTotal + adjustment + shipping;
        }

        public async Task<List<Order>> SearchOrderAsync(string? keyword)
        {
            return await _orderRepository.SearchAsync(keyword);
        }

        public async Task<PagedResult<Order>> GetPagedOrderAsync(int page, int pageSize)
        {
            var query = _orderRepository.Query()
        .Include(o => o.Customer)
        .OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync(); // toplam kayıt sayısı

            var pagedOrders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>
            {
                Items = pagedOrders,
                TotalCount = totalCount
            };
        }

        Task<PagedResult<Order>> IOrderService.GetPagedOrderAsync(int page, int pageSize)
        {
            return GetPagedOrderAsync(page, pageSize);
        }
    }
}
