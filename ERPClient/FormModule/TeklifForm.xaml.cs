using ClosedXML.Excel;
using ERPClient.Model;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Path = System.IO.Path;
using User = EntityLayer.Entities.User;

namespace ERPClient.FormModule
{
    /// <summary>
    /// Interaction logic for TeklifForm.xaml
    /// </summary>
    public partial class TeklifForm : Window
    {
        HttpClient _client = new HttpClient();
        private const string customerUrl = "http://192.168.5.81:5001/api/Customer";
        private const string productUrl = "http://192.168.5.81:5001/api/Product";

        private readonly User _currentUser;
        private List<Customer> customers = new();
        private List<Product> products = new();
        private ICollectionView productView;
        //private ICollectionView customerView;

        private TextBox cmbCustomerTextBox;
        private ObservableCollection<ProductOrderRow> orderItems = new();
        private ObservableCollection<Customer> filteredCustomers = new();
        private Model.Customer? _selectedCustomer;
        public TeklifForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadCustomers();
            LoadProducts();
            dgProducts.ItemsSource = orderItems;
        }

        #region Yedek Parça Ekleme ve Silme kısmı
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
                UnitPrice = unitPrice
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
        #endregion
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Şablon Yolu
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "numune.xlsx");

                if (!File.Exists(templatePath))
                {
                    MessageBox.Show("Şablon bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 2. Masaüstü yoluna klasörleri oluşturma (eğer yoksa)
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string teklifFolder = Path.Combine(desktopPath, "Invoice", "Teklif");

                if (!Directory.Exists(teklifFolder))
                {
                    Directory.CreateDirectory(teklifFolder);
                }

                string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_Teklif.xlsx";
                string outputPath = Path.Combine(teklifFolder, fileName);
                File.Copy(templatePath, outputPath, true);

                using var workbook = new XLWorkbook(outputPath);
                var worksheet = workbook.Worksheet(1);

                // 3. Müşteri Bilgilerini Yaz
                worksheet.Cell("D7").Value = _selectedCustomer.CustomerName;
                worksheet.Cell("D8").Value = _selectedCustomer.Address;
                worksheet.Cell("D9").Value = _selectedCustomer.DeliveryAddress;
                worksheet.Cell("D10").Value = _selectedCustomer.Contact;
                worksheet.Cell("D11").Value = _selectedCustomer.Email;
                worksheet.Cell("K7").Value = DateTime.Now.ToString("dd/MM/yyyy");
                worksheet.Cell("K8").Value = _selectedCustomer.PhoneNumber;

                // 4. Ürünleri yaz
                int startRow = 16;
                int index = 1;
                decimal subtotal = 0;

                foreach (var product in orderItems)
                {
                    worksheet.Cell(startRow, "A").Value = index++;
                    worksheet.Cell(startRow, "B").Value = string.IsNullOrWhiteSpace(product.CustomProductName)
                        ? product.ProductName
                        : product.CustomProductName;
                    worksheet.Cell(startRow, "J").Value = product.Quantity;
                    worksheet.Cell(startRow, "L").Value = product.UnitPrice;

                    decimal lineTotal = product.Quantity * product.UnitPrice;
                    worksheet.Cell(startRow, "Q").Value = lineTotal;

                    subtotal += lineTotal;
                    startRow++;
                }

                // 5. Toplamlar
                worksheet.Cell("Q24").Value = subtotal;
                worksheet.Cell("Q26").Value = subtotal;

                // Kargo
                if (decimal.TryParse(txtShippingFee.Text, out decimal shipping))
                    worksheet.Cell("Q27").Value = shipping;
                else
                    worksheet.Cell("Q27").Value = 0;

                // Asıl toplam
                worksheet.Cell("Q28").FormulaA1 = "Q26+Q27";

                // 6. Kaydet
                workbook.Save();

                // 7. PDF'e Çevir (LibreOffice ile)
                string sofficePath = GetSofficePath(); // sistemine göre ayarla
                string pdfOutput = Path.ChangeExtension(outputPath, ".pdf");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = sofficePath,
                        Arguments = $"--headless --convert-to pdf \"{outputPath}\" --outdir \"{Path.GetDirectoryName(outputPath)}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };
                process.Start();
                LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname}, {_selectedCustomer.CustomerName}  adlı müşterinin düşük teklif proformasını oluşturdu.");
                process.WaitForExit();

                // 8. PDF dosyasını aç
                if (File.Exists(pdfOutput))
                    Process.Start(new ProcessStartInfo(pdfOutput) { UseShellExecute = true });
                else
                    MessageBox.Show("PDF oluşturulamadı!");

            }
            catch (Exception ex)
            {
                MessageBox.Show("HATA: " + ex.Message);
            }
        }


        #region Customer
        private void cmbCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCustomer.SelectedItem is Customer selectedCustomer)
            {
                _selectedCustomer = selectedCustomer;
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

        private async void LoadCustomers()
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
        #endregion

        #region Product
        private void cmbProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProduct.SelectedItem is Product selectedProduct)
            {
                txtProductName.Text = selectedProduct.ParcaAdi;
                txtUnitPrice.Text = selectedProduct.FiyatEUR.ToString("N2");
                txtQuantity.Text = "1"; // her zaman başlangıç olarak 1
                txtShippingFee.Text = "0"; // her zaman başlangıç olarak 0

                if (!decimal.TryParse(txtShippingFee.Text, out decimal shippingFee))
                {
                    MessageBox.Show("Geçerli bir kargo ücreti giriniz!");
                    return;
                }
            }
        }

        private async void LoadProducts()
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
        #endregion
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Teklif Form sayfasından çıkış yaptı.");
            this.Close();
        }
        private void Clean()
        {
            cmbProduct.SelectedIndex = -1;
            txtProductName.Clear();
            txtQuantity.Text = "1";
            txtUnitPrice.Clear();
        }

        private string GetSofficePath()
        {
            if (OperatingSystem.IsWindows()) // windows işletim sistemi ise
            {
                return @"C:\Program Files\LibreOffice\program\soffice.exe";
            }

            else if (OperatingSystem.IsMacOS()) //mac işletim sistemi ise
            {
                return @"/Applications/LibreOffice.app/Contents/MacOS/soffice";
            }

            else // Linux işletim sistemi ise
            {
                return "/usr/bin/soffice";
            }
        }

        #region Filtreleme ve Arama
        private void cmbProduct_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
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

        private void cmbCustomer_Loaded(object sender, RoutedEventArgs e)
        {
            cmbCustomerTextBox = (TextBox)cmbCustomer.Template.FindName("PART_EditableTextBox", cmbCustomer);
        }

        private void cmbCustomer_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
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
        #endregion

    }
}
