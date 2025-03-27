using EntityLayer.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ERPClient.SiparisModule
{
    /// <summary>
    /// Interaction logic for SiparisGecmisi.xaml
    /// </summary>
    public partial class SiparisGecmisi : Window
    {
        HttpClient _client = new HttpClient();
        private const string apiUrl = "http://192.168.5.81:5001/api/Order";
        private List<OrderListDto> allOrders = new(); // loadtable içine dolacak

        public SiparisGecmisi()
        {
            InitializeComponent();
            LoadTable();
        }

        private async void LoadTable()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                allOrders = JsonConvert.DeserializeObject<List<OrderListDto>>(jsonResponse) ?? new();

                Dispatcher.Invoke(() =>
                {
                    dgOrders.ItemsSource = allOrders;
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show("Geçmiş siparişler yüklenirken bir hata oluştu!\n" + ex.Message);
            }
            
        }

        // Sipariş detaylarını listeleyen buton
        private async void btnList_Click(object sender, RoutedEventArgs e) //Sipariş detaylarının listeleneceği kısım
        {
            var btn = sender as Button;
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

                    var detail = new SiparisDetay(detailDto);
                    detail.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Sipariş detayları alınamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Tarih aralığı seçmediniz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            var filtered = allOrders.Where(x => x.OrderDate.Date >= startDate && x.OrderDate.Date <= endDate).ToList();
            dgOrders.ItemsSource = filtered;
        }
        #endregion

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
