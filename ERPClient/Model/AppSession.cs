using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Entities;

namespace ERPClient.Model
{
    public static class AppSession
    {
        public static User? CurrentUser { get; set; } 
    }
}
