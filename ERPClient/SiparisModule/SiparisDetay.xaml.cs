using EntityLayer.DTOs;
using System.Windows;
using System.IO;
using System.Diagnostics;
using Path = System.IO.Path;
using EntityLayer.Entities.Enums;
using User = EntityLayer.Entities.User;
using ERPClient.Model;

namespace ERPClient.SiparisModule
{
    /// <summary>
    /// Interaction logic for SiparisDetay.xaml
    /// </summary>
    public partial class SiparisDetay : Window
    {
        private readonly OrderDetailDto _detail;
        private readonly User _currentUser;

        public SiparisDetay(OrderDetailDto detail, User user)
        {
            InitializeComponent();
            _detail = detail;
            _currentUser = user;
            LoadDetails();

        }

        // Sipariş geçmişi içerisinde bulunan sipariş detayları butonuna tıklanıldığında çağırılan kısım
        private void LoadDetails()
        {
            txtCustomerName.Text = _detail.Customer.CustomerName;
            txtCountry.Text = _detail.Customer.CountryName?.CountryName ?? "";
            txtTaxNo.Text = _detail.Customer.TaxNo;
            txtAddress.Text = _detail.Customer.Address;
            txtDeliveryAddress.Text = _detail.Customer.DeliveryAddress;
            txtContact.Text = _detail.Customer.Contact;
            txtPhone.Text = _detail.Customer.PhoneNumber;
            txtEmail.Text = _detail.Customer.Email;
            txtTaxOffice.Text = _detail.Customer.TaxOffice;

            txtOrderDate.Text = _detail.OrderDate.ToShortDateString();

            var YurtIci = _detail.Customer.CountryName?.CountryName?.ToLower() == "Türkiye";
            var currencySymbol = YurtIci ? "₺" : "€";

            txtShippingFee.Text = currencySymbol + (_detail.ShippingFee ?? 0).ToString("N2");
            txtTotal.Text = currencySymbol + _detail.TotalPrice.ToString("N2");

            // İNDİRİM/ZAM TUTARI HESAPLAMA
            decimal rawTotal = _detail.OrderItems.Sum(x => x.UnitPrice * x.Quantity);
            //decimal adjustmentValue = 0;
            var percentage = Math.Abs(_detail.AdjustmentPercentage ?? 0);
            txtAdjustmentPercentage.Text = $"{percentage:0}%";

            // Ürünler
            var items = _detail.OrderItems.Select(x => new
            {
                ParcaAdi = string.IsNullOrWhiteSpace(x.CustomProductName) ? x.Product.ParcaAdi : x.CustomProductName,
                x.Quantity,
                x.UnitPrice,
                Total = x.Quantity * x.UnitPrice,
                SelectedMachine = x.SelectedMachine ?? "GENEL",
            }).ToList();

            foreach (var item in items)
            {
                Debug.WriteLine($"PARÇA: {item.ParcaAdi} / MAKİNE: {item.SelectedMachine}");
            }

            dgOrderItems.ItemsSource = items;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Sipariş Detay sayfasından çıkış yaptı.");
            this.Close();
        }



        private async void btnGeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // UI'dan verileri çekiyoruz
                string customerName = txtCustomerName.Text;
                string address = txtAddress.Text;
                string deliveryAddress = txtDeliveryAddress.Text;
                string contact = txtContact.Text;
                string email = txtEmail.Text;
                string phone = txtPhone.Text;
                string country = txtCountry.Text.Trim();
                string adjustment = txtAdjustmentPercentage.Text.Trim();
                string taxoffice = txtTaxOffice.Text;
                string taxno = txtTaxNo.Text;

                if (decimal.TryParse(adjustment.Replace("%", "").Replace(",", "."), out decimal adjustmentValue))
                {
                    if (adjustmentValue == 0)
                        adjustment = "";
                }


                string shippingFee = txtShippingFee.Text.Replace("₺", "").Replace("€", "").Trim();
                string total = txtTotal.Text;

                // Ürünleri JSON'a çeviriyoruz
                var items = dgOrderItems.Items
                    .OfType<dynamic>()
                    .Select(x => new PdfOrderItemDto
                    {
                        ParcaAdi = x.ParcaAdi,
                        Quantity = x.Quantity,
                        UnitPrice = x.UnitPrice,
                        Total = x.Total,

                        SelectedMachine = string.IsNullOrWhiteSpace(x.SelectedMachine) ? "GENEL" : x.SelectedMachine
                    }).ToList();

                string jsonItems = System.Text.Json.JsonSerializer.Serialize(items);

                
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "GeneratePdf.exe");

                if (!File.Exists(exePath))
                {
                    MessageBox.Show("GeneratorPdf uygulamasının dosya konumu yanlış!");
                    return;
                }

                // JSON içindeki çift tırnakları escape et
                string escapedJson = jsonItems.Replace("\"", "\\\"");

                // Argümanları hazırla
                string arguments = $"\"{customerName}\" " +
                    $"\"{address}\" " +
                    $"\"{deliveryAddress}\" " +
                    $"\"{contact}\" " +
                    $"\"{email}\" " +
                    $"\"{phone}\" " +
                    $"\"{country}\" " +
                    $"\"{adjustment}\" " +
                    $"\"{shippingFee}\" " +
                    $"\"{total}\" " +
                    $"\"{escapedJson}\" " +
                    $"\"{taxoffice}\" " +
                    $"\"{taxno}\" ";

                await Task.Run(() =>
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = arguments,
                        UseShellExecute = false, 
                        CreateNoWindow = true,   // CMD ekranını sakla
                        //WindowStyle = ProcessWindowStyle.Hidden
                    };

                    Process.Start(psi);
                    LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname}, {customerName}  adlı müşterinin proformasını oluşturdu.");

                });
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Invoice", "status.txt");
                File.WriteAllText(logPath, $"PDF OLUŞTURULAMADI ❌ -> {DateTime.Now:dd.MM.yyyy HH:mm:ss}\nHATA: {ex.Message}");
                MessageBox.Show("PDF oluşturulurken hata oluştu:\n" + ex.Message);
            }
        }

    }
}