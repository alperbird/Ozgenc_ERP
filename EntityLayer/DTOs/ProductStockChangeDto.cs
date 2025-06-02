using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class ProductStockChangeDto
    {
        public int Id { get; set; }
        public int QuantityChange { get; set; }
    }
}
