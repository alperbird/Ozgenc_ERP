using EntityLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;
using System.Diagnostics;
using Path = System.IO.Path;
using EntityLayer.Entities.Enums;

namespace ERPClient.SiparisModule
{
    /// <summary>
    /// Interaction logic for SiparisDetay.xaml
    /// </summary>
    public partial class SiparisDetay : Window
    {
        private readonly OrderDetailDto _detail;


        public SiparisDetay(OrderDetailDto detail)
        {
            InitializeComponent();
            _detail = detail;
            LoadDetails();

        }

        // Sipariş geçmişi içerisinde bulunan sipariş detayları butonuna tıklanıldığında çağırılan kısım
        private void LoadDetails()
        {
            txtCustomerName.Text = _detail.Customer.CustomerName;
            txtCountry.Text = _detail.Customer.Country;
            txtTaxNo.Text = _detail.Customer.TaxNo;
            txtAddress.Text = _detail.Customer.Address;
            txtDeliveryAddress.Text = _detail.Customer.DeliveryAddress;
            txtContact.Text = _detail.Customer.Contact;
            txtPhone.Text = _detail.Customer.PhoneNumber;
            txtEmail.Text = _detail.Customer.Email;

            txtOrderDate.Text = _detail.OrderDate.ToShortDateString();

            var YurtIci = _detail.Customer.Country?.ToLower() == "Türkiye";
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
                x.Product.ParcaAdi,
                x.Quantity,
                x.UnitPrice,
                Total = x.Quantity * x.UnitPrice
            }).ToList();

            dgOrderItems.ItemsSource = items;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
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
                string adjustment = txtAdjustmentPercentage.Text;
                string shippingFee = txtShippingFee.Text;
                string total = txtTotal.Text;

                // Ürünleri JSON'a çeviriyoruz
                var items = dgOrderItems.Items
                    .OfType<dynamic>()
                    .Select(x => new PdfOrderItemDto
                    {
                        ParcaAdi = x.ParcaAdi,
                        Quantity = x.Quantity,
                        UnitPrice = x.UnitPrice,
                        Total = x.Total
                    }).ToList();

                string jsonItems = System.Text.Json.JsonSerializer.Serialize(items);

                // exe yolu (benim pc için. Siz de kendi dosya yolunuza göre burayı düzenlemelisiniz...)
                string exePath = @"C:\Users\root\source\repos\Ozgenc_ERP\GeneratePdf\bin\Debug\GeneratePdf.exe";

                // exe yolu (server pc için: Server'da değişiklik yapıldığında burası açılacak)
                //string exePath = @"C:\Ozgenc_ERP\Tools\GeneratePdf.exe";

                // JSON içindeki çift tırnakları escape et
                string escapedJson = jsonItems.Replace("\"", "\\\"");

                // Argümanları hazırla
                string arguments = $"\"{customerName}\" \"{address}\" \"{deliveryAddress}\" \"{contact}\" \"{email}\" \"{phone}\" \"{country}\" \"{adjustment}\" \"{shippingFee}\" \"{total}\" \"{escapedJson}\"";

                await Task.Run(() =>
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = arguments,
                        UseShellExecute = false, // Shell'e ihtiyaç yok
                        CreateNoWindow = true,   // CMD ekranını sakla
                        WindowStyle = ProcessWindowStyle.Hidden // Garanti olsun
                        //WorkingDirectory = @"C:\Ozgenc_ERP\Tools" // exe'nin bulunduğu klasör! Burası da server kısmında değişiklik olursa açılacak
                    };

                    Process.Start(psi);

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