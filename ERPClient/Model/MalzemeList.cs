using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPClient.Model
{
    public class MalzemeList
    {
        public string MalzemeAdi { get; set; } = string.Empty;
        public int TalepMiktari { get; set; }
        public string? TuketimZamani { get; set; }
        public string? IhtiyacTarihi { get; set; }
        public string? Not { get; set; }
    }
}
