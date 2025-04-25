using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BusinessLayer.PdfTemplates
{
    public class YurtIciProforma
    {
        public static void Fill(Order order, string filePath)
        {
            string tempDir = Path.Combine(Path.GetDirectoryName(filePath)!, "temp_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);
            ZipFile.ExtractToDirectory(filePath, tempDir);

            string contentXml = Path.Combine(tempDir, "content.xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(contentXml);

            ReplaceCell(doc, "D11", order.Customer.CustomerName);
            ReplaceCell(doc, "D12", order.Customer.Contact);
            ReplaceCell(doc, "D13", order.Customer.Address);
            ReplaceCell(doc, "D14", order.Customer.DeliveryAddress);
            ReplaceCell(doc, "I11", order.OrderDate.ToString("dd/MM/yyyy"));
            ReplaceCell(doc, "I15", order.Customer.Email);

            int row = 18;
            decimal total = 0;

            foreach (var item in order.OrderItems)
            {
                string productName = string.IsNullOrWhiteSpace(item.CustomProductName)
                    ? item.Product.ParcaAdi
                    : item.CustomProductName;

                ReplaceCell(doc, $"D{row}", productName);
                ReplaceCell(doc, $"H{row}", item.Quantity.ToString());
                ReplaceCell(doc, $"I{row}", item.UnitPrice.ToString(CultureInfo.InvariantCulture));

                decimal lineTotal = item.Quantity * item.UnitPrice;
                ReplaceCell(doc, $"J{row}", lineTotal.ToString(CultureInfo.InvariantCulture));
                total += lineTotal;

                row++;
            }

            ReplaceCell(doc, "J24", total.ToString(CultureInfo.InvariantCulture));
            decimal totalWithKdv = total * 1.2m;
            ReplaceCell(doc, "J29", totalWithKdv.ToString(CultureInfo.InvariantCulture));

            doc.Save(contentXml);

            if (File.Exists(filePath))
                File.Delete(filePath);

            ZipFile.CreateFromDirectory(tempDir, filePath);
            Directory.Delete(tempDir, true);
        }

        private static void ReplaceCell(XmlDocument doc, string cellRef, string newValue)
        {
            var node = doc.SelectSingleNode($"//table:table-cell[@table:cell-address='{cellRef}']/text:p", GetNamespaceManager(doc));
            if (node != null)
                node.InnerText = newValue;
        }

        private static XmlNamespaceManager GetNamespaceManager(XmlDocument doc)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
            nsmgr.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
            return nsmgr;
        }


    }

}
