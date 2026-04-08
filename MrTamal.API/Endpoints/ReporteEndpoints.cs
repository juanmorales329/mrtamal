using System.Security.Claims;
using MrTamal.API.Services;
using MrTamal.Shared.DTOs;

namespace MrTamal.API.Endpoints;

public static class ReporteEndpoints
{
    public static void MapReporteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reportes").WithTags("Reportes").RequireAuthorization();

        group.MapPost("/", async (ReporteRequest req, ReporteService reporteSvc, ClaimsPrincipal user, HttpContext ctx) =>
        {
            var uid = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var sucursalId = GetSucursalIdFromHeader(ctx);
            var reporte = await reporteSvc.GenerarAsync(req, uid, sucursalId);
            return Results.Ok(reporte);
        });

        group.MapPost("/pdf", async (ReporteRequest req, ReporteService reporteSvc, PdfService pdfSvc, ClaimsPrincipal user, HttpContext ctx) =>
        {
            var uid = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var sucursalId = GetSucursalIdFromHeader(ctx);
            var reporte = await reporteSvc.GenerarAsync(req, uid, sucursalId);
            var titulo = req.Tipo switch
            {
                TipoReporte.Diario => $"Reporte Diario - {req.FechaInicio?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy")}",
                TipoReporte.Semanal => $"Reporte Semanal - {req.FechaInicio?.ToString("dd/MM/yyyy")} al {req.FechaFin?.ToString("dd/MM/yyyy")}",
                TipoReporte.Mensual => req.FechaInicio.HasValue ? $"Reporte Mensual - {req.FechaInicio.Value:MMMM yyyy}" : $"Reporte Mensual - {DateTime.Now:MMMM yyyy}",
                TipoReporte.ComparacionAnual => $"Comparación Anual {req.Anio} vs {req.AnioComparacion}",
                TipoReporte.ComparacionMensual => $"Comparación Mensual por Mes - {req.Anio}",
                _ => "Reporte"
            };
            var pdf = pdfSvc.GenerarReportePdf(reporte, titulo, req.Simbolo ?? "$");
            return Results.File(pdf, "application/pdf", $"reporte_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        });

        group.MapPost("/excel", async (ReporteRequest req, ReporteService reporteSvc, PdfService pdfSvc, ClaimsPrincipal user, HttpContext ctx) =>
        {
            var uid = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var sucursalId = GetSucursalIdFromHeader(ctx);
            var reporte = await reporteSvc.GenerarAsync(req, uid, sucursalId);
            var titulo = req.Tipo switch
            {
                TipoReporte.Diario => $"Reporte Diario - {req.FechaInicio?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy")}",
                TipoReporte.Semanal => $"Reporte Semanal - {req.FechaInicio?.ToString("dd/MM/yyyy")} al {req.FechaFin?.ToString("dd/MM/yyyy")}",
                TipoReporte.Mensual => req.FechaInicio.HasValue ? $"Reporte Mensual - {req.FechaInicio.Value:MMMM yyyy}" : $"Reporte Mensual - {DateTime.Now:MMMM yyyy}",
                TipoReporte.ComparacionAnual => $"Comparación Anual {req.Anio} vs {req.AnioComparacion}",
                TipoReporte.ComparacionMensual => $"Comparación Mensual por Mes - {req.Anio}",
                _ => "Reporte"
            };
            var excel = pdfSvc.GenerarReporteExcel(reporte, titulo, req.Simbolo ?? "$");
            return Results.File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"reporte_{DateTime.Now:yyyyMMdd}.xlsx");
        });
    }

    private static int? GetSucursalIdFromHeader(HttpContext ctx) =>
        ctx.Request.Headers.TryGetValue("X-Sucursal-Id", out var val) &&
        int.TryParse(val, out var id) && id > 0 ? id : null;

    private static int GetWeekNumber(DateTime date)
    {
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }
}
