using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class GeneralStock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int GeneralPartId { get; set; }
        [ForeignKey("GeneralPartId")]
        public F_GeneralPart? GeneralPart { get; set; }

        public int SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public F_Supplier? Supplier { get; set; }

        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public F_Warehouse? Warehose { get; set; }

        public DateTime OrderDate { get; set; } //sipariş tarihi
        public DateTime DeliveryDate { get; set; } // siparişin giriş tarihi
        public int OrderQuantity { get; set; } //sipariş adedi
        public int ReceivedQuantity { get; set; } //gelen adet
    }
}
