using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class ProductStockDto
    {
        public int Id { get; set; }
        public string ParcaAdi { get; set; } = string.Empty;
        public int Stok { get; set; }
    }
}
