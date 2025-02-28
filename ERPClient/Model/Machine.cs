using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ERPClient.Model
{
    public class Machine
    {
        public int Id { get; set; }
        public string? MakineAdi { get; set; }

        [JsonIgnore]
        public List<ProductMachine>? ProductMachines { get; set; }
    }
}
