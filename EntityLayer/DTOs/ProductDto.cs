using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class ProductDto
    {// GET işlemimizi yapabilmek adına ProductDto sınıfımızı oluşturduk.
        public int Id { get; set; }
        public string? Sinif { get; set; }
        public string? ResimYolu { get; set; }
        public byte[]? ResimData { get; set; }
        public string? ParcaAdi { get; set; }
        public string? ParcaIngilizce { get; set; }
        public decimal FiyatTL { get; set; }
        public decimal FiyatEUR { get; set; }
        public string? Aciklama { get; set; }

        public int Stok { get; set; }

        public string MakineAdi { get; set; }

        public List<ProductMachineDto>? ProductMachines { get; set; }
    }
}
