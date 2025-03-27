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



        private void btnGeneratePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var excelApp = new Excel.Application();

                // ŞABLON YOLU
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string invoiceFolder = Path.Combine(desktopPath, "Invoice");
                string templateName = txtCountry.Text.Trim().ToLower() == "türkiye" ? "yurticisablon.xlsx" : "yurtdisisablon.xlsx";
                string templatePath = Path.Combine(invoiceFolder, templateName);

                // PDF YOLU
                Directory.CreateDirectory(invoiceFolder); // Klasör yoksa oluştur
                string fileName = $"{txtCustomerName.Text}_{DateTime.Now:ddMMyyyy}.pdf";
                string outputPath = Path.Combine(invoiceFolder, fileName);

                var workbook = excelApp.Workbooks.Open(templatePath);
                Excel.Worksheet sheet = workbook.Sheets[1];

                // Hücre yazımları
                sheet.Cells[7, 4].Value = txtCustomerName.Text;       // D7 - Buyer
                sheet.Cells[8, 4].Value = txtAddress.Text;             // D8 - Address
                sheet.Cells[9, 4].Value = txtDeliveryAddress.Text;     // D9 - Delivery Address
                sheet.Cells[10, 4].Value = txtContact.Text;            // D10 - Contact
                sheet.Cells[11, 4].Value = txtEmail.Text;              // D11 - Email
                sheet.Cells[7, 11].Value = txtOrderDate.Text;          // K7 - Date
                sheet.Cells[8, 11].Value = txtPhone.Text;              // K8 - Number


                int row = 16;

                foreach (var item in dgOrderItems.Items)
                {
                    dynamic data = item;
                    sheet.Cells[row, 2].Value = data.ParcaAdi;       // B - Parça Adı
                    sheet.Cells[row, 10].Value = data.Quantity;      // J - Adet
                    sheet.Cells[row, 12].Value = data.UnitPrice;     // L - Birim Fiyat
                    sheet.Cells[row, 17].Value = data.Total;         // Q - Toplam
                    row++;
                }

                // FİYAT HESAPLAMA BİLGİLERİ
                sheet.Cells[25, 17].Value = $"{txtAdjustmentPercentage.Text}";
                sheet.Cells[27, 17].Value = txtShippingFee.Text;             // Q27 - Kargo ücreti
                sheet.Cells[28, 17].Value = txtTotal.Text;                   // Q28 - Toplam Tutar (Net)

                sheet.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, outputPath,
                    Excel.XlFixedFormatQuality.xlQualityStandard, true, false, 1, 1);

                workbook.Close(false);
                excelApp.Quit();

                Marshal.ReleaseComObject(sheet);
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excelApp);

                Process.Start("explorer.exe", outputPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF oluşturulurken hata oluştu:\n" + ex.Message);
            }
        }
    }
}
