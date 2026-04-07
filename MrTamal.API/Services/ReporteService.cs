using Microsoft.EntityFrameworkCore;
using MrTamal.API.Data;
using MrTamal.Shared.DTOs;

namespace MrTamal.API.Services;

public class ReporteService(AppDbContext db)
{
    public async Task<ReporteResumen> GenerarAsync(ReporteRequest req, int usuarioId)
    {
        var (inicio, fin) = ObtenerRango(req);

        var inicioUtc = DateTime.SpecifyKind(inicio, DateTimeKind.Utc);
        var finUtc = DateTime.SpecifyKind(fin, DateTimeKind.Utc);

        // Si el usuario pertenece a una sucursal, mostrar datos de toda la sucursal
        var usuario = await db.Usuarios.FindAsync(usuarioId);
        var sucursalId = usuario?.SucursalId;

        IQueryable<Ingreso> qIng = db.Ingresos.Include(i => i.Catalogo)
            .Where(i => i.Fecha >= inicioUtc && i.Fecha <= finUtc);
        IQueryable<Egreso> qEgr = db.Egresos.Include(e => e.Catalogo)
            .Where(e => e.Fecha >= inicioUtc && e.Fecha <= finUtc);

        if (sucursalId.HasValue)
        {
            qIng = qIng.Where(i => i.SucursalId == sucursalId);
            qEgr = qEgr.Where(e => e.SucursalId == sucursalId);
        }
        else
        {
            qIng = qIng.Where(i => i.UsuarioId == usuarioId);
            qEgr = qEgr.Where(e => e.UsuarioId == usuarioId);
        }

        var ingresos = await qIng.OrderBy(i => i.Fecha)
            .Select(i => new ReporteDetalle(i.Fecha, i.Catalogo!.Codigo, i.Catalogo.Descripcion, i.Cantidad, i.Notas))
            .ToListAsync();

        var egresos = await qEgr.OrderBy(e => e.Fecha)
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
        // Si vienen fechas explícitas, usarlas siempre
        if (req.FechaInicio.HasValue && req.FechaFin.HasValue)
        {
            var ini = DateTime.SpecifyKind(req.FechaInicio.Value, DateTimeKind.Utc);
            var fin = DateTime.SpecifyKind(req.FechaFin.Value, DateTimeKind.Utc);
            return (ini, fin);
        }
        return req.Tipo switch
        {
            TipoReporte.Diario => (hoy, hoy.AddDays(1).AddTicks(-1)),
            TipoReporte.Semanal => (hoy.AddDays(-(int)hoy.DayOfWeek), hoy.AddDays(7 - (int)hoy.DayOfWeek).AddTicks(-1)),
            TipoReporte.Mensual => (new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                                    new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddTicks(-1)),
            _ => (hoy.AddMonths(-1), hoy)
        };
    }

    private async Task<List<ReporteComparacion>> ComparacionAnualAsync(int usuarioId, int anio1, int anio2)
    {
        var usuario = await db.Usuarios.FindAsync(usuarioId);
        var sucursalId = usuario?.SucursalId;
        var result = new List<ReporteComparacion>();
        foreach (var anio in new[] { anio1, anio2 })
        {
            var ini = DateTime.SpecifyKind(new DateTime(anio, 1, 1), DateTimeKind.Utc);
            var fin = DateTime.SpecifyKind(new DateTime(anio, 12, 31, 23, 59, 59), DateTimeKind.Utc);
            IQueryable<Ingreso> qI = db.Ingresos.Where(i => i.Fecha >= ini && i.Fecha <= fin);
            IQueryable<Egreso> qE = db.Egresos.Where(e => e.Fecha >= ini && e.Fecha <= fin);
            if (sucursalId.HasValue) { qI = qI.Where(i => i.SucursalId == sucursalId); qE = qE.Where(e => e.SucursalId == sucursalId); }
            else { qI = qI.Where(i => i.UsuarioId == usuarioId); qE = qE.Where(e => e.UsuarioId == usuarioId); }
            var ti = await qI.SumAsync(i => (decimal?)i.Cantidad) ?? 0;
            var te = await qE.SumAsync(e => (decimal?)e.Cantidad) ?? 0;
            result.Add(new ReporteComparacion(anio.ToString(), ti, te, ti - te));
        }
        return result;
    }

    private async Task<List<ReporteComparacion>> ComparacionMensualAsync(int usuarioId, int anio)
    {
        var usuario = await db.Usuarios.FindAsync(usuarioId);
        var sucursalId = usuario?.SucursalId;
        var result = new List<ReporteComparacion>();
        for (int mes = 1; mes <= 12; mes++)
        {
            var ini = DateTime.SpecifyKind(new DateTime(anio, mes, 1), DateTimeKind.Utc);
            var fin = DateTime.SpecifyKind(ini.AddMonths(1).AddTicks(-1), DateTimeKind.Utc);
            IQueryable<Ingreso> qI = db.Ingresos.Where(i => i.Fecha >= ini && i.Fecha <= fin);
            IQueryable<Egreso> qE = db.Egresos.Where(e => e.Fecha >= ini && e.Fecha <= fin);
            if (sucursalId.HasValue) { qI = qI.Where(i => i.SucursalId == sucursalId); qE = qE.Where(e => e.SucursalId == sucursalId); }
            else { qI = qI.Where(i => i.UsuarioId == usuarioId); qE = qE.Where(e => e.UsuarioId == usuarioId); }
            var ti = await qI.SumAsync(i => (decimal?)i.Cantidad) ?? 0;
            var te = await qE.SumAsync(e => (decimal?)e.Cantidad) ?? 0;
            result.Add(new ReporteComparacion($"{ini:MMMM}", ti, te, ti - te));
        }
        return result;
    }
}
