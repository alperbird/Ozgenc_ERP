using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class OrderListDto
    {
        public int Id { get; set; }

        public Customer Customer { get; set; }
        public string? CustomerName { get; set; }
        public string? Address { get; set; }
        public string? DeliveryAddress { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; } 
    }
}
