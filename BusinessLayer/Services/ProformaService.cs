using BusinessLayer.Interfaces;
using BusinessLayer.PdfTemplates;
using EntityLayer.Entities;
using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BusinessLayer.Services
{
    public class ProformaService : IProformaService
    {
        private readonly YurtDisiProforma _yurtDisi = new();
        private readonly YurtIciProforma _yurtIci = new();

        public async Task<string> GenerateProformaAsync(Order order)
        {
            bool isYurtdisi = !IsYurtIci(order.Customer.CountryName.CountryName);



            var templateName = isYurtdisi ? "yurtdisisablon.ods" : "yurticisablon.ods";
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "PdfTemplates", templateName);

            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedProformas");
            Directory.CreateDirectory(outputDir);

            var fileName = $"{order.Customer.CustomerName}_{order.OrderDate:dd_MM_yyyy}.ods";
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(outputDir, fileName);

            // Eğer aynı adla dosya varsa sil
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            // Şablonu kopyala
            System.IO.File.Copy(templatePath, filePath);

            if (isYurtdisi)
                YurtDisiProforma.Fill(order, filePath);
            else
                YurtIciProforma.Fill(order, filePath);

            // LibreOffice ile PDF'e çevir
            string sofficePath = GetSofficePath();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = sofficePath,
                    Arguments = $"--headless --convert-to pdf \"{filePath}\" --outdir \"{outputDir}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            var pdfPath = Path.ChangeExtension(filePath, ".pdf");
            return pdfPath;
        }

        private void ReplaceCell(XmlDocument doc, string cellRef, string newValue)
        {
            var node = doc.SelectSingleNode($"//table:table-cell[@table:cell-address='{cellRef}']/text:p", GetNamespaceManager(doc));
            if (node != null)
                node.InnerText = newValue;
        }

        private XmlNamespaceManager GetNamespaceManager(XmlDocument doc)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
            nsmgr.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
            return nsmgr;
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

        private static bool IsYurtIci(string? countryName)
        {
            return string.Equals(countryName?.Trim(), "türkiye", StringComparison.OrdinalIgnoreCase);
        }
    }

   
}
