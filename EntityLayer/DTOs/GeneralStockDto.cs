using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class GeneralStockDto
    {
        public int Id { get; set; }
        public int GeneralPartId { get; set; }
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime OrderDate { get; set; } //sipariş tarihi
        public DateTime DeliveryDate { get; set; } //sipariş giriş tarihi
        public int OrderQuantity { get; set; } //sipariş adedi
        public int ReceivedQuantity { get; set; } //gelen sipariş adedi
    }
}
