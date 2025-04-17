using BusinessLayer.Interfaces;
using DataAccessLayer.Data;
using EntityLayer.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;

namespace WebClient.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProformaService _service;

        public OrderController(AppDbContext context, IProformaService service)
        {
            _context = context;
            _service = service;
        }
        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null, int page = 1)
        {
            int pageSize = 10;

            var ordersQuery = _context.Orders
                .Include(o => o.Customer)
                .AsQueryable();

            if (startDate != null && endDate != null)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);
            }

            var totalOrders = await ordersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var orderListDtos = orders.Select(o => new OrderListDto
            {
                Id = o.Id,
                Customer = o.Customer,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice
            }).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.StartDate = startDate?.ToString("dd-MM-yyyy");
            ViewBag.EndDate = endDate?.ToString("dd-MM-yyyy");
            return View(orderListDtos);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.Country)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var dto = new OrderDetailDto
            {
                Id = order.Id,
                Customer = order.Customer,
                OrderDate = order.OrderDate,
                ShippingFee = order.ShippingFee,
                AdjustmentPercentage = order.AdjustmentPercentage,
                AdjustmentType = order.AdjustmentType,
                TotalPrice = order.TotalPrice,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Product = oi.Product,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateProforma(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer).ThenInclude(c => c.Country)
                .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var filePath = await _service.GenerateProformaAsync(order);
            return File(System.IO.File.ReadAllBytes(filePath), "application/pdf", Path.GetFileName(filePath));
        }

    }
}
