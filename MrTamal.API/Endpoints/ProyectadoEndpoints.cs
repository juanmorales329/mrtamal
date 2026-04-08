using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MrTamal.API.Data;
using MrTamal.API.Services;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.API.Endpoints;

public static class ProyectadoEndpoints
{
    public static void MapProyectadoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/proyectado").WithTags("Proyectado").RequireAuthorization();

        // Obtener o calcular resumen proyectado
        group.MapGet("/{anio:int}", async (int anio, AppDbContext db, ClaimsPrincipal user) =>
        {
            var uid = GetUserId(user);

            // Buscar meta sin filtro de sucursal - cualquier meta del año
            var meta = await db.MetasAnuales
                .FirstOrDefaultAsync(m => m.Anio == anio);

            if (meta is null) return Results.NotFound(new { mensaje = "No hay meta configurada para este año." });

            var diasNoLab = await db.DiasNoLaborables
                .Where(d => d.Fecha.Year == anio)
                .ToListAsync();

            var resumen = CalcularResumen(meta, diasNoLab, db, uid, anio);
            return Results.Ok(await resumen);
        });

        // Listar metas
        group.MapGet("/metas", async (AppDbContext db, ClaimsPrincipal user) =>
        {
            var uid = GetUserId(user);
            var usuario = await db.Usuarios.FindAsync(uid);
            var lista = await db.MetasAnuales.Include(m => m.Sucursal)
                .Where(m => usuario!.SucursalId == null || m.SucursalId == usuario.SucursalId)
                .OrderByDescending(m => m.Anio).ToListAsync();
            return Results.Ok(lista.Select(m => new MetaAnualDto(m.Id, m.Anio, m.MetaVentas, m.ExcluirDomingos, m.SucursalId, m.Sucursal?.Nombre)));
        });

