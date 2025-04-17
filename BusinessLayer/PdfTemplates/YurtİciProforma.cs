using EntityLayer.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.PdfTemplates
{
    public class YurtIciProforma : IDocument
    {
        private readonly Order _order;

        public YurtIciProforma(Order order)
        {
            _order = order;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(TextStyle.Default.FontFamily("Calibri").FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.ConstantItem(100).Element(c => c.Image("wwwroot/images/logo-msk.png"));
                row.RelativeItem().AlignCenter().Text("PROFORMA FATURA").FontSize(16).SemiBold();
                row.ConstantItem(100).Element(c => c.Image("wwwroot/images/logo-ozgenc.png"));
            });

            container.PaddingTop(5).Text($"FATURA : {_order.Customer.CustomerName} {_order.OrderDate:dd/MM/yyyy}");
            container.Text($"İLGİLİ KİŞİ: {_order.Customer.Contact}");
        }

        void ComposeContent(IContainer container)
        {
            var customer = _order.Customer;

            container.PaddingVertical(10).Column(col =>
            {
                col.Spacing(8);

                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(info =>
                    {
                        info.Item().Text($"TARİH: {_order.OrderDate:dd/MM/yyyy}");
                        info.Item().Text("FATURA NO: ");
                        info.Item().Text("VERGİ NO: ");
                        info.Item().Text("VERGİ DAİRESİ: ");
                        info.Item().Text($"MAİL ADRESİ: {customer.Email}");
                    });

                    row.RelativeItem().Column(info =>
                    {
                        info.Item().Text("ADRES:");
                        info.Item().Text($"{customer.Address} {customer.Address}");
                        info.Item().Text("SEVK ADRESİ:");
                        info.Item().Text($"{customer.DeliveryAddress ?? customer.DeliveryAddress}");
                    });
                });

                col.Item().Element(ComposeProductTable);
                col.Item().Element(ComposeTotals);
            });
        }

        void ComposeProductTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // No
                    columns.RelativeColumn(2);   // Stok Kod
                    columns.RelativeColumn(4);   // Detay
                    columns.ConstantColumn(50);  // Miktar
                    columns.ConstantColumn(70);  // Fiyat
                    columns.ConstantColumn(80);  // Toplam
                });

                table.Header(header =>
                {
                    header.Cell().Text("NO").SemiBold();
                    header.Cell().Text("STOK KODU").SemiBold();
                    header.Cell().Text("DETAY").SemiBold();
                    header.Cell().Text("MİKTAR").SemiBold();
                    header.Cell().Text("FİYAT").SemiBold();
                    header.Cell().Text("TOPLAM").SemiBold();
                });

                int i = 1;
                foreach (var item in _order.OrderItems)
                {
                    var total = item.Quantity * item.UnitPrice;
                    table.Cell().Text(i++.ToString());
                    table.Cell().Text("-"); // Stok Kodu boş
                    table.Cell().Text(i++.ToString());
                    var productNameToUse = string.IsNullOrWhiteSpace(item.CustomProductName)
                        ? item.Product.ParcaAdi
                        : item.CustomProductName;
                    table.Cell().Text(productNameToUse);
                    table.Cell().Text(item.Quantity.ToString());
                    table.Cell().Text(item.UnitPrice.ToString("C2", new CultureInfo("en-IE")));
                    table.Cell().Text(total.ToString("C2", new CultureInfo("en-IE")));
                }
            });
        }

        void ComposeTotals(IContainer container)
        {
            decimal total = _order.OrderItems.Sum(x => x.Quantity * x.UnitPrice);
            decimal kdv = total * 0.2m;
            decimal genelToplam = total + kdv;

            container.PaddingTop(10).Column(col =>
            {
                col.Item().AlignRight().Text($"TOPLAM: {total.ToString("C2", new CultureInfo("en-IE"))}");
                col.Item().AlignRight().Text($"KDV: {kdv.ToString("C2", new CultureInfo("en-IE"))}");
                col.Item().AlignRight().Text($"GENEL TOPLAM: {genelToplam.ToString("C2", new CultureInfo("en-IE"))}").Bold();
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.PaddingTop(15).Column(col =>
            {
                col.Item().Text("FİYATLARIMIZA KDV DAHİL DEĞİLDİR.").Italic();
                col.Item().PaddingTop(10).Text("BANKA BİLGİSİ").SemiBold();
                col.Item().Text("ZİRAAT BANKASI BURSA OSB GİRİŞİMCİ ŞUBESİ");
                col.Item().Text("HESAP SAHİBİ: M.S.K MOTOR KOMPRESÖR MAKİNA SAN.TİC.LTD.ŞTİ");
                col.Item().Text("IBAN: TR20 0001 0027 0155 9886 9350 09");
                col.Item().Text("HESAP NO: 2701-5598 8693 5009");
                col.Item().Text("SWIFT CODE: TCZBTR2A");
                col.Item().Text("TESLİM KOŞULLARI: Fabrikadan Teslim");
                col.Item().Text("SATIŞ KOŞULLARI: % 100 GÖNDERİMDEN ÖNCE");
                col.Item().PaddingTop(10).Text("Yetkili İmzası ve Şirket Mührü");
            });
        }
    }
}
