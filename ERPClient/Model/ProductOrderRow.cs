using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPClient.Model
{
    public class ProductOrderRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
        public string? CustomProductName { get; set; }

        //Açıklama: Burası sadece ve sadece YurtDisi ve YurtIci siparişlerinde ekle butonuna basıldığında referans alınacak olan model sınıfıdır.
    }
}
