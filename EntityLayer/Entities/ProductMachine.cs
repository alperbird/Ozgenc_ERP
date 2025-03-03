using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class ProductMachine
    {
        public int Id { get; set; } //PRIMARY KEY ve IDENTITY olacak!

        public int ProductId { get; set; }
        [JsonIgnore]
        public Product? Product { get; set; }

        public int MakineId { get; set; }
        public Machine? Machine { get; set; }
    }

}