        // Crear/actualizar meta (upsert)
        group.MapPost("/metas", async (CreateMetaAnualRequest req, AppDbContext db, ClaimsPrincipal user) =>
        {
            var uid = GetUserId(user);
            var existente = await db.MetasAnuales
                .FirstOrDefaultAsync(m => m.Anio == req.Anio);
            if (existente is not null)
            {
                existente.MetaVentas = req.MetaVentas;
                existente.ExcluirDomingos = req.ExcluirDomingos;
            }
            else
            {
                db.MetasAnuales.Add(new MetaAnual
                {
                    Anio = req.Anio, MetaVentas = req.MetaVentas,
                    ExcluirDomingos = req.ExcluirDomingos,
                    SucursalId = req.SucursalId, UsuarioId = uid
                });
            }
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        // Días no laborables
        group.MapGet("/dias-nolaborables/{anio:int}", async (int anio, AppDbContext db, ClaimsPrincipal user) =>
        {
            var uid = GetUserId(user);
            var usuario = await db.Usuarios.FindAsync(uid);
            var lista = await db.DiasNoLaborables
                .Where(d => d.Fecha.Year == anio &&
                    (usuario!.SucursalId == null || d.SucursalId == usuario.SucursalId))
                .OrderBy(d => d.Fecha).ToListAsync();
            return Results.Ok(lista.Select(d => new DiaNolaboralDto(d.Id, d.Fecha, d.Descripcion)));
        });

        group.MapPost("/dias-nolaborables", async (CreateDiaNolaboralRequest req, AppDbContext db, ClaimsPrincipal user) =>
        {
            var uid = GetUserId(user);
            var fechaUtc = DateTime.SpecifyKind(req.Fecha.Date, DateTimeKind.Utc);
            if (await db.DiasNoLaborables.AnyAsync(d => d.Fecha == fechaUtc && d.SucursalId == req.SucursalId))
                return Results.BadRequest("Esa fecha ya está registrada.");
            db.DiasNoLaborables.Add(new DiaNolaboral
            {
                Fecha = fechaUtc, Descripcion = req.Descripcion,
                SucursalId = req.SucursalId, UsuarioId = uid
            });
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        group.MapDelete("/dias-nolaborables/{id:int}", async (int id, AppDbContext db) =>
        {
            var d = await db.DiasNoLaborables.FindAsync(id);
            if (d is null) return Results.NotFound();
            db.DiasNoLaborables.Remove(d);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Exportar PDF del proyectado
        group.MapGet("/{anio:int}/pdf", async (int anio, AppDbContext db, ClaimsPrincipal user, PdfService pdfSvc) =>
        {
            var uid = GetUserId(user);
            var meta = await db.MetasAnuales.FirstOrDefaultAsync(m => m.Anio == anio);
            if (meta is null) return Results.NotFound();
            var anoIni = DateTime.SpecifyKind(new DateTime(anio, 1, 1), DateTimeKind.Utc);
            var anoFin = DateTime.SpecifyKind(new DateTime(anio, 12, 31, 23, 59, 59), DateTimeKind.Utc);
            var diasNoLab = await db.DiasNoLaborables.Where(d => d.Fecha >= anoIni && d.Fecha <= anoFin).ToListAsync();
            var resumen = await CalcularResumen(meta, diasNoLab, db, uid, anio);
            var sucursal = await db.Usuarios.Include(u => u.Sucursal).Where(u => u.Id == uid).Select(u => u.Sucursal).FirstOrDefaultAsync();
            var simbolo = sucursal?.SimboloMoneda ?? "$";
            var pdf = pdfSvc.GenerarProyectadoPdf(resumen, simbolo);
            return Results.File(pdf, "application/pdf", $"proyectado_{anio}.pdf");
        });

        group.MapGet("/{anio:int}/excel", async (int anio, AppDbContext db, ClaimsPrincipal user, PdfService pdfSvc) =>
        {
            var uid = GetUserId(user);
            var meta = await db.MetasAnuales.FirstOrDefaultAsync(m => m.Anio == anio);
            if (meta is null) return Results.NotFound();
            var anoIni = DateTime.SpecifyKind(new DateTime(anio, 1, 1), DateTimeKind.Utc);
            var anoFin = DateTime.SpecifyKind(new DateTime(anio, 12, 31, 23, 59, 59), DateTimeKind.Utc);
            var diasNoLab = await db.DiasNoLaborables.Where(d => d.Fecha >= anoIni && d.Fecha <= anoFin).ToListAsync();
            var resumen = await CalcularResumen(meta, diasNoLab, db, uid, anio);
            var sucursal = await db.Usuarios.Include(u => u.Sucursal).Where(u => u.Id == uid).Select(u => u.Sucursal).FirstOrDefaultAsync();
            var simbolo = sucursal?.SimboloMoneda ?? "$";
            var excel = pdfSvc.GenerarProyectadoExcel(resumen, simbolo);
            return Results.File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"proyectado_{anio}.xlsx");
        });
    }

    private static async Task<ResumenProyectado> CalcularResumen(
        MetaAnual meta, List<DiaNolaboral> diasNoLab, AppDbContext db, int uid, int anio)
    {
        var fechasNoLab = diasNoLab.Select(d => d.Fecha.Date).ToHashSet();

        // Calcular días laborables del año
        int diasLaborablesAnio = 0;
        var inicio = new DateTime(anio, 1, 1);
        var fin = new DateTime(anio, 12, 31);
        for (var d = inicio; d <= fin; d = d.AddDays(1))
        {
            if (meta.ExcluirDomingos && d.DayOfWeek == DayOfWeek.Sunday) continue;
            if (fechasNoLab.Contains(d.Date)) continue;
            diasLaborablesAnio++;
        }

        var metaDiaria = diasLaborablesAnio > 0 ? meta.MetaVentas / diasLaborablesAnio : 0;

        // Ventas reales del año - visibilidad según rol
        var usuario = await db.Usuarios.FindAsync(uid);
        var esGlobal = (usuario?.Rol == Roles.Admin || usuario?.Rol == Roles.Gerente) && !usuario.SucursalId.HasValue;
        var sucursalId = usuario?.SucursalId;

        // Usar rango UTC en lugar de .Year para compatibilidad con PostgreSQL
        var anoInicio = DateTime.SpecifyKind(new DateTime(anio, 1, 1), DateTimeKind.Utc);
        var anoFin = DateTime.SpecifyKind(new DateTime(anio, 12, 31, 23, 59, 59), DateTimeKind.Utc);

        IQueryable<Ingreso> queryIngresos = db.Ingresos.Where(i => i.Fecha >= anoInicio && i.Fecha <= anoFin);
        if (!esGlobal)
        {
            if (sucursalId.HasValue)
                queryIngresos = queryIngresos.Where(i => i.SucursalId == sucursalId);
            else
                queryIngresos = queryIngresos.Where(i => i.UsuarioId == uid);
        }

        var ventaReal = await queryIngresos.SumAsync(i => (decimal?)i.Cantidad) ?? 0;

        // Cuatrimestres fijos: 100k cada uno
        const decimal metaCuatrimestre = 100_000m;
        var cuatrimestres = new List<ResumenCuatrimestre>();
        var definiciones = new[]
        {
            (1, "Ene - Abr", 1, 4),
            (2, "May - Ago", 5, 8),
            (3, "Sep - Dic", 9, 12)
        };
        foreach (var (num, periodo, mesInicio, mesFin) in definiciones)
        {
            var ini = DateTime.SpecifyKind(new DateTime(anio, mesInicio, 1), DateTimeKind.Utc);
            var finC = DateTime.SpecifyKind(new DateTime(anio, mesFin, 1).AddMonths(1).AddTicks(-1), DateTimeKind.Utc);
            IQueryable<Ingreso> qC = db.Ingresos.Where(i => i.Fecha >= ini && i.Fecha <= finC);
            if (!esGlobal)
            {
                if (sucursalId.HasValue) qC = qC.Where(i => i.SucursalId == sucursalId);
                else qC = qC.Where(i => i.UsuarioId == uid);
            }
            var ventaC = await qC.SumAsync(i => (decimal?)i.Cantidad) ?? 0;
            var pctC = metaCuatrimestre > 0 ? Math.Round(ventaC / metaCuatrimestre * 100, 1) : 0;
            cuatrimestres.Add(new ResumenCuatrimestre(num, periodo, metaCuatrimestre, ventaC, ventaC - metaCuatrimestre, pctC));
        }

        // Desglose mensual
        var desglose = new List<DesgloseMes>();
        for (int mes = 1; mes <= 12; mes++)
        {
            var inicioMes = new DateTime(anio, mes, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);
            int diasLabMes = 0;
            for (var d = inicioMes; d <= finMes; d = d.AddDays(1))
            {
                if (meta.ExcluirDomingos && d.DayOfWeek == DayOfWeek.Sunday) continue;
                if (fechasNoLab.Contains(d.Date)) continue;
                diasLabMes++;
            }
            var metaMes = metaDiaria * diasLabMes;
            var iniMesUtc = DateTime.SpecifyKind(inicioMes, DateTimeKind.Utc);
            var finMesUtc = DateTime.SpecifyKind(finMes.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            IQueryable<Ingreso> qMes = db.Ingresos.Where(i => i.Fecha >= iniMesUtc && i.Fecha <= finMesUtc);
            if (!esGlobal)
            {
                if (sucursalId.HasValue) qMes = qMes.Where(i => i.SucursalId == sucursalId);
                else qMes = qMes.Where(i => i.UsuarioId == uid);
            }
            var ventaMes = await qMes.SumAsync(i => (decimal?)i.Cantidad) ?? 0;

            desglose.Add(new DesgloseMes(
                mes, inicioMes.ToString("MMMM"),
                diasLabMes, metaMes, ventaMes,
                ventaMes - metaMes, ventaMes >= metaMes
            ));
        }

        var pct = meta.MetaVentas > 0 ? (ventaReal / meta.MetaVentas) * 100 : 0;

        return new ResumenProyectado(
            anio, meta.MetaVentas, diasLaborablesAnio, ventaReal,
            Math.Round(pct, 1),
            Math.Round(metaDiaria, 2),
            Math.Round(metaDiaria * 7, 2),
            Math.Round(metaDiaria * (diasLaborablesAnio / 12m), 2),
            Math.Round(metaDiaria * (diasLaborablesAnio / 6m), 2),
            Math.Round(metaDiaria * (diasLaborablesAnio / 4m), 2),
            Math.Round(metaDiaria * (diasLaborablesAnio / 3m), 2),
            Math.Round(metaDiaria * (diasLaborablesAnio / 2m), 2),
            desglose,
            diasNoLab.Select(d => new DiaNolaboralDto(d.Id, d.Fecha, d.Descripcion)).ToList(),
            cuatrimestres
        );
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        var val = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return int.TryParse(val, out var id) ? id : throw new UnauthorizedAccessException();
    }
}
