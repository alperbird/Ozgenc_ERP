using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.PdfTemplates
{
    using System.Globalization;
    using EntityLayer.Entities.Enums;
    using EntityLayer.Entities;
    using System.IO.Compression;
    using System.Xml;

    public class YurtDisiProforma
    {
        public string TemplatePath => "Templates/yurtdisisablon.ods";
        public string OutputDirectory => "GeneratedProformas";

        public static void Fill(Order order, string filePath)
        {
            string tempDir = Path.Combine(Path.GetDirectoryName(filePath)!, "temp_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);
            ZipFile.ExtractToDirectory(filePath, tempDir);

            string contentXml = Path.Combine(tempDir, "content.xml");
            var doc = new XmlDocument();
            doc.Load(contentXml);

            var nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
            nsMgr.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");

            FindCellByText(doc, nsMgr, "TOTAL");
            FindCellByText(doc, nsMgr, "GENERAL TOTAL");
            FindCellByText(doc, nsMgr, "SHIPPING");
            FindCellByText(doc, nsMgr, "GRAND TOTAL");

            var table = doc.SelectSingleNode("//table:table", nsMgr);
            var rows = table.SelectNodes("table:table-row", nsMgr);


            // Müşteri bilgileri
            ReplaceCell(doc, nsMgr, 6, 3, order.Customer.CustomerName); // D7
            ReplaceCell(doc, nsMgr, 7, 3, order.Customer.Address);      // D8
            ReplaceCell(doc, nsMgr, 8, 3, order.Customer.DeliveryAddress); // D9
            ReplaceCell(doc, nsMgr, 9, 3, order.Customer.Contact);      // D10
            ReplaceCell(doc, nsMgr, 10, 3, order.Customer.Email);       // D11
            ReplaceCell(doc, nsMgr, 6, 9, DateTime.Now.ToString("dd/MM/yyyy")); // K7
            ReplaceCell(doc, nsMgr, 7, 7, order.Customer.PhoneNumber);         // K8

            // Ürünler ve hesaplamalar
            int row = 15;
            int productIndex = 1;
            foreach (var item in order.OrderItems)
            {
                string name = string.IsNullOrWhiteSpace(item.CustomProductName)
                    ? item.Product.ParcaAdi
                    : item.CustomProductName;
                ReplaceCell(doc, nsMgr, row, 0, productIndex.ToString()); 
                ReplaceCell(doc, nsMgr, row, 1, name); // Ürün Adı
                ReplaceCell(doc, nsMgr, row, 2, item.Quantity.ToString()); // Adet
                ReplaceCell(doc, nsMgr, row, 3, item.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture)); // Birim Fiyat
                ReplaceCell(doc, nsMgr, row, 4, item.TotalPrice.ToString("0.00", CultureInfo.InvariantCulture)); // Toplam Fiyat
                row++;
                productIndex++;
                Console.WriteLine($"Ürün: {name} - Adet: {item.Quantity} - Fiyat: {item.UnitPrice}");
            }


            // Tutar bilgileri doğrudan TotalPrice üzerinden
            //ReplaceCell(doc, nsMgr, 28, 13, order.AdjustmentType == PriceAdjustmentType.Discount
            //    ? $"-{order.AdjustmentPercentage}%"
            //    : $"+{order.AdjustmentPercentage}%");
            //ReplaceCell(doc, nsMgr, 31, 13, (order.ShippingFee ?? 0).ToString("0.00", CultureInfo.InvariantCulture));
            //ReplaceCell(doc, nsMgr, 32, 13, order.TotalPrice.ToString("0.00", CultureInfo.InvariantCulture));

            ReplaceCell(doc, nsMgr, 24, 1, order.OrderItems.Sum(i => i.Quantity * i.UnitPrice).ToString("0.00", CultureInfo.InvariantCulture));

            // N29: Genel Tutar (İndirimli veya zamlı)
            decimal itemsTotal = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);
            decimal adjustment = 0;
            if (order.AdjustmentType == PriceAdjustmentType.Discount)
                adjustment = -(itemsTotal * (order.AdjustmentPercentage ?? 0) / 100);
            else if (order.AdjustmentType == PriceAdjustmentType.Increase)
                adjustment = (itemsTotal * (order.AdjustmentPercentage ?? 0) / 100);
            decimal generalTotal = itemsTotal + adjustment;
            ReplaceCell(doc, nsMgr, 26, 1, generalTotal.ToString("0.00", CultureInfo.InvariantCulture));

            // N30: Kargo Ücreti
            ReplaceCell(doc, nsMgr, 27, 1, (order.ShippingFee ?? 0).ToString("0.00", CultureInfo.InvariantCulture));

            // N31: Grand Total
            decimal grandTotal = generalTotal + (order.ShippingFee ?? 0);
            ReplaceCell(doc, nsMgr, 28, 1, grandTotal.ToString("0.00", CultureInfo.InvariantCulture));

            doc.Save(contentXml);

            if (File.Exists(filePath))
                File.Delete(filePath);

            ZipFile.CreateFromDirectory(tempDir, filePath);
            Directory.Delete(tempDir, true);
        }

        //private static void ReplaceCell(XmlDocument doc, XmlNamespaceManager nsMgr, int rowIndex, int colIndex, string value)
        //{
        //    var table = doc.SelectSingleNode("//table:table", nsMgr);
        //    if (table == null) return;

        //    var rows = table.SelectNodes("table:table-row", nsMgr);
        //    if (rows == null || rowIndex >= rows.Count) return;

        //    var row = rows[rowIndex];
        //    var cells = row.SelectNodes("table:table-cell", nsMgr);
        //    if (cells == null || colIndex >= cells.Count) return;

        //    var cell = cells[colIndex];
        //    var text = cell.SelectSingleNode("text:p", nsMgr);

        //    if (text == null)
        //    {
        //        text = doc.CreateElement("text:p", nsMgr.LookupNamespace("text"));
        //        cell.AppendChild(text);
        //    }

        //    text.InnerText = value;
        //}

        private static void ReplaceCell(XmlDocument doc, XmlNamespaceManager nsMgr, int rowIndex, int colIndex, string value)
        {
            var table = doc.SelectSingleNode("//table:table", nsMgr);
            if (table == null) return;

            var rows = table.SelectNodes("table:table-row", nsMgr);
            if (rows == null || rowIndex >= rows.Count) return;

            var row = rows[rowIndex];
            var cells = row.SelectNodes("table:table-cell", nsMgr);
            if (cells == null || colIndex >= cells.Count) return;

            var cell = cells[colIndex];

            // Hücre içindeki tüm <text:p> düğümlerini temizle
            foreach (XmlNode child in cell.ChildNodes)
            {
                if (child.Name == "text:p")
                {
                    cell.RemoveChild(child);
                }
            }

            // Yeni <text:p> oluştur
            var textNode = doc.CreateElement("text:p", nsMgr.LookupNamespace("text"));
            textNode.InnerText = value;
            cell.AppendChild(textNode);

            // Hücre türünü string olarak belirt (bu kısmı unutursan sayı gelmez)
            cell.Attributes.RemoveNamedItem("office:value-type");
            var valueTypeAttr = doc.CreateAttribute("office", "value-type", nsMgr.LookupNamespace("office"));
            valueTypeAttr.Value = "string";
            cell.Attributes.Append(valueTypeAttr);
        }

        private static void FindCellByText(XmlDocument doc, XmlNamespaceManager nsMgr, string targetText)
        {
            var rows = doc.SelectNodes("//table:table-row", nsMgr);
            if (rows == null) return;

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var cells = row.SelectNodes("table:table-cell", nsMgr);
                if (cells == null) continue;

                int colIndex = 0;

                foreach (XmlNode cell in cells)
                {
                    int repeat = 1;
                    var repeatAttr = cell.Attributes?["table:number-columns-repeated"];
                    if (repeatAttr != null)
                        repeat = int.Parse(repeatAttr.Value);

                    var textNode = cell.SelectSingleNode("text:p", nsMgr);
                    if (textNode != null && textNode.InnerText.Trim().Equals(targetText, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Bulundu: '{targetText}' → Satır {rowIndex}, Sütun {colIndex}");
                    }

                    colIndex += repeat;
                }
            }
        }

        public string GetOutputFileName(string customerName, DateTime orderDate)
        {
            var safeName = string.Join("_", customerName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(OutputDirectory, $"{safeName}_{orderDate:dd_MM_yyyy}.ods");
        }
    }

}

