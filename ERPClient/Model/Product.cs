using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ERPClient.Model
{
    public class Product
    {
        public int Id { get; set; }

        [NotMapped]
        public string MakineAdi { get; set; } = string.Empty;
        public string? Sinif { get; set; }
        public string? ResimYolu { get; set; }
        public string? ParcaAdi { get; set; }
        public decimal FiyatTL { get; set; }
        public decimal FiyatEUR { get; set; }
        [JsonIgnore]
        public List<ProductMachine>? ProductMachines { get; set; }
    }
}
