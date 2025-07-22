using EntityLayer.DTOs;
using EntityLayer.Entities;
using ERPClient.Model;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Customer = ERPClient.Model.Customer;
using Product = ERPClient.Model.Product;

namespace ERPClient.SiparisModule
{
    /// <summary>
    /// Interaction logic for YurtDisi.xaml
    /// </summary>
    public partial class YurtDisi : Window
    {
        HttpClient _client = new HttpClient();
        private readonly User _currentUser;
        private const string customerUrl = "http://192.168.5.81:5001/api/Customer";
        private const string productUrl = "http://192.168.5.81:5001/api/Product";
        private ICollectionView productView;
        private ICollectionView customerView;

        private TextBox cmbCustomerTextBox;
        private List<Customer> customers = new();
        private List<Product> products = new();
        private ObservableCollection<ProductOrderRow> orderItems = new();
        private ObservableCollection<Customer> filteredCustomers = new();

        public YurtDisi(User user)
        {
            InitializeComponent();
            //LoadCustomers();
            //LoadProducts();
            _currentUser = user;
            dgProducts.ItemsSource = orderItems;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Yurt Dışı Sipariş sayfasından çıkış yaptı.");
            this.Close();
        }

        // Müşteri seçeceğiz kalan kısımlar otomatik dolacak falan filan
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
                txtProductName.Text = selectedProduct.ParcaIngilizce;
                txtUnitPrice.Text = selectedProduct.FiyatEUR.ToString("N2");
                txtQuantity.Text = "1"; // her zaman başlangıç olarak 1
                txtShippingFee.Text = "0"; 

                if (!decimal.TryParse(txtShippingFee.Text, out decimal shippingFee))
                {
                    MessageBox.Show("Geçerli bir kargo ücreti giriniz!");
                    return;
                }
            }
        }

        // Ürünleri tabloya ekleyeceğimiz buton
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

            if (cmbProduct.SelectedItem is not Product selectedProduct)
            {
                MessageBox.Show("Lütfen bir ürün seçin.");
                return;
            }
            // DataGrid'e ekleme işlemimiz
            var newItem = new ProductOrderRow
            {
                ProductId = selectedProduct.Id,
                ProductName = txtProductName.Text,
                CustomProductName = txtProductName.Text, // Kullanıcı tarafından girilen özel parça adı
                Quantity = quantity,
                UnitPrice = unitPrice,
                AvailableMachines = new ObservableCollection<string>(
                    selectedProduct.ProductMachines
                    .Select(pm => pm.Machine.MakineAdi)
                    .Distinct()
                    .ToList() ?? new List<string?> { " " }),
                SelectedMachine = selectedProduct.ProductMachines?.FirstOrDefault()?.Machine.MakineAdi ?? " ",
            };

            orderItems.Add(newItem);
            Clean();
        }

        // Sipariş kaydetme işlemi
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            
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

            
            decimal.TryParse(txtAdjustmentPercentage.Text, out decimal adjustmentPercentage);

            
            var items = orderItems.Select(item => new OrderItemCreateDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                CustomProductName = item.CustomProductName,
                SelectedMachine = item.SelectedMachine ?? "GENERAL"
            }).ToList();

            
            var dto = new OrderCreateDto
            {
                CustomerId = selectedCustomer.Id,
                OrderDate = DateTime.Now, 
                ShippingFee = shippingFee,
                AdjustmentType = (EntityLayer.Entities.Enums.PriceAdjustmentType)adjustmentType,
                AdjustmentPercentage = adjustmentPercentage,
                OrderItems = orderItems.Select(item => new OrderItemCreateDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    CustomProductName = item.CustomProductName,
                    SelectedMachine = item.SelectedMachine ?? "GENEL"

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
                    LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname}, '{selectedCustomer.CustomerName}' için yeni bir yurt dışı siparişi oluşturdu.");

                    //foreach (var item in dto.OrderItems)
                    //{
                    //    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    //    if (product != null)
                    //    {
                    //        LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname}, '{product.ParcaAdi}' ürününün stok miktarını siparişle birlikte {item.Quantity} adet düşürdü.");
                    //    }

                    //    int newStok = product.Stok - item.Quantity;

                    //    if (newStok < 0)
                    //    {
                    //        MessageBox.Show($"DİKKAT! '{product.ParcaAdi}' ürününün stok miktarı eksiye düştü!", "Stok Ekside", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //    }
                    //    else if (newStok <= 10)
                    //    {
                    //        MessageBox.Show($"UYARI! '{product.ParcaAdi}' ürününün stok miktarı azaldı!", "Stok Azaldı", MessageBoxButton.OK, MessageBoxImage.Information);
                    //    }
                    //}

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

        // Yurt Dışı müşterilerini çeken müşteri listesi
        private async Task LoadCustomers()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(customerUrl);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var allCustomers = JsonConvert.DeserializeObject<List<Customer>>(json) ?? new();

                customers = allCustomers
            .Where(c => !string.Equals(c.CountryName?.Trim(), "Türkiye", StringComparison.OrdinalIgnoreCase))
            .ToList();

                filteredCustomers = new ObservableCollection<Customer>(customers);
                cmbCustomer.ItemsSource = filteredCustomers;
                cmbCustomer.DisplayMemberPath = "CustomerName";
                cmbCustomer.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Müşteri listesi yüklenemedi!\n" + ex.Message);
            }
        }

        // Ürünleri çeken fonksiyon
        private async Task LoadProducts()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(productUrl);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                products = JsonConvert.DeserializeObject<List<Product>>(json) ?? new();

                productView = CollectionViewSource.GetDefaultView(products);
                cmbProduct.ItemsSource = productView;
                cmbProduct.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yedek parçalar yüklenemedi!\n" + ex.Message);
            }
        }

        // Temizleme fonksiyonumuz
        private void Clean()
        {
            cmbProduct.SelectedIndex = -1;
            txtProductName.Clear();
            txtQuantity.Text = "1";
            txtUnitPrice.Clear();
        }

        // Bu delete butonu sadece yanlış bir ürün eklediğimizde silmeye yarayacak...
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

        #region Arama ve Filtreleme
        private void cmbCustomer_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var input = cmbCustomer.Text.ToLower();

            var filtered = customers
                .Where(c => c.CustomerName != null && c.CustomerName.ToLower().Contains(input))
                .ToList();

            filteredCustomers.Clear();
            foreach (var item in filtered)
                filteredCustomers.Add(item);

            cmbCustomer.IsDropDownOpen = true;
        }

        private void cmbCustomer_Loaded(object sender, RoutedEventArgs e)
        {
            cmbCustomerTextBox = (TextBox)cmbCustomer.Template.FindName("PART_EditableTextBox", cmbCustomer);
        }

        private void cmbProduct_KeyUp(object sender, KeyEventArgs e)
        {
            if (productView == null) return;

            string filterText = cmbProduct.Text?.ToLower() ?? "";

            productView.Filter = item =>
            {
                if (item is Product p)
                    return p.ParcaAdi != null && p.ParcaAdi.ToLower().Contains(filterText);
                return false;
            };

            productView.Refresh();
            cmbProduct.IsDropDownOpen = true;

            var tb = (TextBox)cmbProduct.Template.FindName("PART_EditableTextBox", cmbProduct);
            if (tb != null)
                tb.SelectionStart = tb.Text.Length;
        }



        #endregion

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomers();
            await LoadProducts();
        }
    }
}
