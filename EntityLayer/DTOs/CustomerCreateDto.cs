using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class CustomerCreateDto
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? TaxNo { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
