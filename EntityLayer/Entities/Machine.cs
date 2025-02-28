using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class Machine
    {
        public int Id { get; set; }
        public string? MakineAdi { get; set; }

        public List<ProductMachine> ProductMachines { get; set; } = new List<ProductMachine>();// Many-to-Many ilişki
    }
}
