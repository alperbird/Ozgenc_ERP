using EntityLayer.Entities;
using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class OrderCreateDto
    {
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }

        public decimal? ShippingFee { get; set; }

        public decimal? AdjustmentPercentage { get; set; }

        public PriceAdjustmentType AdjustmentType { get; set; }

        public List<OrderItemDto> OrderItems { get; set; }
    }
}
