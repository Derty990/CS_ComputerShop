using Firma.Data.Data.Customers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firma.PortalWWW.Documents
{
    public class InvoiceDocument : IDocument
    {
        private readonly Order _order;

        public InvoiceDocument(Order order)
        {
            _order = order;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Strona ");
                    x.CurrentPageNumber();
                });
            });
        }

        void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"Faktura nr FV/{_order.IdOrder}/{DateTime.Now.Year}").Style(titleStyle);
                    column.Item().Text($"Data wystawienia: {DateTime.Now:d MMMM yyyy}");
                    column.Item().Text($"Data zamówienia: {_order.OrderDate:d MMMM yyyy}");
                });
                row.ConstantItem(100).Height(50).Placeholder("Logo"); // Miejsce na logo
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Spacing(20);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("Sprzedawca").SemiBold();
                        column.Item().Text("Vivitech Sp. z o.o.");
                        column.Item().Text("ul. Komputerowa 1, 00-001 Warszawa");
                        column.Item().Text("NIP: 123-456-78-90");
                    });
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("Nabywca").SemiBold();
                        column.Item().Text($"{_order.FirstName} {_order.LastName}");
                        column.Item().Text(_order.Address);
                        column.Item().Text($"{_order.PostalCode} {_order.City}");
                    });
                });

                column.Item().Element(ComposeTable);
                column.Item().AlignRight().Text($"Do zapłaty: {_order.TotalAmount:C}").Bold();
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Text("Produkt");
                    header.Cell().AlignCenter().Text("Ilość");
                    header.Cell().AlignRight().Text("Cena jedn.");
                    header.Cell().AlignRight().Text("Wartość");
                });

                foreach (var item in _order.OrderItems)
                {
                    table.Cell().Text(item.Product?.Name ?? "B/D");
                    table.Cell().AlignCenter().Text(item.Quantity);
                    table.Cell().AlignRight().Text($"{item.UnitPrice:C}");
                    table.Cell().AlignRight().Text($"{(item.Quantity * item.UnitPrice):C}");
                }
            });
        }
    }
}