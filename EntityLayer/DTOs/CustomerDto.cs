using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public string Address { get; set; }
        public string DeliveryAddress { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string TaxNo { get; set; }
        public DateTime Date { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
