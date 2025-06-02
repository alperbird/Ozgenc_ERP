using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class PdfOrderItemDto
    {
        public string ParcaAdi { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
        public bool Kdv { get; set; }

        public string? SelectedMachine { get; set; }
    }
}
