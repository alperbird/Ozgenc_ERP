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
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
            decimal adjustmentValue = 0;
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
                // 🟢 UI'dan tüm verileri önce çekiyoruz
                string customerName = txtCustomerName.Text;
                string address = txtAddress.Text;
                string deliveryAddress = txtDeliveryAddress.Text;
                string contact = txtContact.Text;
                string email = txtEmail.Text;
                string orderDate = txtOrderDate.Text;
                string phone = txtPhone.Text;
                string adjustmentText = txtAdjustmentPercentage.Text;
                string shippingFee = txtShippingFee.Text;
                string total = txtTotal.Text;
                string country = txtCountry.Text.Trim().ToLower();

                var items = dgOrderItems.Items
                    .OfType<dynamic>()
                    .Select(x => new PdfOrderItemDto
                    {
                        ParcaAdi = x.ParcaAdi,
                        Quantity = x.Quantity,
                        UnitPrice = x.UnitPrice,
                        Total = x.Total
                    }).ToList();

                await Task.Run(() =>
                {
                    var excelApp = new Excel.Application();

                    // ŞABLON
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string invoiceFolder = Path.Combine(desktopPath, "Invoice");
                    Directory.CreateDirectory(invoiceFolder);
                    string templateName = country == "Türkiye" ? "yurticisablon.xlsx" : "yurtdisisablon.xlsx";
                    string templatePath = Path.Combine(invoiceFolder, templateName);

                    // DOSYA ADI
                    string baseFileName = $"{customerName}_{DateTime.Now:ddMMyyyy}";
                    string outputPath = Path.Combine(invoiceFolder, baseFileName + ".pdf");
                    int i = 1;
                    while (File.Exists(outputPath))
                        outputPath = Path.Combine(invoiceFolder, $"{baseFileName}_{i++}.pdf");

                    var workbook = excelApp.Workbooks.Open(templatePath);
                    Excel.Worksheet sheet = workbook.Sheets[1];

                    // HÜCRE DOLDUR
                    sheet.Cells[7, 4].Value = customerName;
                    sheet.Cells[8, 4].Value = address;
                    sheet.Cells[9, 4].Value = deliveryAddress;
                    sheet.Cells[10, 4].Value = contact;
                    sheet.Cells[11, 4].Value = email;
                    sheet.Cells[7, 11].Value = orderDate;
                    sheet.Cells[8, 11].Value = phone;

                    int row = 16;
                    foreach (var item in items)
                    {
                        sheet.Cells[row, 2].Value = item.ParcaAdi;
                        sheet.Cells[row, 10].Value = item.Quantity;
                        sheet.Cells[row, 12].Value = item.UnitPrice;
                        sheet.Cells[row, 17].Value = item.Total;
                        row++;
                    }

                    sheet.Cells[25, 17].Value = adjustmentText;
                    sheet.Cells[27, 17].Value = shippingFee;
                    sheet.Cells[28, 17].Value = total;

                    sheet.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, outputPath,
                        Excel.XlFixedFormatQuality.xlQualityStandard, true, false, 1, 1);

                    workbook.Close(false);
                    excelApp.Quit();

                    Marshal.ReleaseComObject(sheet);
                    Marshal.ReleaseComObject(workbook);
                    Marshal.ReleaseComObject(excelApp);

                    // Aç
                    Process.Start("explorer.exe", outputPath);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF oluşturulurken hata oluştu:\n" + ex.Message);
            }
        }



    }
}
