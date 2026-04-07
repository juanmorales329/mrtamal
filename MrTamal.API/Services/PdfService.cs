using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MrTamal.Shared.DTOs;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace MrTamal.API.Services;

public class PdfService
{
    public byte[] GenerarProyectadoPdf(ResumenProyectado r, string simbolo)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text($"🎯 Proyectado de Ventas {r.Anio}").FontSize(16).Bold().AlignCenter();
                    col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).AlignCenter().FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(8).Column(col =>
                {
                    // KPIs
                    col.Item().Background(Colors.Orange.Lighten4).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Column(c => { c.Item().Text("Meta Anual").Bold(); c.Item().Text($"{simbolo} {r.MetaAnual:N2}").FontSize(12).FontColor(Colors.Orange.Darken2); });
                        row.RelativeItem().Column(c => { c.Item().Text("Venta Real").Bold(); c.Item().Text($"{simbolo} {r.VentaReal:N2}").FontSize(12).FontColor(Colors.Green.Darken2); });
                        row.RelativeItem().Column(c => { c.Item().Text("Cumplimiento").Bold(); c.Item().Text($"{r.PorcentajeCumplimiento}%").FontSize(12).FontColor(r.PorcentajeCumplimiento >= 100 ? Colors.Green.Darken2 : Colors.Red.Darken2); });
                        row.RelativeItem().Column(c => { c.Item().Text("Días Lab.").Bold(); c.Item().Text($"{r.DiasLaborables}").FontSize(12).FontColor(Colors.Blue.Darken2); });
                    });

                    // Cuatrimestres
                    if (r.Cuatrimestres?.Any() == true)
                    {
                        col.Item().PaddingTop(12).Text("CONTROL POR CUATRIMESTRE").Bold().FontSize(11).FontColor(Colors.Orange.Darken2);
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(30); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(1); });
                            t.Header(h => { foreach (var hh in new[] { "#", "Período", "Meta", "Real", "Diferencia", "%" }) h.Cell().Background(Colors.Orange.Lighten3).Padding(4).Text(hh).Bold(); });
                            foreach (var c in r.Cuatrimestres)
                            {
                                t.Cell().Padding(4).Text($"{c.Numero}°");
                                t.Cell().Padding(4).Text(c.Periodo);
                                t.Cell().Padding(4).Text($"{simbolo} {c.Meta:N0}");
                                t.Cell().Padding(4).Text($"{simbolo} {c.VentaReal:N0}").FontColor(Colors.Green.Darken2);
                                t.Cell().Padding(4).Text($"{(c.Diferencia >= 0 ? "+" : "")}{simbolo} {c.Diferencia:N0}").FontColor(c.Diferencia >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                                t.Cell().Padding(4).Text($"{c.PorcentajeCumplimiento}%").FontColor(c.PorcentajeCumplimiento >= 100 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            }
                        });
                    }

                    // Desglose mensual
                    col.Item().PaddingTop(12).Text("DESGLOSE MENSUAL").Bold().FontSize(11).FontColor(Colors.Grey.Darken2);
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(2); c.ConstantColumn(40); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); });
                        t.Header(h => { foreach (var hh in new[] { "Mes", "Días", "Meta", "Real", "Diferencia", "Estado" }) h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(hh).Bold(); });
                        foreach (var m in r.DesgloseMensual)
                        {
                            t.Cell().Padding(4).Text(m.NombreMes).Bold();
                            t.Cell().Padding(4).Text($"{m.DiasLaborables}");
                            t.Cell().Padding(4).Text($"{simbolo} {m.MetaMes:N0}");
                            t.Cell().Padding(4).Text($"{simbolo} {m.VentaReal:N0}").FontColor(Colors.Green.Darken2);
                            t.Cell().Padding(4).Text($"{(m.Diferencia >= 0 ? "+" : "")}{simbolo} {m.Diferencia:N0}").FontColor(m.Diferencia >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            t.Cell().Padding(4).Text(m.VentaReal == 0 && m.MetaMes > 0 ? "Pendiente" : m.Cumplido ? "✓ Cumplido" : "✗ Bajo meta")
                                .FontColor(m.VentaReal == 0 ? Colors.Grey.Medium : m.Cumplido ? Colors.Green.Darken2 : Colors.Red.Darken2);
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x => { x.Span("Página "); x.CurrentPageNumber(); x.Span(" de "); x.TotalPages(); });
            });
        }).GeneratePdf();
    }

    public byte[] GenerarProyectadoExcel(ResumenProyectado r, string simbolo)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var pkg = new ExcelPackage();

        // Hoja cuatrimestres
        if (r.Cuatrimestres?.Any() == true)
        {
            var ws = pkg.Workbook.Worksheets.Add("Cuatrimestres");
            var headers = new[] { "#", "Período", "Meta", "Real", "Diferencia", "Cumplimiento %" };
            for (int i = 0; i < headers.Length; i++) { ws.Cells[1, i + 1].Value = headers[i]; ws.Cells[1, i + 1].Style.Font.Bold = true; ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 109, 0)); ws.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White); }
            int row = 2;
            foreach (var c in r.Cuatrimestres)
            {
                ws.Cells[row, 1].Value = $"{c.Numero}°"; ws.Cells[row, 2].Value = c.Periodo;
                ws.Cells[row, 3].Value = c.Meta; ws.Cells[row, 4].Value = c.VentaReal;
                ws.Cells[row, 5].Value = c.Diferencia; ws.Cells[row, 6].Value = (double)c.PorcentajeCumplimiento;
                for (int col = 3; col <= 5; col++) ws.Cells[row, col].Style.Numberformat.Format = $"\"{simbolo}\" #,##0.00";
                ws.Cells[row, 6].Style.Numberformat.Format = "0.0\"%\"";
                row++;
            }
            ws.Cells[ws.Dimension.Address].AutoFitColumns();
        }

        // Hoja mensual
        var wsM = pkg.Workbook.Worksheets.Add("Desglose Mensual");
        var hM = new[] { "Mes", "Días Lab.", "Meta", "Real", "Diferencia", "Estado" };
        for (int i = 0; i < hM.Length; i++) { wsM.Cells[1, i + 1].Value = hM[i]; wsM.Cells[1, i + 1].Style.Font.Bold = true; wsM.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid; wsM.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(50, 50, 50)); wsM.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White); }
        int rowM = 2;
        foreach (var m in r.DesgloseMensual)
        {
            wsM.Cells[rowM, 1].Value = m.NombreMes; wsM.Cells[rowM, 2].Value = m.DiasLaborables;
            wsM.Cells[rowM, 3].Value = m.MetaMes; wsM.Cells[rowM, 4].Value = m.VentaReal;
            wsM.Cells[rowM, 5].Value = m.Diferencia;
            wsM.Cells[rowM, 6].Value = m.VentaReal == 0 && m.MetaMes > 0 ? "Pendiente" : m.Cumplido ? "Cumplido" : "Bajo meta";
            for (int col = 3; col <= 5; col++) wsM.Cells[rowM, col].Style.Numberformat.Format = $"\"{simbolo}\" #,##0.00";
            rowM++;
        }
        wsM.Cells[wsM.Dimension.Address].AutoFitColumns();

        return pkg.GetAsByteArray();
    }

    public byte[] GenerarReporteExcel(ReporteResumen reporte, string titulo, string simbolo)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var pkg = new ExcelPackage();

        void FillSheet(ExcelWorksheet ws, List<ReporteDetalle> items, Color headerColor)
        {
            var headers = new[] { "Fecha", "Código", "Descripción", "Cantidad", "Notas" };
            for (int i = 0; i < headers.Length; i++) { ws.Cells[1, i + 1].Value = headers[i]; ws.Cells[1, i + 1].Style.Font.Bold = true; ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(headerColor); ws.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White); }
            int r = 2;
            foreach (var item in items)
            {
                ws.Cells[r, 1].Value = item.Fecha.ToString("dd/MM/yyyy"); ws.Cells[r, 2].Value = item.Codigo;
                ws.Cells[r, 3].Value = item.Descripcion; ws.Cells[r, 4].Value = item.Cantidad;
                ws.Cells[r, 4].Style.Numberformat.Format = $"\"{simbolo}\" #,##0.00";
                ws.Cells[r, 5].Value = item.Notas ?? ""; r++;
            }
            if (items.Any()) ws.Cells[ws.Dimension.Address].AutoFitColumns();
        }

        FillSheet(pkg.Workbook.Worksheets.Add("Ingresos"), reporte.Ingresos, Color.FromArgb(46, 125, 50));
        FillSheet(pkg.Workbook.Worksheets.Add("Egresos"), reporte.Egresos, Color.FromArgb(198, 40, 40));

        if (reporte.Comparaciones?.Any() == true)
        {
            var wsC = pkg.Workbook.Worksheets.Add("Comparación");
            var hC = new[] { "Período", "Ingresos", "Egresos", "Balance" };
            for (int i = 0; i < hC.Length; i++) { wsC.Cells[1, i + 1].Value = hC[i]; wsC.Cells[1, i + 1].Style.Font.Bold = true; wsC.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid; wsC.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(33, 150, 243)); wsC.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White); }
            int r = 2;
            foreach (var c in reporte.Comparaciones)
            {
                wsC.Cells[r, 1].Value = c.Periodo; wsC.Cells[r, 2].Value = c.TotalIngresos; wsC.Cells[r, 3].Value = c.TotalEgresos; wsC.Cells[r, 4].Value = c.Balance;
                for (int col = 2; col <= 4; col++) wsC.Cells[r, col].Style.Numberformat.Format = $"\"{simbolo}\" #,##0.00";
                r++;
            }
            wsC.Cells[wsC.Dimension.Address].AutoFitColumns();
        }

        return pkg.GetAsByteArray();
    }

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
