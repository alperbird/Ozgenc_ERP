using User = EntityLayer.Entities.User;
using EntityLayer.DTOs;
using Newtonsoft.Json;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using ERPClient.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Text;



namespace ERPClient.SiparisModule
{
    /// <summary>
    /// Interaction logic for SiparisGecmisi.xaml
    /// </summary>
    public partial class SiparisGecmisi : Window
    {
        HttpClient _client = new HttpClient();
        private readonly User _currentUser;
        private const string apiUrl = "http://192.168.5.81:5001/api/Order";
        private List<OrderListDto> allOrders = new(); // loadtable içine dolacak

        //pagination 
        private int currentPage = 1;
        private int maxPage;
        private const int pageSize = 16;

        
        public SiparisGecmisi(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadTable();
        }

        private async void LoadTable()
        {
            try
            {
                string pagedUrl = $"http://192.168.5.81:5001/api/Order/paged?page={currentPage}&pageSize={pageSize}";
                HttpResponseMessage response = await _client.GetAsync(pagedUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    // Artık PagedResult<OrderListDto> deserializeliyoruz
                    var result = JsonConvert.DeserializeObject<EntityLayer.DTOs.PagedResult<OrderListDto>>(jsonResponse);

                    if (result != null)
                    {
                        var pagedOrders = result.Items;
                        int totalOrderCount = result.TotalCount;
                        maxPage = (int)Math.Ceiling((double)totalOrderCount / pageSize);
                        allOrders = pagedOrders.ToList();

                        Dispatcher.Invoke(() =>
                        {
                            dgOrders.ItemsSource = pagedOrders;
                            txtPageInfo.Text = $"Sayfa {currentPage} - Gösterilen: {pagedOrders.Count} / Toplam: {totalOrderCount}";
                        });
                    }
                }
                else
                {
                    string err = await response.Content.ReadAsStringAsync();
                    System.Windows.MessageBox.Show($"Veri yüklenemedi! Status Code: {response.StatusCode}\nDetay: {err}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Geçmiş siparişler yüklenirken bir hata oluştu!\n" + ex.Message);
            }
        }


        // Sipariş detaylarını listeleyen buton
        private async void btnList_Click(object sender, RoutedEventArgs e) //Sipariş detaylarının listeleneceği kısım
        {
            var btn = sender as System.Windows.Controls.Button;
            var selectedOrder = btn?.DataContext as OrderListDto;

            if (selectedOrder != null)
            {
                // Sipariş ID ile detaylı siparişi API'den çek
                using var client = new HttpClient();
                var response = await client.GetAsync($"http://192.168.5.81:5001/api/Order/{selectedOrder.Id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var detailDto = JsonConvert.DeserializeObject<OrderDetailDto>(json);

                    var detail = new SiparisDetay(detailDto, _currentUser);
                    LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname}, {selectedOrder.CustomerName}  Sipariş Detayları sayfasına girdi.");
                    detail.ShowDialog();
                }
                else
                {
                    System.Windows.MessageBox.Show("Sipariş detayları alınamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region Tarih Aralığı Filtreleme
        private void cmbDateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (cmbDateFilter.SelectedItem as ComboBoxItem)?.Content.ToString();

            bool showDates = selected == "Tarih Aralığı";
            dpStartDate.Visibility = showDates ? Visibility.Visible : Visibility.Collapsed;
            dpEndDate.Visibility = showDates ? Visibility.Visible : Visibility.Collapsed;
        }


        private void BtnFiltrele_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = cmbDateFilter.SelectedItem as ComboBoxItem;
            var filterType = selectedItem?.Tag?.ToString();

            DateTime now = DateTime.Now;
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MaxValue;

            if (filterType == "Weekly")
            {
                int diff = (int)now.DayOfWeek - (int)DayOfWeek.Monday;
                if (diff < 0)
                {
                    diff += 7;
                }
                startDate = now.AddDays(-diff).Date;
                endDate = startDate.AddDays(6).Date;
            }

            else if (filterType == "Monthly")
            {
                startDate = new DateTime(now.Year, now.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }

            else if(filterType == "Custom")
            {
                if (dpStartDate.SelectedDate.HasValue && dpEndDate.SelectedDate.HasValue)
                {
                    startDate = dpStartDate.SelectedDate.Value.Date;
                    endDate = dpEndDate.SelectedDate.Value.Date;
                }
                else
                {
                    System.Windows.MessageBox.Show("Tarih aralığı seçmediniz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            var filtered = allOrders.Where(x => x.OrderDate.Date >= startDate && x.OrderDate.Date <= endDate).ToList();
            dgOrders.ItemsSource = filtered;
            txtPageInfo.Text = $"Filtreli sonuçlar: {filtered.Count} kayıt";
            btnPrevious.IsEnabled = false;
            btnNext.IsEnabled = false;
        }
        #endregion

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Sipariş Geçmişi sayfasından çıkış yaptı.");
            this.Close();
        }

        
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadTable();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < maxPage)
            {
                currentPage++;
                LoadTable();
            }
        }

        public void Refresh()
        {
            LoadTable();
        }

        private void BtnFiltreTemizle_Click(object sender, RoutedEventArgs e)
        {
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            cmbDateFilter.SelectedIndex = -1;

            currentPage = 1;
            LoadTable();

            btnPrevious.IsEnabled = true;
            btnNext.IsEnabled = true;
        }

        private async void btnOnaylandiMi_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is OrderListDto selectedOrder)
            {
                var confirm = MessageBox.Show("Siparişi onaylamak istiyor musunuz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                    return;

                // Sipariş ID'si
                int orderId = selectedOrder.Id;

                // Buradan detayları çekip işlemleri yapabilirsin...
                var response = await _client.GetAsync($"http://192.168.5.81:5001/api/Order/{orderId}");
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Sipariş detayı alınamadı!");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var detail = JsonConvert.DeserializeObject<OrderDetailDto>(json);

                var groupedItems = detail.OrderItems
                    .GroupBy(x => x.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalQuantity = g.Sum(x => x.Quantity)
                    });

                foreach (var item in groupedItems)
                {
                    var stockResp = await _client.GetAsync($"http://192.168.5.81:5001/api/Product/{item.ProductId}");
                    if (!stockResp.IsSuccessStatusCode)
                        continue;

                    var productJson = await stockResp.Content.ReadAsStringAsync();
                    var product = JsonConvert.DeserializeObject<Product>(productJson);

                    int newStock = product.Stok - item.TotalQuantity;

                    if (newStock < 0)
                    {
                        MessageBox.Show($"{product.ParcaAdi} stoğu eksiye düştü! Yeni stok: {newStock}", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else if (newStock <= 10)
                    {
                        MessageBox.Show($"{product.ParcaAdi} stoğu kritik seviyeye düştü! Yeni stok: {newStock}", "Bilgilendirme", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    var updateDto = new ProductStockChangeDto
                    {
                        Id = item.ProductId,
                        QuantityChange = newStock
                    };

                    var jsonUpdate = JsonConvert.SerializeObject(updateDto);
                    var content = new StringContent(jsonUpdate, Encoding.UTF8, "application/json");

                    await _client.PutAsync("http://192.168.5.81:5001/api/Product/change-stock", content);
                }

                MessageBox.Show("Sipariş başarıyla onaylandı. Stok güncellendi.");
            }
        }
    }
}
