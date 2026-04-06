using Microsoft.EntityFrameworkCore;
using MrTamal.API.Data;
using MrTamal.Shared.DTOs;

namespace MrTamal.API.Services;

public class ReporteService(AppDbContext db)
{
    public async Task<ReporteResumen> GenerarAsync(ReporteRequest req, int usuarioId)
    {
        var (inicio, fin) = ObtenerRango(req);

        // PostgreSQL necesita UTC
        var inicioUtc = DateTime.SpecifyKind(inicio, DateTimeKind.Utc);
        var finUtc = DateTime.SpecifyKind(fin, DateTimeKind.Utc);

        var ingresos = await db.Ingresos
            .Include(i => i.Catalogo)
            .Where(i => i.UsuarioId == usuarioId && i.Fecha >= inicioUtc && i.Fecha <= finUtc)
            .OrderBy(i => i.Fecha)
            .Select(i => new ReporteDetalle(i.Fecha, i.Catalogo!.Codigo, i.Catalogo.Descripcion, i.Cantidad, i.Notas))
            .ToListAsync();

        var egresos = await db.Egresos
            .Include(e => e.Catalogo)
            .Where(e => e.UsuarioId == usuarioId && e.Fecha >= inicioUtc && e.Fecha <= finUtc)
            .OrderBy(e => e.Fecha)
            .Select(e => new ReporteDetalle(e.Fecha, e.Catalogo!.Codigo, e.Catalogo.Descripcion, e.Cantidad, e.Notas))
            .ToListAsync();

        List<ReporteComparacion>? comparaciones = null;

        if (req.Tipo == TipoReporte.ComparacionAnual && req.Anio.HasValue && req.AnioComparacion.HasValue)
            comparaciones = await ComparacionAnualAsync(usuarioId, req.Anio.Value, req.AnioComparacion.Value);
        else if (req.Tipo == TipoReporte.ComparacionMensual && req.Anio.HasValue)
            comparaciones = await ComparacionMensualAsync(usuarioId, req.Anio.Value);

        return new ReporteResumen(
            ingresos.Sum(i => i.Cantidad),
            egresos.Sum(e => e.Cantidad),
            ingresos.Sum(i => i.Cantidad) - egresos.Sum(e => e.Cantidad),
            ingresos, egresos, comparaciones
        );
    }

    private static (DateTime inicio, DateTime fin) ObtenerRango(ReporteRequest req)
    {
        var hoy = DateTime.UtcNow.Date;
        return req.Tipo switch
        {
            TipoReporte.Diario => (hoy, hoy.AddDays(1).AddTicks(-1)),
            TipoReporte.Semanal => (hoy.AddDays(-(int)hoy.DayOfWeek), hoy.AddDays(7 - (int)hoy.DayOfWeek).AddTicks(-1)),
            TipoReporte.Mensual => (new DateTime(hoy.Year, hoy.Month, 1), new DateTime(hoy.Year, hoy.Month, 1).AddMonths(1).AddTicks(-1)),
            _ when req.FechaInicio.HasValue && req.FechaFin.HasValue => (req.FechaInicio.Value, req.FechaFin.Value),
            _ => (hoy.AddMonths(-1), hoy)
        };
    }

    private async Task<List<ReporteComparacion>> ComparacionAnualAsync(int usuarioId, int anio1, int anio2)
    {
        var result = new List<ReporteComparacion>();
        foreach (var anio in new[] { anio1, anio2 })
        {
            var ini = DateTime.SpecifyKind(new DateTime(anio, 1, 1), DateTimeKind.Utc);
            var fin = DateTime.SpecifyKind(new DateTime(anio, 12, 31, 23, 59, 59), DateTimeKind.Utc);
            var ti = await db.Ingresos.Where(i => i.UsuarioId == usuarioId && i.Fecha >= ini && i.Fecha <= fin).SumAsync(i => (decimal?)i.Cantidad) ?? 0;
            var te = await db.Egresos.Where(e => e.UsuarioId == usuarioId && e.Fecha >= ini && e.Fecha <= fin).SumAsync(e => (decimal?)e.Cantidad) ?? 0;
            result.Add(new ReporteComparacion(anio.ToString(), ti, te, ti - te));
        }
        return result;
    }

    private async Task<List<ReporteComparacion>> ComparacionMensualAsync(int usuarioId, int anio)
    {
        var result = new List<ReporteComparacion>();
        for (int mes = 1; mes <= 12; mes++)
        {
            var ini = DateTime.SpecifyKind(new DateTime(anio, mes, 1), DateTimeKind.Utc);
            var fin = DateTime.SpecifyKind(ini.AddMonths(1).AddTicks(-1), DateTimeKind.Utc);
            var ti = await db.Ingresos.Where(i => i.UsuarioId == usuarioId && i.Fecha >= ini && i.Fecha <= fin).SumAsync(i => (decimal?)i.Cantidad) ?? 0;
            var te = await db.Egresos.Where(e => e.UsuarioId == usuarioId && e.Fecha >= ini && e.Fecha <= fin).SumAsync(e => (decimal?)e.Cantidad) ?? 0;
            result.Add(new ReporteComparacion($"{ini:MMMM}", ti, te, ti - te));
        }
        return result;
    }
}
