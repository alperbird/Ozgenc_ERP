using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.DTOs
{
    public class PagedResult<T> //ben biraz eşşeğim ve order pagination değerlerini burada tutuyorum
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }

    }
}
