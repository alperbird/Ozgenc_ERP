using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class ProductMachineDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int MakineId { get; set; }

        public MachineDto? Machine { get; set; }
    }
}
