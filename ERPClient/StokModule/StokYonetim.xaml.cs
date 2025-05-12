using EntityLayer.Entities;
using ERPClient.DTOs;
using ERPClient.Model;
using ERPClient.ViewModel;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace ERPClient.StokModule
{
    /// <summary>
    /// Interaction logic for StokYonetim.xaml
    /// </summary>
    public partial class StokYonetim : Window
    {
        private StokYonetimViewModel vm = new();
        private readonly User _currentUser;
        public StokYonetim(User user)
        {
            InitializeComponent();
            DataContext = vm;
            _currentUser = user;

            Loaded += async (s, e) =>
            {
                await LoadFromApi(); // ürünleri çeker + LoadProducts içeride çağrılır
            };
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Stok Takip sayfasından çıkış yaptı.");
            this.Close();
        }

        public async Task LoadFromApi()
        {
            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://192.168.5.81:5001"); 
                var response = await client.GetAsync("/api/Product/stocklist");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var products = JsonConvert.DeserializeObject<List<ProductStockDto>>(json);

                    if (products != null && products.Any())
                        vm.LoadProducts(products);
                    else
                        Debug.WriteLine("API'den boş liste geldi.");
                }
                else
                {
                    Debug.WriteLine($"API Hatası: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API bağlantısı başarısız: {ex.Message}");
            }
        }

        private async Task UpdateStockToApiAsync(int productId, int newStock)
        {
            var dto = new ProductStockDto
            {
                Id = productId,
                ParcaAdi = vm.SelectedProduct.ParcaAdi,
                Stok = newStock
            };

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PutAsync("http://192.168.5.81:5001/api/Product/update-stock", content); 

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Stok güncellemesi API'ye yazılamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        #region Stok Butonları
        private async void BtnStokEkle_Click(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedProduct is null) return;
            if (int.TryParse(txtStokMiktar.Text, out var miktar))
            {
                vm.SelectedProduct.Stok += miktar;
                vm.UpdatePagedProducts();

                await UpdateStockToApiAsync(vm.SelectedProduct.Id, miktar); // API'ye güncelle

                CheckStockWarning(vm.SelectedProduct.Stok);
                LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} '{vm.SelectedProduct.ParcaAdi}' parçasına {miktar} adet stok ekledi.");
            }
            txtStokMiktar.Text = ""; // miktarı sıfırla
        }

        private async void BtnStokAzalt_Click(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedProduct is null) return;
            if (int.TryParse(txtStokMiktar.Text, out var miktar))
            {
                vm.SelectedProduct.Stok -= miktar;
                vm.UpdatePagedProducts();

                await UpdateStockToApiAsync(vm.SelectedProduct.Id, -miktar);

                CheckStockWarning(vm.SelectedProduct.Stok);
                LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} '{vm.SelectedProduct.ParcaAdi}' parçasından {miktar} adet stok düşürdü.");
            }
            txtStokMiktar.Text = ""; // miktarı sıfırla
        }

        #endregion

        #region Pagination Butonları
        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            vm.PrevPage();
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            vm.NextPage();
        }
        #endregion

        #region Uyarı Mesajımız
        private void ShowUyari(string mesaj)
        {
            txtUyari.Text = mesaj;
            txtUyari.Visibility = Visibility.Visible;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += (s, e) =>
            {
                txtUyari.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        private void CheckStockWarning(int stok)
        {
            if (stok < 0)
                ShowUyari("Yedek parça stoğu eksiye düştü!");
            else if (stok <= 10)
                ShowUyari("Yedek parça stoğu miktarı azaldı!");
        }
        #endregion
    }
}
