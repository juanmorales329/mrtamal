using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MrTamal.Shared.DTOs;

namespace MrTamal.API.Services;

public class PdfService
{
    public byte[] GenerarReportePdf(ReporteResumen reporte, string titulo)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(titulo).FontSize(18).Bold().AlignCenter();
                    col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).AlignCenter().FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    // Resumen
                    col.Item().Background(Colors.Blue.Lighten4).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Total Ingresos").Bold();
                            c.Item().Text($"Q {reporte.TotalIngresos:N2}").FontColor(Colors.Green.Darken2).FontSize(14);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Total Egresos").Bold();
                            c.Item().Text($"Q {reporte.TotalEgresos:N2}").FontColor(Colors.Red.Darken2).FontSize(14);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Balance").Bold();
                            var color = reporte.Balance >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                            c.Item().Text($"Q {reporte.Balance:N2}").FontColor(color).FontSize(14);
                        });
                    });

                    col.Item().PaddingTop(15).Text("INGRESOS").Bold().FontSize(12).FontColor(Colors.Green.Darken2);
                    col.Item().Table(t => BuildTable(t, reporte.Ingresos));

                    col.Item().PaddingTop(15).Text("EGRESOS").Bold().FontSize(12).FontColor(Colors.Red.Darken2);
                    col.Item().Table(t => BuildTable(t, reporte.Egresos));

                    if (reporte.Comparaciones?.Any() == true)
                    {
                        col.Item().PaddingTop(15).Text("COMPARACIÓN").Bold().FontSize(12);
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });
                            t.Header(h =>
                            {
                                foreach (var header in new[] { "Período", "Ingresos", "Egresos", "Balance" })
                                    h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(header).Bold();
                            });
                            foreach (var comp in reporte.Comparaciones)
                            {
                                t.Cell().Padding(4).Text(comp.Periodo);
                                t.Cell().Padding(4).Text($"Q {comp.TotalIngresos:N2}").FontColor(Colors.Green.Darken2);
                                t.Cell().Padding(4).Text($"Q {comp.TotalEgresos:N2}").FontColor(Colors.Red.Darken2);
                                var balColor = comp.Balance >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                                t.Cell().Padding(4).Text($"Q {comp.Balance:N2}").FontColor(balColor);
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static void BuildTable(TableDescriptor t, List<ReporteDetalle> items)
    {
        t.ColumnsDefinition(c =>
        {
            c.ConstantColumn(80);
            c.ConstantColumn(60);
            c.RelativeColumn(2);
            c.RelativeColumn();
            c.RelativeColumn(2);
        });
        t.Header(h =>
        {
            foreach (var header in new[] { "Fecha", "Código", "Descripción", "Cantidad", "Notas" })
                h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(header).Bold();
        });
        foreach (var item in items)
        {
            t.Cell().Padding(4).Text(item.Fecha.ToString("dd/MM/yyyy"));
            t.Cell().Padding(4).Text(item.Codigo);
            t.Cell().Padding(4).Text(item.Descripcion);
            t.Cell().Padding(4).Text($"Q {item.Cantidad:N2}");
            t.Cell().Padding(4).Text(item.Notas ?? "");
        }
        if (!items.Any())
            t.Cell().ColumnSpan(5).Padding(4).Text("Sin registros").FontColor(Colors.Grey.Medium).Italic();
    }
}
