using EntityLayer.DTOs;
using ERPClient.Model;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using User = EntityLayer.Entities.User;

namespace ERPClient.YedekParcaModule
{
    /// <summary>
    /// Interaction logic for YedekParcaUpdate.xaml
    /// </summary>
    public partial class YedekParcaUpdate : Page
    {
        private readonly User _currentUser;
        private Product _selectedProduct;
        private readonly HttpClient _client;
        private readonly string apiUrl = "http://192.168.5.81:5001/api/Product";
        private readonly string apiMachineUrl = "http://192.168.5.81:5001/api/Machine";
        private string? selectedImagePathUpdate = null;
        private EntityLayer.Entities.Product selectedProduct;
        private User user;

        public YedekParcaUpdate(Product product, HttpClient client, User user)
        {
            _selectedProduct = product;
            _client = client;
            _currentUser = user;
            InitializeComponent();
            LoadMachines();
            LoadSinif();
            LoadProductData(); // Seçili ürünü yükle
            
        }

        public YedekParcaUpdate(EntityLayer.Entities.Product selectedProduct, HttpClient client, User user)
        {
            this.selectedProduct = selectedProduct;
            _client = client;
            this.user = user;
        }

        //Makine listele
        private async void LoadMachines()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(apiMachineUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                List<Machine> machines = System.Text.Json.JsonSerializer.Deserialize<List<Machine>>(responseBody, new JsonSerializerOptions
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

        private async Task<Dictionary<int, string>> GetMachineDictionaryAsync()
        {
            var response = await _client.GetAsync("http://192.168.5.81:5001/api/Machine");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            // Newtonsoft kullanıyoruz çünkü içinde $values var
            var root = JsonConvert.DeserializeObject<JObject>(responseBody);
            var values = root["$values"];

            if (values == null)
            {
                MessageBox.Show("Makine listesi çözümlenemedi!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Dictionary<int, string>();
            }

            var machines = values.ToObject<List<Machine>>() ?? new List<Machine>();
            return machines.ToDictionary(m => m.Id, m => m.MakineAdi);
        }

        // Ürün bilgilerini yükle
        private void LoadProductData()
        {
            if (cmbSinif.Items.Contains(_selectedProduct.Sinif))
            {
                cmbSinif.SelectedItem = _selectedProduct.Sinif;
            }
            txtParcaAdi.Text = _selectedProduct.ParcaAdi;
            txtTL.Text = _selectedProduct.FiyatTL.ToString();
            txtEUR.Text = _selectedProduct.FiyatEUR.ToString();
            txtAciklama.Text = _selectedProduct.Aciklama;

            if (!string.IsNullOrEmpty(_selectedProduct.ResimYolu))
            {
                txtPhotoNameUpdate.Text = Path.GetFileName(_selectedProduct.ResimYolu);
                selectedImagePathUpdate = _selectedProduct.ResimYolu;
            }
            else
            {
                txtPhotoNameUpdate.Text = "Seçili Fotoğraf Yok";
                selectedImagePathUpdate = null;
            }
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
                    Sinif = cmbSinif.SelectedItem.ToString(),
                    ParcaAdi = txtParcaAdi.Text,
                    FiyatTL = decimal.TryParse(txtTL.Text, out decimal fiyatTL) ? fiyatTL : 0,
                    FiyatEUR = decimal.TryParse(txtEUR.Text, out decimal fiyatEUR) ? fiyatEUR : 0,
                    Aciklama = txtAciklama.Text,
                    ResimYolu = selectedImagePathUpdate,
                    MakineIdleri = selectedMachineIds
                };
                await UpdateProduct(updatedProduct);
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(updatedProduct, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                //MessageBox.Show($"Giden JSON:\n{jsonContent}", "Güncelleme JSON", MessageBoxButton.OK, MessageBoxImage.Information);

                var productContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage productResponse = await _client.PutAsync($"{apiUrl}/{_selectedProduct.Id}", productContent);


                string responseContent = await productResponse.Content.ReadAsStringAsync();

                if (productResponse.IsSuccessStatusCode)
                {
                    MessageBox.Show("Yedek parça başarıyla güncellendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    var old = _selectedProduct;
                    StringBuilder sb = new();
                    sb.AppendLine($"{_currentUser.Name} {_currentUser.Surname}, '{old.ParcaAdi}' adlı yedek parçayı güncelledi:");

                    if (old.ParcaAdi != updatedProduct.ParcaAdi)
                        sb.AppendLine($"- Parça Adı: '{old.ParcaAdi}' → '{updatedProduct.ParcaAdi}'");

                    if (old.Sinif != updatedProduct.Sinif)
                        sb.AppendLine($"- Sınıf: '{old.Sinif}' → '{updatedProduct.Sinif}'");

                    if (old.FiyatTL != updatedProduct.FiyatTL)
                        sb.AppendLine($"- Fiyat (TL): {old.FiyatTL} → {updatedProduct.FiyatTL}");

                    if (old.FiyatEUR != updatedProduct.FiyatEUR)
                        sb.AppendLine($"- Fiyat (EUR): {old.FiyatEUR} → {updatedProduct.FiyatEUR}");

                    if (old.Aciklama != updatedProduct.Aciklama)
                        sb.AppendLine($"- Açıklama değiştirildi.");

                    if (old.ResimYolu != updatedProduct.ResimYolu)
                        sb.AppendLine($"- Görsel güncellendi.");

                    var oldMakineIds = old.ProductMachines.Select(pm => pm.MakineId).OrderBy(id => id).ToList();
                    var newMakineIds = updatedProduct.MakineIdleri.OrderBy(id => id).ToList();

                    if (!oldMakineIds.SequenceEqual(newMakineIds))
                    {
                        // Eğer machineDict varsa constructor'dan al, yoksa burada yeniden çek
                        var machineDict = await GetMachineDictionaryAsync();

                        var oldMakineNames = string.Join(", ", oldMakineIds.Select(id => machineDict.ContainsKey(id) ? machineDict[id] : $"Makine#{id}"));
                        var newMakineNames = string.Join(", ", newMakineIds.Select(id => machineDict.ContainsKey(id) ? machineDict[id] : $"Makine#{id}"));

                        sb.AppendLine($"- Makine Atamaları: {oldMakineNames} → {newMakineNames}");

                        LogHelper.LogAction(_currentUser.Id, sb.ToString().Trim());

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme işlemi başarısız: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Geri butonu (Kapat)
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} yedek parçayı güncellemedi. Güncelleme sayfasından çıkış yaptı.");
            Window.GetWindow(this)?.Close();
        }

        private string GetMachineNameById(int id)
        {
            var machine = lstMakine.Items.Cast<Machine>().FirstOrDefault(m => m.Id == id);
            return machine?.MakineAdi ?? "Bilinmeyen Makine"; // **Eğer bulamazsa "Bilinmeyen Makine" döndür**
        }

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


        #region Fotoğraf güncellme işlemleri
        private async void btnSelectPhotoUpdate_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp;*.svg",
                Title = "Bir Fotoğraf Seçin"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePathUpdate = openFileDialog.FileName;

                // Seçilen fotoğrafın adını göster
                txtPhotoNameUpdate.Text = Path.GetFileName(selectedImagePathUpdate);

                // Fotoğrafı API'ye yükle ve yeni yolu al
                selectedImagePathUpdate = await UploadImage(selectedImagePathUpdate);
            }
        }

        private async Task<string> UploadImage(string filePath)
        {
            using (var client = new HttpClient())
            {
                var form = new MultipartFormDataContent();
                var stream = File.OpenRead(filePath);
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                form.Add(fileContent, "file", System.IO.Path.GetFileName(filePath)); // Hata olan kısım düzeltildi!

                var response = await client.PostAsync("http://192.168.5.81:5001/api/Product/upload-photo", form);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<dynamic>();
                    return result.photoPath; // API'den dönen fotoğraf yolu
                }
                return null;
            }
        }

        private async Task UpdateProduct(ProductUpdateDto product)
        {
            Console.WriteLine($"📷 Güncellenen Resim Yolu: {product.ResimYolu}");

            using (var client = new HttpClient())
            {
                var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(product), System.Text.Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"http://192.168.5.81:5001/api/Product/{product.Id}", jsonContent);
                response.EnsureSuccessStatusCode();
            }
        }
        #endregion
    }
}
