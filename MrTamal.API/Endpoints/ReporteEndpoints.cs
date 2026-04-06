using System.Security.Claims;
using MrTamal.API.Services;
using MrTamal.Shared.DTOs;

namespace MrTamal.API.Endpoints;

public static class ReporteEndpoints
{
    public static void MapReporteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reportes").WithTags("Reportes").RequireAuthorization();

        group.MapPost("/", async (ReporteRequest req, ReporteService reporteSvc, ClaimsPrincipal user) =>
        {
            var uid = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reporte = await reporteSvc.GenerarAsync(req, uid);
            return Results.Ok(reporte);
        });

        group.MapPost("/pdf", async (ReporteRequest req, ReporteService reporteSvc, PdfService pdfSvc, ClaimsPrincipal user) =>
        {
            var uid = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reporte = await reporteSvc.GenerarAsync(req, uid);
            var titulo = req.Tipo switch
            {
                TipoReporte.Diario => $"Reporte Diario - {DateTime.Now:dd/MM/yyyy}",
                TipoReporte.Semanal => $"Reporte Semanal - Semana {GetWeekNumber(DateTime.Now)}",
                TipoReporte.Mensual => $"Reporte Mensual - {DateTime.Now:MMMM yyyy}",
                TipoReporte.ComparacionAnual => $"Comparación Anual {req.Anio} vs {req.AnioComparacion}",
                TipoReporte.ComparacionMensual => $"Comparación Mensual por Mes - {req.Anio}",
                _ => "Reporte"
            };
            var pdf = pdfSvc.GenerarReportePdf(reporte, titulo);
            return Results.File(pdf, "application/pdf", $"reporte_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        });
    }

    private static int GetWeekNumber(DateTime date)
    {
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }
}
