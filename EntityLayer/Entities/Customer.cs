using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        public string CustomerName { get; set; } = string.Empty;


        public string Address { get; set; } = string.Empty;


        public string DeliveryAddress { get; set; } = string.Empty;


        public string Contact { get; set; } = string.Empty;


        public string Email { get; set; } = string.Empty;


        public string PhoneNumber { get; set; } = string.Empty;


        public DateTime Date { get; set; }

        public string? TaxNo { get; set; }
    }
}
