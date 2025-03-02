using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ERPClient.Model;
using System.Text.Json;
using System.Windows.Navigation;
using ERPClient.YedekParcaModule;
using System.IO;
using EntityLayer.DTOs;
using Newtonsoft.Json;

namespace ERPClient
{
    /// <summary>
    /// Interaction logic for YedekParca.xaml
    /// </summary>
    public partial class YedekParca : Window
    {
        private readonly HttpClient _client = new HttpClient();
        private const string apiUrl = "http://192.168.5.68:5001/api/Product";
        private const string apiUrlMachine = "http://192.168.5.68:5001/api/Machine";
        private List<Product> tumParcalar = new List<Product>(); // Tüm verileri saklamak için
        private Product? _selectedProduct;

        public YedekParca()
        {
            InitializeComponent();
            LoadTable();
            LoadMachines(); // Makine listesini yükle
        }

        // Tüm ürünleri getir
        public async void LoadTable()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                // Newtonsoft.Json kullanarak deserialize et!
                List<ProductDto> yedekparca = JsonConvert.DeserializeObject<List<ProductDto>>(responseBody) ?? new List<ProductDto>();

                // WPF DataGrid'e bağla
                dgYedekParca.ItemsSource = yedekparca;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Veri yüklenirken hata oluştu: {ex.Message}");
            }
            catch (System.Text.Json.JsonException ex)
            {
                MessageBox.Show($"JSON Çözümleme Hatası: {ex.Message}");
            }
        }





        // Makine listesini API’den çek
        private async void LoadMachines()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(apiUrlMachine);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                //JSON NULL mı?
                if (string.IsNullOrWhiteSpace(responseBody) || responseBody == "null")
                {
                    MessageBox.Show("Gelen JSON NULL veya boş!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //JSON'u deserialize et
                var machines = System.Text.Json.JsonSerializer.Deserialize<List<Machine>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                }) ?? new List<Machine>();

                //Eğer `machines` NULL dönerse, hatayı ekrana yaz
                if (machines == null)
                {
                    MessageBox.Show("JSON NULL olarak deserialize edildi! JSON formatını kontrol et.");
                    return;
                }

                if (machines.Count == 0)
                {
                    MessageBox.Show("API'den gelen JSON listesi BOŞ!");
                    return;
                }

                lstMakine.ItemsSource = machines; //Listeyi ListBox’a bağlıyoruz
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Makine listesi yüklenirken hata oluştu: {ex.Message}");
            }
        }

        // Yeni ürün ekleme
        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedMachineIds = lstMakine.SelectedItems.Cast<Machine>().Select(m => m.Id).ToList();

                var productDto = new ProductCreateDto
                {
                    Sinif = txtSinif.Text,
                    ParcaAdi = txtParcaAdi.Text,
                    FiyatTL = decimal.TryParse(txtTL.Text, out decimal fiyatTL) ? fiyatTL : 0,
                    FiyatEUR = decimal.TryParse(txtEUR.Text, out decimal fiyatEUR) ? fiyatEUR : 0,
                    MakineIdleri = selectedMachineIds
                };

                string jsonContent = System.Text.Json.JsonSerializer.Serialize(productDto, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                //MessageBox.Show($"Giden JSON:\n{jsonContent}");

                var productContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage productResponse = await _client.PostAsync(apiUrl, productContent);
                productResponse.EnsureSuccessStatusCode();

                MessageBox.Show("Ürün ve makineler başarıyla kaydedildi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTable();
                CleanForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ekleme işlemi başarısız: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        // Ürün Güncelleme
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgYedekParca.SelectedItem is Product selectedProduct)
            {
                var updateWindow = new YedekParcaUpdateWindow(selectedProduct, _client);
                updateWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir ürün seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Ürün Silme
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Tag is ProductDto selectedProductDto) // DTO geliyor, Product değil!
                {
                    var result = MessageBox.Show(
                        $"Ürün '{selectedProductDto.ParcaAdi}' ve ilişkili makineler silinecek! Emin misin?",
                        "Silme Onayı",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        HttpResponseMessage response = await _client.DeleteAsync($"{apiUrl}/{selectedProductDto.Id}");

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Ürün başarıyla silindi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadTable(); // Tabloyu güncelle
                        }
                        else
                        {
                            string errorMessage = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Silme başarısız: {errorMessage}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Tag içerisinde ProductDto nesnesi yok, DataGrid’e atanmış mı kontrol et!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }




        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        // UI Temizleme
        private void CleanForm()
        {
            lstMakine.SelectedItems.Clear();
            txtSinif.Clear();
            txtParcaAdi.Clear();
            txtTL.Clear();
            txtEUR.Clear();
        }

        #region Arama Çubuğu ve Filtreleme işlemleri
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            FilterProducts();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearch_Click(sender, e);
            }
        }

        // Filtreleme Metodu
        private async void FilterProducts()
        {
            string searchText = txtSearch.Text.Trim();

            try
            {
                string requestUrl = string.IsNullOrWhiteSpace(searchText)
                    ? $"{apiUrl}"  // Boşsa tüm ürünleri çek
                    : $"{apiUrl}/search?query={Uri.EscapeDataString(searchText)}"; // API’ye arama isteği gönder

                HttpResponseMessage response = await _client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var filteredProducts = System.Text.Json.JsonSerializer.Deserialize<List<Product>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Product>();

                dgYedekParca.ItemsSource = filteredProducts; // DataGrid’i güncelle
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Arama yapılırken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        // DataGrid Seçim Değiştiğinde
        private void dgYedekParca_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgYedekParca.SelectedItem is Product selected)
            {
                _selectedProduct = selected;
                Console.WriteLine($"Seçili Ürün: {_selectedProduct.ParcaAdi} (ID: {_selectedProduct.Id})");
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }
    }

}
