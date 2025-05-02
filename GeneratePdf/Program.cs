using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

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
            string adjustmentText = args[7];
            string shippingFee = args[8];
            string total = args[9];
            string jsonItems = args[10];

            List<PdfOrderItemDto> items;
            try
            {
                items = JsonConvert.DeserializeObject<List<PdfOrderItemDto>>(jsonItems);
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
            string dosyaAdiBase = $"{tarihFormatli}_{customerName}";
            string pdfYolu = Path.Combine(invoiceKlasoru, dosyaAdiBase + ".pdf");

            int sayac = 1;
            while (File.Exists(pdfYolu))
            {
                pdfYolu = Path.Combine(invoiceKlasoru, $"{dosyaAdiBase}_({sayac}).pdf");
                sayac++;
            }

            string exeKlasoru = AppDomain.CurrentDomain.BaseDirectory;
            string sablonAdi = country.Trim().ToLower() == "türkiye" ? "yurticisablon.xlsx" : "yurtdisisablon.xlsx";
            string sablonYolu = Path.Combine(exeKlasoru, sablonAdi);

            if (!File.Exists(sablonYolu))
            {
                Console.WriteLine($"Şablon dosyası bulunamadı: {sablonYolu}");
                return;
            }

            var excelApp = new Excel.Application
            {
                Visible = false,
                DisplayAlerts = false
            };

            var workbook = excelApp.Workbooks.Open(sablonYolu);
            dynamic sheet = workbook.Sheets[1];

            try
            {
                if (country.Trim().ToLower() == "türkiye")
                {
                    sheet.Cells[11, 4].Value = customerName;
                    sheet.Cells[12, 4].Value = contact;
                    sheet.Cells[13, 4].Value = address;
                    sheet.Cells[15, 4].Value = deliveryAddress;
                    sheet.Cells[11, 9].Value = DateTime.Now.ToString("dd/MM/yyyy");
                    sheet.Cells[15, 9].Value = email;

                    int row = 18;
                    foreach (var item in items)
                    {
                        sheet.Cells[row, 4].Value = item.ParcaAdi;
                        sheet.Cells[row, 8].Value = item.Quantity;
                        sheet.Cells[row, 9].Value = item.UnitPrice;
                        sheet.Cells[row, 10].Value = item.Total;
                        row++;
                    }
                }
                else
                {
                    sheet.Cells[7, 4].Value = customerName;
                    sheet.Cells[8, 4].Value = address;
                    sheet.Cells[9, 4].Value = deliveryAddress;
                    sheet.Cells[10, 4].Value = contact;
                    sheet.Cells[11, 4].Value = email;
                    sheet.Cells[7, 11].Value = DateTime.Now.ToString("dd/MM/yyyy");
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
                }

                workbook.ExportAsFixedFormat(
                    Excel.XlFixedFormatType.xlTypePDF,
                    pdfYolu,
                    Excel.XlFixedFormatQuality.xlQualityStandard,
                    true,
                    false,
                    1,
                    1,
                    false
                );

                Console.WriteLine($"PDF oluşturuldu: {pdfYolu}");
                Process.Start(new ProcessStartInfo(pdfYolu) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata oluştu: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
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
