using EntityLayer.Entities.Enums;
using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public Customer Customer { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? ShippingFee { get; set; }
        public decimal? AdjustmentPercentage { get; set; }
        public PriceAdjustmentType? AdjustmentType { get; set; }

        public List<OrderItemDto> OrderItems { get; set; }
    }
}
