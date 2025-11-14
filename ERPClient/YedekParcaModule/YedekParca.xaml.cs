using System.Text;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ERPClient.Model;
using System.Text.Json;
using ERPClient.YedekParcaModule;
using System.IO;
using EntityLayer.DTOs;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Path = System.IO.Path;
using OfficeOpenXml;
using Microsoft.Win32;
using User = EntityLayer.Entities.User;

namespace ERPClient
{

    /// <summary>
    /// Interaction logic for YedekParca.xaml
    /// </summary>
    public partial class YedekParca : Window
    {
        private readonly HttpClient _client = new HttpClient();
        private const string apiUrl = "http://192.168.5.81:5001/api/Product";
        private const string apiUrlMachine = "http://192.168.5.81:5001/api/Machine";
        private List<Product> tumParcalar = new List<Product>(); // Tüm verileri saklamak için
        private Product? _selectedProduct;
        private string? selectedImagePath;
        private byte[]? selectedImageData;
        private readonly User _currentUser;

        private ObservableCollection<Product> allProducts = new ObservableCollection<Product>(); // Tüm veriler burada tutulacak
        private ObservableCollection<Product> paginatedProducts = new ObservableCollection<Product>(); // DataGrid'e bağlanacak veri
        private int currentPage = 1;
        private int pageSize = 10;

        public YedekParca(User user)
        {
            InitializeComponent();
            LoadTable();
            LoadMachines(); // Makine listesini yükle
            LoadSinif();
            _currentUser = user;
        }

        // Tüm ürünleri getir
        public async void LoadTable()
        {
            try
            {
                // Önce Yedek Parça listesini API'den çekiyoruz
                HttpResponseMessage response = await _client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                List<ProductDto> yedekparcaDtos = JsonConvert.DeserializeObject<List<ProductDto>>(responseBody) ?? new List<ProductDto>();

                // Şimdi makine listesini API'den çekiyoruz
                HttpResponseMessage machineResponse = await _client.GetAsync(apiUrlMachine);
                machineResponse.EnsureSuccessStatusCode();

                string machineResponseBody = await machineResponse.Content.ReadAsStringAsync();
                List<Machine> machines = JsonConvert.DeserializeObject<List<Machine>>(machineResponseBody) ?? new List<Machine>();

                // MakineId -> MakineAdi eşleştirmesi için Dictionary oluştur
                var machineDict = machines.ToDictionary(m => m.Id, m => m.MakineAdi);

                // DTO -> Model dönüşümü (Makine isimlerini de ekliyoruz)
                allProducts = new ObservableCollection<Product>(yedekparcaDtos.Select(dto => new Product
                {
                    Id = dto.Id,
                    Sinif = dto.Sinif,
                    ResimYolu = dto.ResimYolu,
                    ParcaAdi = dto.ParcaAdi,
                    FiyatTL = dto.FiyatTL,
                    FiyatEUR = dto.FiyatEUR,
                    Aciklama = dto.Aciklama,
                    ProductMachines = dto.ProductMachines.Select(pmDto => new ProductMachine
                    {
                        Id = pmDto.Id,
                        ProductId = pmDto.ProductId,
                        MakineId = pmDto.MakineId
                    }).ToList(), // DTO'dan Model'e çeviriyoruz

                    // Makine ID'leri üzerinden isimleri dictionary'den al
                    MakineAdi = string.Join(", ", dto.ProductMachines
                        .Select(pm => machineDict.TryGetValue(pm.MakineId, out var makineAdi) ? makineAdi : "Bilinmeyen Makine"))
                }));

                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yüklenirken hata oluştu: {ex.Message}");
            }
        }


        // Makine listeleme
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
                    Sinif = cmbSinif.SelectedItem.ToString(),
                    ParcaAdi = txtParcaAdi.Text,
                    ParcaIngilizce = txtEnglishProduct.Text,
                    FiyatTL = decimal.TryParse(txtTL.Text, out decimal fiyatTL) ? fiyatTL : 0,
                    FiyatEUR = decimal.TryParse(txtEUR.Text, out decimal fiyatEUR) ? fiyatEUR : 0,
                    Aciklama = txtAciklama.Text,
                    ResimYolu = selectedImagePath != null ? Path.GetFileName(selectedImagePath) : "yedekparcafoto",
                    ResimData = selectedImageData,
                    MakineIdleri = selectedMachineIds
                };

