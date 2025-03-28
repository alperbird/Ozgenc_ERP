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

    public App()
    {
        client.BaseAddress = new Uri("http://localhost:7285/api/Product/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
            );
    }

}

