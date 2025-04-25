using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using DataAccessLayer.Data;
using DataAccessLayer.Repositories;
using EntityLayer.DTOs;
using EntityLayer.Entities.Enums;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;
using DataAccessLayer.Interfaces;

namespace WebClient.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProformaService _service;
        private readonly IOrderRepository _repo;

        public OrderController(AppDbContext context, IProformaService service, IOrderRepository repo)
        {
            _context = context;
            _service = service;
            _repo = repo;
        }

        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null, int page = 1) //listeleme + pagination
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

        public async Task<IActionResult> Detail(int id) //detaylar sayfası
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.CountryName)
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
        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await _context.Products
                .OrderBy(p => p.Id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ParcaAdi = p.ParcaAdi,
                    FiyatEUR = p.FiyatEUR
                }).ToListAsync();

            ViewBag.Customers = await _context.Customers
                .Include(c => c.CountryName)
                .OrderBy(c => c.Id)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    CustomerName = c.CustomerName,
                    CountryId = c.CountryId,
                    CountryName = c.CountryName.CountryName, // Burası önemli
                    Address = c.Address,
                    DeliveryAddress = c.DeliveryAddress,
                    Contact = c.Contact,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    TaxNo = c.TaxNo
                }).ToListAsync();

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto dto) //sipariş oluşturma
        {
            if (!ModelState.IsValid)
            {
                // Hatalıysa formu yeniden doldur (müşteri & ürün)
                ViewBag.Customers = await _context.Customers
                    .Include(c => c.CountryName)
                    .Select(c => new CustomerDto
                    {
                        Id = c.Id,
                        CustomerName = c.CustomerName,
                        CountryId = c.CountryId,
                        CountryName = c.CountryName.CountryName,
                        Address = c.Address,
                        DeliveryAddress = c.DeliveryAddress,
                        Contact = c.Contact,
                        Email = c.Email,
                        PhoneNumber = c.PhoneNumber,
                        TaxNo = c.TaxNo
                    }).ToListAsync();

                ViewBag.Products = await _context.Products
                    .Select(p => new 
                    {
                        Id = p.Id,
                        ParcaAdi = p.ParcaAdi,
                        UnitPrice = p.FiyatEUR
                    }).ToListAsync();

                return View(dto);
            }

            // Siparişi oluştur
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.Now,
                ShippingFee = dto.ShippingFee ?? 0,
                AdjustmentPercentage = dto.AdjustmentPercentage ?? 0,
                AdjustmentType = dto.AdjustmentType,
                OrderItems = dto.OrderItems
                    .Where(item => item.ProductId > 0) // 0 olanları ayıkla
                    .Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        CustomProductName = string.IsNullOrWhiteSpace(item.CustomProductName) ? null : item.CustomProductName.Trim()
                    }).ToList()
            };

            // Toplam fiyatı hesapla
            decimal total = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);

            if (order.AdjustmentType == PriceAdjustmentType.Discount) //indirim
                total -= (decimal)(total * order.AdjustmentPercentage / 100);
            else if (order.AdjustmentType == PriceAdjustmentType.Increase) //zam
                total += (decimal)(total * order.AdjustmentPercentage / 100);

            total += order.ShippingFee ?? 0;
            order.TotalPrice = total;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Sipariş başarıyla oluşturuldu.";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> GenerateProforma(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.CountryName)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var filePath = await _service.GenerateProformaAsync(order);

            if (!System.IO.File.Exists(filePath))
                return StatusCode(500, "PDF oluşturulamadı.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }



        //public async Task<IActionResult> Index(string? keyword)
        //{
        //    var orders = await _repo.SearchAsync(keyword);
        //    var orderListDtos = orders.Select(o => new OrderListDto
        //    {
        //        Id = o.Id,
        //        Customer = o.Customer,
        //        OrderDate = o.OrderDate,
        //        TotalPrice = o.TotalPrice
        //    }).ToList();

        //    return View(orderListDtos);
        //}

    }
}
