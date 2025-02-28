using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class Product
    {
            public int Id { get; set; }

            [NotMapped]
            public string MakineAdi { get; set; } = string.Empty;
            public string? Sinif { get; set; }
            public string? ParcaAdi { get; set; }
            public decimal FiyatTL { get; set; }
            public decimal FiyatEUR { get; set; }

            public List<ProductMachine> ProductMachines { get; set; } = new List<ProductMachine>(); // Many-to-Many ilişki
        }
    }
