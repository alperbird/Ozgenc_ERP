using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPClient.Model
{
    public class ProductMachine
    {
        public int Id { get; set; }  // 🔥 Ara tablonun Primary Key'i

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int MakineId { get; set; }
        public Machine? Machine { get; set; }
    }
}
