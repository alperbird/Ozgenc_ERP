using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class OrderItemCreateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string? CustomProductName { get; set; } // parça adını değiştirilebilir yapmak için bunu ekledim.

        public string SelectedMachine { get; set; } // makine seçimi için ekledim.
    }
}
