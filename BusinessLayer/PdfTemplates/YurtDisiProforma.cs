using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.PdfTemplates
{
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;
    using QuestPDF.Drawing;
    using QuestPDF.Elements;
    using System.Globalization;
    using EntityLayer.Entities.Enums;
    using EntityLayer.Entities;

    public class YurtDisiProforma : IDocument
    {
        private readonly Order _order;

        public YurtDisiProforma(Order order)
        {
            _order = order;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(TextStyle.Default.FontFamily("Calibri").FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeBody);
                page.Footer().Element(ComposeFooter);
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.ConstantItem(100).Element(c => c.Image("wwwroot/images/logo-murser.png"));
                row.RelativeItem().AlignCenter().Text("COMMERCIAL INVOICE", TextStyle.Default.FontFamily("Calibri").FontSize(16).SemiBold());
                row.ConstantItem(100).Element(c => c.Image("wwwroot/images/logo-ozgenc.png"));
            });
        }

        void ComposeBody(IContainer container)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Spacing(10);
                column.Item().Element(ComposeBuyerInfo);
                column.Item().Element(ComposeProductTable);
                column.Item().Element(ComposeBottomInfo);
            });
        }

        void ComposeBuyerInfo(IContainer container)
        {
            var customer = _order.Customer;

            container.Border(1).Padding(5).Column(col =>
            {
                col.Spacing(4);
                col.Item().Text($"BUYER: {customer.CustomerName}");
                col.Item().Text($"Address: {customer.Address} {customer.Address}");
                col.Item().Text($"Delivery Address: {customer.DeliveryAddress ?? customer.DeliveryAddress}");
                col.Item().Text($"Contact: {customer.Contact}");
                col.Item().Text($"Email: {customer.Email}");
                col.Item().Text($"Phone: {customer.PhoneNumber}");
            });
        }

        void ComposeProductTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.RelativeColumn(4);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(70);
                });

                table.Header(header =>
                {
                    header.Cell().Text("No").SemiBold();
                    header.Cell().Text("Details").SemiBold();
                    header.Cell().Text("Qty").SemiBold();
                    header.Cell().Text("Unit Price").SemiBold();
                    header.Cell().Text("Total").SemiBold();
                });

                int i = 1;
                foreach (var item in _order.OrderItems)
                {
                    var total = item.Quantity * item.UnitPrice;
                    table.Cell().Text(i++.ToString());
                    table.Cell().Text(item.Product.ParcaAdi);
                    table.Cell().Text(item.Quantity.ToString());
                    table.Cell().Text(item.UnitPrice.ToString("C2", new CultureInfo("fr-FR")));
                    table.Cell().Text(total.ToString("C2", new CultureInfo("fr-FR")));
                }
            });
        }

        void ComposeBottomInfo(IContainer container)
        {
            var shipping = _order.ShippingFee ?? 0;
            var discount = _order.AdjustmentPercentage ?? 0;
            var isDiscount = _order.AdjustmentType == PriceAdjustmentType.Discount;

            decimal itemsTotal = _order.OrderItems.Sum(x => x.Quantity * x.UnitPrice);
            decimal adjustmentAmount = isDiscount ? -itemsTotal * discount / 100 : itemsTotal * discount / 100;
            decimal grandTotal = itemsTotal + adjustmentAmount + shipping;

            container.PaddingTop(10).Column(col =>
            {
                col.Spacing(5);
                col.Item().Text($"Discount/Surcharge: {(isDiscount ? "-" : "+")}{discount}%");
                col.Item().Text($"Shipping: {shipping.ToString("C2", new CultureInfo("fr-FR"))}");
                col.Item().Text($"GRAND TOTAL: {grandTotal.ToString("C2", new CultureInfo("fr-FR"))}").Bold();

                col.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Text("SELLER\n\nAuthorized Signature and Company Stamp");
                    row.RelativeItem().Text("BUYER\n\nAuthorized Signature and Company Stamp");
                });

                col.Item().PaddingTop(15).Text("BANK INFORMATION").SemiBold();
                col.Item().Text("Bank: ZIRAAT BANKASI A.S. | Branch: Bursa OSB Girisimci Sube");
                col.Item().Text("Account (EUR): 2701-56483856-5009 | IBAN: TR310001002701564838565009");
                col.Item().Text("SWIFT CODE: TCZBTR2A");
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text("MURSER DIS TICARET LTD. STI. - Bursa, Turkey - www.ozgencmachine.com").FontSize(9).Italic();
        }
    }


}
