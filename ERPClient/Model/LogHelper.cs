using DataAccessLayer.Data;
using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPClient.Model
{
    public static class LogHelper
    {
        public static void LogAction(int userId, string info)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer("Server=192.168.5.81,1433;Database=Ozgenc_ERP;User Id=sa;Password=ozgencmachine16;TrustServerCertificate=True;")
                    .Options;
            using (var context = new AppDbContext(options))
            {
                context.Logs.Add(new Log
                {
                    UserId = userId,
                    Information = info,
                    Date = DateTime.Now
                });

                context.SaveChanges();
            }
        }
    }
}
