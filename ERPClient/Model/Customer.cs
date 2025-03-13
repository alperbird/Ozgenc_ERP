using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPClient.Model
{
    public class Customer
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? TaxNo { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;

    }
}
