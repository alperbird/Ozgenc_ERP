using EntityLayer.DTOs;
using ERPClient.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for YurtIci.xaml
    /// </summary>
    public partial class YurtIci : Window
    {
        HttpClient _client = new HttpClient();
        private const string customerUrl = "http://192.168.5.81:5001/api/Customer";
        private const string productUrl = "http://192.168.5.81:5001/api/Product";

        private List<Customer> customers = new();
        private List<Product> products = new();
        private ObservableCollection<ProductOrderRow> orderItems = new();

        public YurtIci()
        {
            InitializeComponent();
            LoadCustomer();
            LoadProduct();
            dgProducts.ItemsSource = orderItems;
        }

        private async void LoadCustomer()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(customerUrl);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var allCustomers = JsonConvert.DeserializeObject<List<Customer>>(json) ?? new();

                customers = allCustomers
                .Where(c => string.Equals(c.CountryName?.Trim(), "Türkiye", StringComparison.OrdinalIgnoreCase))
                .ToList();

                cmbCustomer.ItemsSource = customers;
                cmbCustomer.DisplayMemberPath = "CustomerName";
                cmbCustomer.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Müşteri listesi yüklenemedi!\n" + ex.Message);
            }
        }

        private async void LoadProduct()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(productUrl);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                products = JsonConvert.DeserializeObject<List<Product>>(json) ?? new();

                cmbProduct.ItemsSource = products;
                cmbProduct.DisplayMemberPath = "ParcaAdi";
                cmbProduct.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yedek parçalar yüklenemedi!\n" + ex.Message);
            }
        }

        private void cmbCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCustomer.SelectedItem is Customer selectedCustomer)
            {
                txtCountry.Text = selectedCustomer.CountryName;
                txtTaxNo.Text = selectedCustomer.TaxNo;
                txtAddress.Text = selectedCustomer.Address;
                txtDeliveryAddress.Text = selectedCustomer.DeliveryAddress;
                txtPhone.Text = selectedCustomer.PhoneNumber;
                txtEmail.Text = selectedCustomer.Email;

                // Tarih formatımız
                txtOrderDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            }
        }

        private void cmbProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProduct.SelectedItem is Product selectedProduct)
            {
                txtProductName.Text = selectedProduct.ParcaAdi;
                txtUnitPrice.Text = selectedProduct.FiyatEUR.ToString("N2");
                txtQuantity.Text = "1"; // her zaman başlangıç olarak 1

                if (!decimal.TryParse(txtShippingFee.Text, out decimal shippingFee))
                {
                    MessageBox.Show("Geçerli bir kargo ücreti giriniz!");
                    return;
                }
            }
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            // Kontrollerimizi yapıyoruz
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Lütfen parça adını giriniz.");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir adet giriniz.");
                return;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice < 0)
            {
                MessageBox.Show("Lütfen geçerli bir fiyat giriniz.");
                return;
            }

            // DataGrid'e ekleme işlemimiz
            var newItem = new ProductOrderRow
            {
                ProductName = txtProductName.Text,
                Quantity = quantity,
                UnitPrice = unitPrice,
            };

            orderItems.Add(newItem);
            Clean();

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ProductOrderRow selectedRow)
            {
                if (MessageBox.Show($"'{selectedRow.ProductName}' ürününü silmek istiyor musunuz?",
                                    "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    orderItems.Remove(selectedRow);
                }
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Yine bir validation işlemi yapalım sakatlık çıkmasın...
            if (cmbCustomer.SelectedItem is not Customer selectedCustomer)
            {
                MessageBox.Show("Lütfen bir müşteri seçin.");
                return;
            }

            if (orderItems.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir ürün ekleyin.");
                return;
            }

            decimal shippingFee = 0;
            if (!decimal.TryParse(txtShippingFee.Text, out shippingFee))
            {
                shippingFee = 0;
            }

            int adjustmentType = 0;
            if (cmbAdjustmentType.SelectedItem is ComboBoxItem selectedType && int.TryParse(selectedType.Tag?.ToString(), out int parsedType))
            {
                adjustmentType = parsedType;
            }

            // Oranı alıyoruz
            decimal.TryParse(txtAdjustmentPercentage.Text, out decimal adjustmentPercentage);

            //OrderItems -> ProductId eşleştirmesi
            var items = orderItems.Select(item => new OrderItemCreateDto
            {
                ProductId = products.FirstOrDefault(p => p.ParcaAdi == item.ProductName)?.Id ?? 0,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList();

            //DTO
            var dto = new OrderCreateDto
            {
                CustomerId = selectedCustomer.Id,
                OrderDate = DateTime.Now,
                ShippingFee = shippingFee,
                AdjustmentType = (EntityLayer.Entities.Enums.PriceAdjustmentType)adjustmentType,
                AdjustmentPercentage = adjustmentPercentage,
                OrderItems = orderItems.Select(item => new OrderItemCreateDto
                {
                    ProductId = products.First(p => p.ParcaAdi == item.ProductName).Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.PostAsync("http://192.168.5.81:5001/api/Order", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Sipariş başarıyla oluşturuldu!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Formu temizle
                    orderItems.Clear();
                    cmbCustomer.SelectedIndex = -1;
                    txtCountry.Text = string.Empty;
                    txtTaxNo.Text = string.Empty;
                    txtAddress.Text = string.Empty;
                    txtDeliveryAddress.Text = string.Empty;
                    txtPhone.Text = string.Empty;
                    txtEmail.Text = string.Empty;
                    txtOrderDate.Text = string.Empty;
                    txtShippingFee.Text = string.Empty;
                    txtAdjustmentPercentage.Text = "0";
                    cmbProduct.SelectedIndex = -1;
                    cmbAdjustmentType.SelectedIndex = -1;
                    txtProductName.Clear();
                    txtQuantity.Text = "1";
                    txtUnitPrice.Clear();
                    txtShippingFee.Clear();
                    txtAdjustmentPercentage.Text = "0";
                }
                else
                {
                    MessageBox.Show("Sipariş oluşturulamadı. Sunucudan hata döndü.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Bir hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Clean()
        {
            cmbProduct.SelectedIndex = -1;
            txtProductName.Clear();
            txtQuantity.Text = "1";
            txtUnitPrice.Clear();
        }
    }
}