                string jsonContent = System.Text.Json.JsonSerializer.Serialize(productDto, new JsonSerializerOptions
                {
                    WriteIndented = true
                });


                var productContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage productResponse = await _client.PostAsync(apiUrl, productContent);
                productResponse.EnsureSuccessStatusCode();

                MessageBox.Show("Yedek parça başarıyla kaydedildi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} {txtParcaAdi.Text} adında yeni bir yedek parça ekledi. ");
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
                LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} {selectedProduct.ParcaAdi} adlı yedek parçanın güncelleme sayfasını açtı.");
                var updateWindow = new YedekParcaUpdateWindow(selectedProduct, _client, _currentUser);
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
                if (btn.Tag is Product selectedProduct) // DTO yerine Product modeli kullanıyoruz
                {
                    var result = MessageBox.Show(
                        $"'{selectedProduct.ParcaAdi}' adlı yedek parça silinecek! Emin misin?",
                        "Silme Onayı",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        HttpResponseMessage response = await _client.DeleteAsync($"{apiUrl}/{selectedProduct.Id}");

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Yedek parça başarıyla silindi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} {selectedProduct.ParcaAdi} adlı yedek parçayı sildi. ");
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
                    MessageBox.Show("Böyle bir yedek parça yok!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }


        //Programı kapat
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Yedek Parça sayfasından çıktı.");
            this.Close();
        }

        // UI Temizleme
        private void CleanForm()
        {
            lstMakine.SelectedItems.Clear();
            cmbSinif.SelectedItem = null;
            txtParcaAdi.Clear();
            txtEnglishProduct.Clear();
            txtTL.Clear();
            txtEUR.Clear();
            txtAciklama.Clear();
            selectedImagePath = null;
            imgPreview.Source = null; 
            txtFileName.Text = "Dosya seçilmedi";
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
                var filteredProducts = JsonConvert.DeserializeObject<List<Product>>(responseBody, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                }) ?? new List<Product>();


                // Eğer arama sonucu geldiyse pagination'ı sıfırla
                allProducts = new ObservableCollection<Product>(filteredProducts);
                currentPage = 1; // Sayfayı başa al

                // Eğer 10’dan fazla sonuç varsa otomatik 2. sayfaya geç
                if (allProducts.Count > pageSize)
                {
                    currentPage = 1;
                }

                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Arama yapılırken hata oluştu: {ex.Message}");
            }
        }

        #endregion

        // DataGrid Seçim Değiştiğinde
        private void dgYedekParca_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgYedekParca.SelectedItem is Product selectedProduct)
            {
                _selectedProduct = allProducts.FirstOrDefault(p => p.Id == selectedProduct.Id);

                if (_selectedProduct != null)
                {
                    Console.WriteLine($"Seçili Ürün: {_selectedProduct.ParcaAdi} (ID: {_selectedProduct.Id})");
                }
            }
        }


        //Selecbox içerisinde mouse kaydırma işlemi
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }

        #region Pagination İşlemleri

        private void UpdatePagination()
        {
            if (allProducts.Count == 0) return; // Eğer hiç veri yoksa pagination çalıştırma

            int totalItems = allProducts.Count;
            int totalPages = (totalItems + pageSize - 1) / pageSize;
            int skip = (currentPage - 1) * pageSize;

            paginatedProducts.Clear();
            foreach (var item in allProducts.Skip(skip).Take(pageSize))
            {
                paginatedProducts.Add(item);
            }

            dgYedekParca.ItemsSource = paginatedProducts;
            txtPageInfo.Text = $"Sayfa {currentPage} / {totalPages}";

            btnPrevious.IsEnabled = currentPage > 1;
            btnNext.IsEnabled = currentPage < totalPages;
        }


        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdatePagination();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage * pageSize) < allProducts.Count)
            {
                currentPage++;
                UpdatePagination();
            }
        }
        #endregion

        // Sınıf değerlerini artık comboboxa atadık. textblock yerine combobox kullanıyoruz.
        private void cmbSinif_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //Parça sınıflandırması combobox doldurma
        private void LoadSinif()
        {
            List<string> Sinif = new List<string>
            {
                "-",
               "MEKANİK",
               "ELEKTRİK",
               "PNÖMATİK",
               "GENEL"
            };
            cmbSinif.ItemsSource = Sinif;
        }


        #region Datagrid içinde bulunan yedek parça fotoğraf gösterme işlemleri
        private void Photo_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image img && img.DataContext is Product selectedProduct)
            {
                Console.WriteLine($"Tıklanan ürün ID: {selectedProduct.Id}, ResimData Null mu: {selectedProduct.ResimData == null}, Uzunluk: {selectedProduct.ResimData?.Length}");

                if (selectedProduct.ResimData == null || selectedProduct.ResimData.Length == 0)
                {
                    MessageBox.Show("Bu ürüne ait fotoğraf bulunmamaktadır.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }


                // Yeni bir pencere aç ve fotoğrafı göster
                ShowImageWindow(selectedProduct.ResimData);
            }
        }

        private void ShowImageWindow(byte[] imagePath)
        {
            Window imageWindow = new Window
            {
                Title = "Ürün Fotoğrafı",
                Width = 600,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imagePath))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = mem;
                image.EndInit();
            }

            Image img = new Image
            {
                Source = image,
                Stretch = Stretch.Uniform
            };

            imageWindow.Content = img;
            imageWindow.ShowDialog();
        }

        #endregion

        #region Fotoğraf Yükleme işlemi
        private void btnSelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Bir Fotoğraf Seçin"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedPath = openFileDialog.FileName;
                selectedImageData = File.ReadAllBytes(selectedPath);
                

                if (_selectedProduct == null)
                {
                    _selectedProduct = new Product(); // eğer selectedProduct null gelirse, yeni bir product nesnesi oluştur karşim. Çok yarrak gibi bir bypass oldu ama şimdilik dursun...
                }

                _selectedProduct.ResimYolu = Path.GetFileName(selectedPath);
                _selectedProduct.ResimData = selectedImageData;

                txtFileName.Text = Path.GetFileName(selectedPath);
                txtFileName.Foreground = Brushes.Black;
                txtFileName.FontStyle = FontStyles.Normal;

                // Önizleme (görselini göster)
                var image = new BitmapImage();
                using (var mem = new MemoryStream(selectedImageData))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                imgPreview.Source = image;
            }
        }
        #endregion


