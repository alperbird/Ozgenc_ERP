using BusinessLayer.Interfaces;
using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class ProformaService : IProformaService
    {
        public Task<string> GenerateProformaAsync(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
