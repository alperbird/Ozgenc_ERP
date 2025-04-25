using EntityLayer.Entities;
using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class OrderUpdateDto
    {
        public CustomerDto Customer;

        public int Id { get; set; }
        public int CustomerId { get; set; }

        public string Address { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }

        public decimal? ShippingFee { get; set; }

        public decimal? AdjustmentPercentage { get; set; }

        public PriceAdjustmentType AdjustmentType { get; set; }

        public List<OrderItemCreateDto> OrderItems { get; set; }
    }
}
