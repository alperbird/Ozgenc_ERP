using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using DataAccessLayer.Data;
using DataAccessLayer.Repositories;
using ERPClient.LoginRegister;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;

namespace ERPClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static readonly HttpClient client = new HttpClient();
    public static DbContextOptions<AppDbContext> DbOptions { get; private set; } 

    public static IStokService StokService { get; set; }
    public static IDepoService DepoService { get; set; }
    public static IParcalarService ParcalarService { get; set; }
    public static ITedarikciService TedarikciService { get; set; }

    public App()
    {
        client.BaseAddress = new Uri("http://localhost:7285/api/Product/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
            );


    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=192.168.5.81,1433;Database=OZGENC_ERP;User Id=sa;Password=ozgencmachine16;TrustServerCertificate=True;");
        DbOptions = optionsBuilder.Options;

        var stokRepo = new StockRepository(new AppDbContext(DbOptions));
        StokService = new GeneralStockService(stokRepo);

        var depoRepo = new WarehouseRepository(new AppDbContext(DbOptions));
        DepoService = new WarehouseService(depoRepo);

        var parcaRepo = new GeneralPartRepository(new AppDbContext(DbOptions));
        ParcalarService = new GeneralPartService(parcaRepo);

        var tedarikciRepo = new SupplierRepository(new AppDbContext(DbOptions));
        TedarikciService = new SupplierService(tedarikciRepo);

        var authWindow = new AuthWindow();
        authWindow.Show();
    }

}