/*        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            // EPPlus lisans
            ExcelPackage.License.SetNonCommercialOrganization("abc.");

            if (allProducts == null || allProducts.Count == 0)
            {
                MessageBox.Show("Excel'e aktarılacak veri bulunamadı.", "Bilgi",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            //snapshot alıyoruz
            var products = allProducts.ToList();

            var sfd = new SaveFileDialog
            {
                Filter = "Excel Dosyası|*.xlsx",
                FileName = $"YedekParcaListesi_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            };

            if (sfd.ShowDialog() != true)
                return;

            try
            {
                using (var package = new ExcelPackage())
                {
                    var sheet = package.Workbook.Worksheets.Add("YedekParcalar");

                    // Başlıklar
                    sheet.Cells[2, 1].Value = "ID";         // A2
                    sheet.Cells[2, 2].Value = "MAKİNE";     // B2
                    sheet.Cells[2, 3].Value = "SINIF";      // C2
                    sheet.Cells[2, 4].Value = "PARÇA ADI";  // D2
                    sheet.Cells[2, 5].Value = "FİYAT (TL)"; // E2
                    sheet.Cells[2, 6].Value = "FİYAT (EUR)";// F2
                    sheet.Cells[2, 7].Value = "NOTLAR";     // G2 (AÇIKLAMA)

                    var row = 3; // Veriler

                    foreach (var p in products)
                    {
                        sheet.Cells[row, 1].Value = p.Id;
                        sheet.Cells[row, 2].Value = p.MakineAdlari; 
                        sheet.Cells[row, 3].Value = p.Sinif;
                        sheet.Cells[row, 4].Value = p.ParcaAdi;
                        sheet.Cells[row, 5].Value = p.FiyatTL;
                        sheet.Cells[row, 6].Value = p.FiyatEUR;
                        sheet.Cells[row, 7].Value = p.Aciklama;
                        row++;
                    }

                    // Kolon genişliklerini otomatik ayarla
                    if (sheet.Dimension != null)
                        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    File.WriteAllBytes(sfd.FileName, bytes);
                }

                MessageBox.Show("Excel dosyası başarıyla oluşturuldu.", "Başarılı",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel'e aktarılırken hata oluştu:\n" + ex.Message,
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
*/
    }


}
