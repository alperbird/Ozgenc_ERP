using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using JsonSerializer = System.Text.Json.JsonSerializer;
namespace GeneratePdf
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 11)
            {
                Console.WriteLine("Eksik parametre. Lütfen sırasıyla şunları gönderin: CustomerName, Address, DeliveryAddress, Contact, Email, Phone, Country, Adjustment, ShippingFee, Total, JSON_ÜrünListesi");
                return;
            }

            string customerName = args[0];
            string address = args[1];
            string deliveryAddress = args[2];
            string contact = args[3];
            string email = args[4];
            string phone = args[5];
            string country = args[6];
            string adjustmentText = args[7]; // kullanılmayacak yurt içi için
            string shippingFee = args[8];    // kullanılmayacak yurt içi için
            string total = args[9];
            string jsonItems = args[10];

            List<PdfOrderItemDto> items;
            try
            {
                items = JsonSerializer.Deserialize<List<PdfOrderItemDto>>(jsonItems);
            }
            catch (Exception)
            {
                Console.WriteLine("Ürün listesi deserialize edilemedi. JSON hatalı olabilir.");
                return;
            }

            string masaustu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string invoiceKlasoru = Path.Combine(masaustu, "Invoice");
            Directory.CreateDirectory(invoiceKlasoru);

            string tarihFormatli = DateTime.Now.ToString("dd_MM_yyyy");
            string dosyaAdiBase = $"{customerName}_{tarihFormatli}";
            string pdfYolu = Path.Combine(invoiceKlasoru, dosyaAdiBase + ".pdf");

            int sayac = 1;
            while (File.Exists(pdfYolu))
            {
                pdfYolu = Path.Combine(invoiceKlasoru, $"{dosyaAdiBase}_({sayac}).pdf");
                sayac++;
            }

            string exeKlasoru = AppDomain.CurrentDomain.BaseDirectory;
            string sablonAdi = country.Trim().ToLower() == "türkiye" ? "yurticisablon.xlsx" : "yurtdisisablon.xlsx";
            string sablonYolu = Path.Combine(invoiceKlasoru, sablonAdi);

            if (!File.Exists(sablonYolu))
            {
                Console.WriteLine($"Şablon dosyası bulunamadı: {sablonYolu}");
                return;
            }

            var excelApp = new Excel.Application();
            Excel.Workbook workbook = excelApp.Workbooks.Open(sablonYolu);
            Excel._Worksheet sheet = workbook.Sheets[1];

            try
            {
                if (country.Trim().ToLower() == "türkiye")
                {
                    // 🇹🇷 Yurt İçi Şablon
                    sheet.Cells[11, 4].Value = customerName;      // D11
                    sheet.Cells[12, 4].Value = contact;           // D12
                    sheet.Cells[13, 4].Value = address;           // D13
                    sheet.Cells[14, 4].Value = deliveryAddress;   // D14
                    sheet.Cells[11, 9].Value = DateTime.Now.ToString("dd/MM/yyyy"); // I11
                    sheet.Cells[15, 9].Value = email;             // I15

                    int row = 18;
                    foreach (var item in items)
                    {
                        sheet.Cells[row, 4].Value = item.ParcaAdi;   // D
                        sheet.Cells[row, 8].Value = item.Quantity;   // H
                        sheet.Cells[row, 9].Value = item.UnitPrice;  // I
                        sheet.Cells[row, 10].Value = item.Total;     // J
                        row++;
                    }

                    // Toplam ve KDV'li toplam
                    sheet.Cells[24, 10].Value = total; // J24

                    if (decimal.TryParse(total, out decimal totalDecimal))
                    {
                        decimal kdvli = totalDecimal + 1.2m;
                        sheet.Cells[29, 10].Value = kdvli.ToString("0.00"); // J29
                    }
                }
                else
                {
                    // 🌍 Yurt Dışı Şablon
                    sheet.Cells[7, 4].Value = customerName;    // D7
                    sheet.Cells[8, 4].Value = address;         // D8
                    sheet.Cells[9, 4].Value = deliveryAddress; // D9
                    sheet.Cells[10, 4].Value = contact;        // D10
                    sheet.Cells[11, 4].Value = email;          // D11
                    sheet.Cells[7, 11].Value = DateTime.Now.ToString("dd/MM/yyyy"); // K7
                    sheet.Cells[8, 11].Value = phone;          // K8

                    int row = 16;
                    foreach (var item in items)
                    {
                        sheet.Cells[row, 2].Value = item.ParcaAdi;   // B
                        sheet.Cells[row, 10].Value = item.Quantity;  // J
                        sheet.Cells[row, 12].Value = item.UnitPrice; // L
                        sheet.Cells[row, 17].Value = item.Total;     // Q (toplam)
                        row++;
                    }

                    sheet.Cells[25, 17].Value = adjustmentText; // Q25
                    sheet.Cells[27, 17].Value = shippingFee;     // Q27
                    sheet.Cells[28, 17].Value = total;           // Q28
                }

                workbook.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, pdfYolu);

                Console.WriteLine($"PDF oluşturuldu: {pdfYolu}");
                Process.Start(new ProcessStartInfo(pdfYolu) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata oluştu: " + ex.Message);
            }
            finally
            {
                workbook.Close(false);
                excelApp.Quit();

                Marshal.ReleaseComObject(sheet);
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excelApp);
            }
        }

        class PdfOrderItemDto
        {
            public string ParcaAdi { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total { get; set; }
        }
    }
}
