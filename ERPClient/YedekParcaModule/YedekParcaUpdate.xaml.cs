using EntityLayer.DTOs;
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
        private readonly string apiMachineUrl = "http://192.168.5.68:5001/api/Machine";

        public YedekParcaUpdate(Product product, HttpClient client)
        {
            _selectedProduct = product;
            _client = client;
            InitializeComponent();
            LoadMachines();
            LoadProductData(); // Seçili ürünü yükle
        }

        //Makine listele
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
                    var selectedMachineIds = _selectedProduct.ProductMachines.Select(pm => pm.MakineId).ToList(); 
                    if (selectedMachineIds.Contains(machine.Id))
                    {
                        lstMakine.SelectedItems.Add(machine);
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
                var selectedMachineIds = lstMakine.SelectedItems.Cast<Machine>().Select(m => m.Id).ToList();

                var updatedProduct = new ProductUpdateDto
                {
                    Id = _selectedProduct.Id,
                    Sinif = txtSinif.Text,
                    ParcaAdi = txtParcaAdi.Text,
                    FiyatTL = decimal.TryParse(txtTL.Text, out decimal fiyatTL) ? fiyatTL : 0,
                    FiyatEUR = decimal.TryParse(txtEUR.Text, out decimal fiyatEUR) ? fiyatEUR : 0,
                    MakineIdleri = selectedMachineIds
                };

                string jsonContent = System.Text.Json.JsonSerializer.Serialize(updatedProduct, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                MessageBox.Show($"Giden JSON:\n{jsonContent}", "Güncelleme JSON", MessageBoxButton.OK, MessageBoxImage.Information);

                var productContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage productResponse = await _client.PutAsync($"{apiUrl}/{_selectedProduct.Id}", productContent);

                string responseContent = await productResponse.Content.ReadAsStringAsync();

                if (productResponse.IsSuccessStatusCode)
                {
                    MessageBox.Show("Ürün başarıyla güncellendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    var mainWindow = Application.Current.Windows.OfType<YedekParca>().FirstOrDefault();
                    if (mainWindow != null)
                    {
                        mainWindow.LoadTable(); // **DataGrid Yenilensin**
                    }

                    Window.GetWindow(this)?.Close();
                }
                else
                {
                    MessageBox.Show($"Güncelleme başarısız! {productResponse.StatusCode}\n{responseContent}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme işlemi başarısız: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // Geri butonu (Kapat)
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private string GetMachineNameById(int id)
        {
            var machine = lstMakine.Items.Cast<Machine>().FirstOrDefault(m => m.Id == id);
            return machine?.MakineAdi ?? "Bilinmeyen Makine"; // **Eğer bulamazsa "Bilinmeyen Makine" döndür**
        }

    }

}
