using BusinessLayer.Interfaces;
using BusinessLayer.PdfTemplates;
using EntityLayer.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class ProformaService : IProformaService
    {
        public async Task<string> GenerateProformaAsync(Order order)
        {
            string customerName = order.Customer.CustomerName;
            string tarihFormatli = DateTime.Now.ToString("dd_MM_yyyy");
            string dosyaAdiBase = $"{customerName}_{tarihFormatli}";

            string masaustu = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string invoiceKlasoru = Path.Combine(masaustu, "Invoice");
            Directory.CreateDirectory(invoiceKlasoru);

            string pdfYolu = Path.Combine(invoiceKlasoru, dosyaAdiBase + ".pdf");
            int sayac = 1;
            while (File.Exists(pdfYolu))
            {
                pdfYolu = Path.Combine(invoiceKlasoru, $"{dosyaAdiBase}_({sayac}).pdf");
                sayac++;
            }

            // Ülkeye göre şablon seçimi
            string ulke = order.Customer.Country?.CountryName?.Trim().ToLower() ?? "";

            IDocument belge;
            if (ulke != "türkiye")
            {
                belge = new YurtDisiProforma(order);
            }
            else
            {
                belge = new YurtIciProforma(order);
            }

            belge.GeneratePdf(pdfYolu);
            return pdfYolu;
        }
    }
}
