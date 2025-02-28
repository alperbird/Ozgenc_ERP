using ERPClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ERPClient.YedekParcaModule
{
    /// <summary>
    /// Interaction logic for YedekParcaUpdate.xaml
    /// </summary>
    public partial class YedekParcaUpdate : Page
    {
        private Product _selectedProduct;
        private readonly HttpClient _client;
        private readonly string apiUrl = "http://192.168.5.68:5001/api/Product";
        private readonly string apiMachineUrl = "http://192.168.5.68:5001/api/Machine"; // Makine listesi için API

        public YedekParcaUpdate(Product product, HttpClient client)
        {
            _selectedProduct = product;
            _client = client;
            InitializeComponent();
            LoadMachines(); // Makineleri yükle
            LoadProductData(); // Seçili ürünü yükle
        }

        // ✅ Makine listesini API'den çek
        private async void LoadMachines()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(apiMachineUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                List<Machine> machines = JsonSerializer.Deserialize<List<Machine>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                }) ?? new List<Machine>();

                lstMakine.ItemsSource = machines;

                // 📌 **Seçili makineleri işaretle**
                foreach (var machine in machines)
                {
                    var selectedMachineIds = _selectedProduct.ProductMachines.Select(pm => pm.MakineId).ToList(); // Many-to-Many kullan
                    if (selectedMachineIds.Contains(machine.Id))
                    {
                        lstMakine.SelectedItems.Add(machine); // Çoklu seçim yapıyoruz
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Makine listesi yüklenirken hata oluştu: {ex.Message}");
            }
        }

        // Ürün bilgilerini yükle
        private void LoadProductData()
        {
            txtSinif.Text = _selectedProduct.Sinif;
            txtParcaAdi.Text = _selectedProduct.ParcaAdi;
            txtTL.Text = _selectedProduct.FiyatTL.ToString();
            txtEUR.Text = _selectedProduct.FiyatEUR.ToString();
        }

        // Ürün Güncelleme
        private async void btnUpdateSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Çoklu makine seçimi al
                var selectedMachineIds = lstMakine.SelectedItems.Cast<Machine>().Select(m => m.Id).ToList();

                if (selectedMachineIds.Count == 0)
                {
                    MessageBox.Show("Lütfen en az bir makine seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _selectedProduct.ProductMachines = selectedMachineIds
                    .Select(id => new ProductMachine { MakineId = id, ProductId = _selectedProduct.Id })
                    .ToList();

                _selectedProduct.Sinif = txtSinif.Text;
                _selectedProduct.ParcaAdi = txtParcaAdi.Text;
                _selectedProduct.FiyatTL = decimal.Parse(txtTL.Text);
                _selectedProduct.FiyatEUR = decimal.Parse(txtEUR.Text);

                var jsonContent = new StringContent(JsonSerializer.Serialize(_selectedProduct), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PutAsync($"{apiUrl}/{_selectedProduct.Id}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Ürün başarıyla güncellendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                    // **Ana sayfadaki tabloyu yenile**
                    var mainWindow = Application.Current.Windows.OfType<YedekParca>().FirstOrDefault();
                    if (mainWindow != null)
                    {
                        mainWindow.LoadTable(); // DataGrid Yenile
                    }

                    // **Açılan pencereyi kapat**
                    Window.GetWindow(this)?.Close();
                }
                else
                {
                    MessageBox.Show($"Güncelleme başarısız! {response.StatusCode}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Geri butonu (Kapat)
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }

}
