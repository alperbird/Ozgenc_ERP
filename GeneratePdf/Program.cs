using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;

namespace GeneratePdf
{
    class Program
    {
        // ---- Basit log yolları  ----
        static string GetInvoiceDir()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Invoice");
            Directory.CreateDirectory(dir);
            return dir;
        }
        static string GetStatusPath() => Path.Combine(GetInvoiceDir(), "status.txt");

        static void Info(string msg)
        {
            File.AppendAllText(GetStatusPath(), $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] INFO: {msg}\n");
        }
        static void Fail(string msg, int code)
        {
            File.AppendAllText(GetStatusPath(), $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] FAIL: {msg}\n");
            Environment.Exit(code);
        }

        // Dosya adı güvenli hale getirme (extension/yerleşik Replace'lere bulaşmadan)
        static string SanitizeFileName(string name)
        {
            var bad = Path.GetInvalidFileNameChars();
            var arr = name.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = 0; j < bad.Length; j++)
                {
                    if (arr[i] == bad[j]) { arr[i] = '_'; break; }
                }
            }
            return new string(arr);
        }

        static string GetCounterPath()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Invoice");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "MSP_counter.txt");
        }

        static string GetNextProformaNumber()
        {
            string today = DateTime.Now.ToString("yyyyMMdd");
            string path = GetCounterPath();

            int nextIndex = 1;
            Dictionary<string, int> dict = new Dictionary<string, int>();

            // varsa oku
            if (File.Exists(path))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        dict[parts[0]] = int.Parse(parts[1]);
                    }
                }
            }

            // bugünkü varsa artır, yoksa 1
            if (dict.ContainsKey(today))
            {
                nextIndex = dict[today] + 1;
                dict[today] = nextIndex;
            }
            else
            {
                nextIndex = 1;
                dict[today] = 1;
            }

            // dosyayı güncelle
            using (var sw = new StreamWriter(path, false))
            {
                foreach (var kv in dict)
                    sw.WriteLine(kv.Key + "=" + kv.Value);
            }

            // örn: MSP20251010_1
            return $"MSP{today}_{nextIndex}";
        }

        // ---- DTO ----
        class PdfOrderItemDto
        {
            public string ParcaAdi { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total { get; set; }
            public string SelectedMachine { get; set; }
        }

        // ---- ENTRY POINT ----
        static int Main(string[] args)
        {
            try
            {
                Info("=== GeneratePdf started ===");

                // 12 param bekleniyor
                if (args.Length < 12)
                    Fail("Eksik parametre. Beklenen=12. Sıra: CustomerName, Address, DeliveryAddress, Contact, Email, Phone, Country, Adjustment, ShippingFee, Total, JSON_ÜrünListesi, TaxOffice", 2);

                // 0..11
                string customerName = args[0] ?? "";
                string address = args[1] ?? "";
                string deliveryAddress = args[2] ?? "";
                string contact = args[3] ?? "";
                string email = args[4] ?? "";
                string phone = args[5] ?? "";
                string country = args[6] ?? "";
                string adjustmentText = args[7] ?? "";
                string shippingFee = args[8] ?? "";
                string total = args[9] ?? "";
                string jsonItems = args[10] ?? "";
                string taxoffice = args[11] ?? "";

                Info("Args alındı.");
                Info("country='" + country + "', taxoffice='" + taxoffice + "'");

                // Ürün listesi
                List<PdfOrderItemDto> items = null;
                try
                {
                    items = JsonConvert.DeserializeObject<List<PdfOrderItemDto>>(jsonItems);
                    if (items == null) Fail("JSON deserialize sonucu null.", 3);
                }
                catch (Exception ex)
                {
                    Fail("JSON deserialize hatası: " + ex.Message, 3);
                }
                Info("Items count: " + items.Count);

                // Şablon yolu
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string cl = (country ?? "").Trim().ToLowerInvariant();
                bool isTR = cl == "türkiye" || cl == "turkiye" || cl.StartsWith("türkiye") || cl == "tr";
                string sablonAdi = isTR ? "yurticisablon.xlsx" : "yurtdisisablon.xlsx";
                string sablonYolu = Path.Combine(exeDir, sablonAdi);
                if (!File.Exists(sablonYolu)) Fail("Şablon bulunamadı: " + sablonYolu, 4);

                // Çıktı PDF yolu
                string invoiceDir = GetInvoiceDir();
                string tarih = DateTime.Now.ToString("yyyy_MM_dd");
                string dosyaBase = SanitizeFileName(tarih + "_" + customerName);
                string pdfYolu = Path.Combine(invoiceDir, dosyaBase + ".pdf");
                int sc = 1;
                while (File.Exists(pdfYolu))
                {
                    pdfYolu = Path.Combine(invoiceDir, dosyaBase + "_(" + sc + ").pdf");
                    sc++;
                }

                Excel.Application excelApp = null;
                Excel.Workbook workbook = null;
                object sheetObj = null;

                try
                {
                    excelApp = new Excel.Application { Visible = false, DisplayAlerts = false };
                    workbook = excelApp.Workbooks.Open(sablonYolu);
                    sheetObj = workbook.Sheets[1];
                    dynamic sheet = sheetObj;

                    if (isTR)
                    {
                        Info("Şablon: YURT İÇİ");

                        // Sol blok
                        sheet.Cells[11, 4].Value2 = customerName;     // D11
                        sheet.Cells[12, 4].Value2 = contact;          // D12
                        sheet.Cells[13, 4].Value2 = address;          // D13
                        sheet.Cells[14, 4].Value2 = deliveryAddress;  // D14

                        // Sağ blok
                        sheet.Cells[11, 9].Value2 = DateTime.Now.ToString("dd/MM/yyyy"); // I11
                        sheet.Cells[14, 9].NumberFormat = "@";                            // I14
                        sheet.Cells[14, 9].Value2 = taxoffice;                            // I14 Vergi Dairesi
                        sheet.Cells[15, 9].Value2 = email;                                // I15
                        sheet.Cells[16, 9].Value2 = phone;                                // I16 (şablonda varsa)

                        // Kalemler
                        decimal toplamTutar = 0m;
                        int row = 18;
                        foreach (var it in items)
                        {
                            sheet.Cells[row, 2].Value2 = string.IsNullOrWhiteSpace(it.SelectedMachine) ? "GENEL" : it.SelectedMachine; // B
                            sheet.Cells[row, 4].Value2 = it.ParcaAdi;     // D
                            sheet.Cells[row, 8].Value2 = it.Quantity;     // H
                            sheet.Cells[row, 9].Value2 = it.UnitPrice;    // I
                            sheet.Cells[row, 10].Value2 = it.Total;        // J
                            toplamTutar += it.Total;
                            row++;
                        }

                        // TOPLAM (J28)
                        sheet.Cells[28, 10].NumberFormat = "@";
                        sheet.Cells[28, 10].Value2 = "₺ " + toplamTutar.ToString("0.00");

                        // KDV (I33/J33 koşullu)
                        decimal kdv = 0m;
                        decimal.TryParse(
                            (adjustmentText ?? "").Trim().Replace("%", "").Replace(",", "."),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out kdv
                        );

                        if (kdv > 0m)
                        {
                            decimal kdvliToplam = toplamTutar * (1 + (kdv / 100m));
                            sheet.Cells[33, 9].NumberFormat = "@";     // I33
                            sheet.Cells[33, 9].Value2 = "KDV";         // I33 => etiket
                            sheet.Cells[33, 10].NumberFormat = "@";     // J33
                            sheet.Cells[33, 10].Value2 = "₺ " + kdvliToplam.ToString("0.00");  // J33 => fiyat
                        }
                        else
                        {
                            sheet.Cells[33, 9].Value2 = null;  // I33
                            sheet.Cells[33, 10].Value2 = null;  // J33
                        }
                    }

                    else
                    {
                        Info("Şablon: YURT DIŞI");

                        sheet.Cells[7, 4].Value2 = customerName;
                        sheet.Cells[8, 4].Value2 = address;
                        sheet.Cells[9, 4].Value2 = deliveryAddress;
                        sheet.Cells[10, 4].Value2 = contact;
                        sheet.Cells[11, 4].Value2 = email;
                        sheet.Cells[7, 11].Value2 = DateTime.Now.ToString("dd/MM/yyyy");

                        string proformaNo = GetNextProformaNumber();
                        sheet.Cells[8, 11].Value2 = proformaNo;
                        sheet.Cells[11, 11].Value2 = phone;

                        int row = 16;
                        foreach (var it in items)
                        {
                            string machineCode = string.IsNullOrWhiteSpace(it.SelectedMachine) ? "GENERAL" : it.SelectedMachine;
                            sheet.Cells[row, 2].Value2 = machineCode + " / " + it.ParcaAdi;
                            sheet.Cells[row, 10].Value2 = it.Quantity;
                            sheet.Cells[row, 12].Value2 = it.UnitPrice;
                            sheet.Cells[row, 17].Value2 = it.Total;
                            row++;
                        }

                        // İNDİRİM (H25/P25/Q25)
                        decimal indirimYuzde = 0m;
                        decimal.TryParse(
                            (adjustmentText ?? "").Trim().Replace("%", "").Replace(",", "."),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out indirimYuzde
                        );

                        if (indirimYuzde > 0m)
                        {
                            sheet.Cells[25, 8].Value2 = "DISCOUNT";         // H25
                            sheet.Cells[25, 16].Value2 = ":";                // P25
                            sheet.Cells[25, 17].Value2 = "%" + indirimYuzde; // Q25
                        }
                        else
                        {
                            sheet.Cells[25, 8].Value2 = null; // H25
                            sheet.Cells[25, 16].Value2 = null; // P25
                            sheet.Cells[25, 17].Value2 = null; // Q25
                        }

                        // SHIPPING (H27/P27/Q27)
                        decimal kargo = 0m;
                        decimal.TryParse(
                            (shippingFee ?? "").Trim().Replace("₺", "").Replace("€", "").Replace(",", "."),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out kargo
                        );

                        if (kargo > 0m)
                        {
                            sheet.Cells[27, 8].Value2 = "SHIPPING"; // H27
                            sheet.Cells[27, 16].Value2 = ":";        // P27
                            sheet.Cells[27, 17].Value2 = kargo;      // Q27
                        }
                        else
                        {
                            sheet.Cells[27, 8].Value2 = null; // H27
                            sheet.Cells[27, 16].Value2 = null; // P27
                            sheet.Cells[27, 17].Value2 = null; // Q27
                        }

                        // GRAND TOTAL (Q28)
                        sheet.Cells[28, 17].Value2 = total;

                        // DİKKAT: Burada **sheet.Cells[27,17] = shippingFee;** YOK!
                    }

                    // Debug xlsx
                    string debugXlsx = Path.Combine(GetInvoiceDir(), "DEBUG_ProformaGor.xlsx");
                    try { workbook.SaveAs(debugXlsx); Info("Saved DEBUG_ProformaGor.xlsx"); } catch (Exception exSave) { Info("DEBUG Save error: " + exSave.Message); }

                    // PDF
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
                    Info("PDF exported: " + pdfYolu);

                    try { Process.Start(new ProcessStartInfo(pdfYolu) { UseShellExecute = true }); } catch { }
                }
                catch (Exception ex)
                {
                    Fail("Excel/Export hatası: " + ex.Message + "\nSTACK: " + ex.StackTrace, 5);
                }
                finally
                {
                    try { if (workbook != null) workbook.Close(false); } catch { }
                    try { if (excelApp != null) excelApp.Quit(); } catch { }

                    try { if (sheetObj != null) Marshal.ReleaseComObject(sheetObj); } catch { }
                    try { if (workbook != null) Marshal.ReleaseComObject(workbook); } catch { }
                    try { if (excelApp != null) Marshal.ReleaseComObject(excelApp); } catch { }
                }

                Info("=== GeneratePdf finished OK ===");
                return 0;
            }
            catch (Exception exTop)
            {
                try { File.AppendAllText(GetStatusPath(), $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] TOP_EX: {exTop.Message}\n{exTop.StackTrace}\n"); } catch { }
                return 99;
            }
        }
    }
}
