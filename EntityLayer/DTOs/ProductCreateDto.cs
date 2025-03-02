using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class ProductCreateDto //İster insert de, ister create de... POST işlemi burada gerçekleşecek.
    {
        public string Sinif { get; set; }
        public string ParcaAdi { get; set; }
        public decimal FiyatTL { get; set; }
        public decimal FiyatEUR { get; set; }

        public List<int> MakineIdleri { get; set; }
    }
}
