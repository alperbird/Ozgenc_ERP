using BusinessLayer.Interfaces;
using EntityLayer.DTOs;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrderAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderBtIdAsync(id);
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto o)
        {
            var order = new Order
            {
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                ShippingFee = o.ShippingFee,
                AdjustmentPercentage = o.AdjustmentPercentage,
                AdjustmentType = o.AdjustmentType,
                OrderItems = o.OrderItems.Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }).ToList()
            };

            await _orderService.AddOrderAsync(order);
            return Ok(order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderUpdateDto o)
        {
            if (id != o.Id)
            {
                return BadRequest("Sipariş Id uyuşmuyor!"); //400 döndürecek
            }

            var existingOrder = await _orderService.GetOrderBtIdAsync(id);

            if(existingOrder == null)
            {
                return NotFound("Sipariş bulunamadı!"); //404 döndürecek
            }

            existingOrder.CustomerId = o.CustomerId;
            existingOrder.OrderDate = o.OrderDate;
            existingOrder.ShippingFee = o.ShippingFee;
            existingOrder.AdjustmentPercentage = o.AdjustmentPercentage;
            existingOrder.AdjustmentType = o.AdjustmentType;

            // OrderItems eski verileri silip yenilerini ekleyeceğiz...

            existingOrder.OrderItems.Clear();
            existingOrder.OrderItems = o.OrderItems.Select(x => new OrderItem
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice
            }).ToList();

            await _orderService.UpdateOrderAsync(existingOrder);
            return Ok(existingOrder);
        }
    }
}
