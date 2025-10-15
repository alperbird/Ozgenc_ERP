using EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Entities.Enums;
using Microsoft.IdentityModel.Tokens;
using Azure.Core.Cryptography;

namespace DataAccessLayer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        

        public DbSet<Product> Products { get; set; }

        public DbSet<Machine> Machines { get; set; }

        public DbSet<ProductMachine> ProductMachines { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }

        public DbSet<F_GeneralPart> F_MalzemeAdlari { get; set; }
        public DbSet<F_Supplier> F_Tedarikci { get; set; }
        public DbSet<F_Warehouse> F_Depo { get; set; }
        public DbSet<GeneralStock> F_StokGiris { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductMachine>()
                .HasKey(pm => pm.Id); //ID'yi PRIMARY KEY yap

            modelBuilder.Entity<ProductMachine>()
                .Property(pm => pm.Id)
                .ValueGeneratedOnAdd()
                .HasAnnotation("SqlServer: Identity", "1,1"); //IDENTITY

            modelBuilder.Entity<ProductMachine>()
                .HasOne(pm => pm.Product)
                .WithMany(p => p.ProductMachines)
                .HasForeignKey(pm => pm.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductMachine>()
                .HasOne(pm => pm.Machine)
                .WithMany(m => m.ProductMachines)
                .HasForeignKey(pm => pm.MakineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .Property(p => p.FiyatTL)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.FiyatEUR)
                .HasColumnType("decimal(18,2)");
        }


    }
}
