using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MrTamal.API.Data;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.API.Endpoints;

public static class MovimientoEndpoints
{
    public static void MapMovimientoEndpoints(this WebApplication app)
    {
        MapIngresosEndpoints(app);
        MapEgresosEndpoints(app);
    }

    private static void MapIngresosEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/ingresos").WithTags("Ingresos").RequireAuthorization();

        group.MapGet("/", async (AppDbContext db, ClaimsPrincipal user, DateTime? desde, DateTime? hasta) =>
        {
            var uid = GetUserId(user);
            // Filtrar por sucursal activa del usuario
            var usuario = await db.Usuarios.FindAsync(uid);
            var query = db.Ingresos.Include(i => i.Catalogo).Include(i => i.Sucursal)
                .Where(i => i.UsuarioId == uid || (usuario!.SucursalId.HasValue && i.SucursalId == usuario.SucursalId));
            if (desde.HasValue) query = query.Where(i => i.Fecha >= Utc(desde.Value));
            if (hasta.HasValue) query = query.Where(i => i.Fecha <= Utc(hasta.Value));
            var lista = await query.OrderByDescending(i => i.Fecha).ToListAsync();
            return Results.Ok(lista.Select(ToDto));
        });

        group.MapGet("/{id:int}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var uid = GetUserId(user);
            var i = await db.Ingresos.Include(x => x.Catalogo).Include(x => x.Sucursal)
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == uid);
            return i is null ? Results.NotFound() : Results.Ok(ToDto(i));
        });

        group.MapPost("/", async (CreateMovimientoRequest req, AppDbContext db, ClaimsPrincipal user) =>
        {
            if (req.CatalogoId <= 0) return Results.BadRequest("Debe seleccionar un tipo válido.");
            var catalogo = await db.Catalogos.FindAsync(req.CatalogoId);
            if (catalogo is null) return Results.BadRequest("El tipo seleccionado no existe.");

            var uid = GetUserId(user);
            var fechaDia = Utc(req.Fecha.Date);

            // Validar duplicado: mismo catálogo, misma fecha, mismo usuario
            var duplicado = await db.Ingresos.AnyAsync(i =>
                i.UsuarioId == uid &&
                i.CatalogoId == req.CatalogoId &&
                i.Fecha.Date == fechaDia.Date);
            if (duplicado)
                return Results.Conflict($"Ya existe un ingreso de tipo '{catalogo.Descripcion}' para esa fecha.");

            var usuario = await db.Usuarios.Include(u => u.Sucursal).FirstAsync(u => u.Id == uid);

            var ingreso = new Ingreso
            {
                Fecha = fechaDia,
                CatalogoId = req.CatalogoId,
                Cantidad = req.Cantidad,
                Notas = req.Notas,
                UsuarioId = uid,
                SucursalId = usuario.SucursalId,
                CreadoEn = DateTime.UtcNow
            };
            db.Ingresos.Add(ingreso);
            await db.SaveChangesAsync();
            await db.Entry(ingreso).Reference(i => i.Catalogo).LoadAsync();
            await db.Entry(ingreso).Reference(i => i.Sucursal).LoadAsync();
            return Results.Created($"/api/ingresos/{ingreso.Id}", ToDto(ingreso));
        });

        group.MapPut("/{id:int}", async (int id, UpdateMovimientoRequest req, AppDbContext db, ClaimsPrincipal user) =>
        {
            var uid = GetUserId(user);
            var ingreso = await db.Ingresos.Include(i => i.Catalogo).Include(i => i.Sucursal)
                .FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == uid);
            if (ingreso is null) return Results.NotFound();
            // La sucursal NO cambia al editar - queda la original del momento de creación
            ingreso.Fecha = Utc(req.Fecha);
            ingreso.CatalogoId = req.CatalogoId;
            ingreso.Cantidad = req.Cantidad;
            ingreso.Notas = req.Notas;
            await db.SaveChangesAsync();
            await db.Entry(ingreso).Reference(i => i.Catalogo).LoadAsync();
            return Results.Ok(ToDto(ingreso));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var uid = GetUserId(user);
            var ingreso = await db.Ingresos.FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == uid);
            if (ingreso is null) return Results.NotFound();
            db.Ingresos.Remove(ingreso);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static void MapEgresosEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/egresos").WithTags("Egresos").RequireAuthorization();

        group.MapGet("/", async (AppDbContext db, ClaimsPrincipal user, DateTime? desde, DateTime? hasta) =>
        {
            var uid = GetUserId(user);
            var usuario = await db.Usuarios.FindAsync(uid);
            var query = db.Egresos.Include(e => e.Catalogo).Include(e => e.Sucursal)
                .Where(e => e.UsuarioId == uid || (usuario!.SucursalId.HasValue && e.SucursalId == usuario.SucursalId));
            if (desde.HasValue) query = query.Where(e => e.Fecha >= Utc(desde.Value));
            if (hasta.HasValue) query = query.Where(e => e.Fecha <= Utc(hasta.Value));
            var lista = await query.OrderByDescending(e => e.Fecha).ToListAsync();
            return Results.Ok(lista.Select(ToEgresoDto));
        });

        group.MapGet("/{id:int}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var uid = GetUserId(user);
            var e = await db.Egresos.Include(x => x.Catalogo).Include(x => x.Sucursal)
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == uid);
            return e is null ? Results.NotFound() : Results.Ok(ToEgresoDto(e));
        });

        group.MapPost("/", async (CreateMovimientoRequest req, AppDbContext db, ClaimsPrincipal user) =>
        {
            if (req.CatalogoId <= 0) return Results.BadRequest("Debe seleccionar un tipo válido.");
            var catalogo = await db.Catalogos.FindAsync(req.CatalogoId);
            if (catalogo is null) return Results.BadRequest("El tipo seleccionado no existe.");

            var uid = GetUserId(user);
            var fechaDia = Utc(req.Fecha.Date);

            // Validar duplicado: mismo catálogo, misma fecha, mismo usuario
            var duplicado = await db.Egresos.AnyAsync(e =>
                e.UsuarioId == uid &&
                e.CatalogoId == req.CatalogoId &&
                e.Fecha.Date == fechaDia.Date);
            if (duplicado)
                return Results.Conflict($"Ya existe un egreso de tipo '{catalogo.Descripcion}' para esa fecha.");

            var usuario = await db.Usuarios.Include(u => u.Sucursal).FirstAsync(u => u.Id == uid);

            var egreso = new Egreso
            {
                Fecha = fechaDia,
                CatalogoId = req.CatalogoId,
                Cantidad = req.Cantidad,
                Notas = req.Notas,
                UsuarioId = uid,
                SucursalId = usuario.SucursalId,
                CreadoEn = DateTime.UtcNow
            };
            db.Egresos.Add(egreso);
            await db.SaveChangesAsync();
            await db.Entry(egreso).Reference(e => e.Catalogo).LoadAsync();
            await db.Entry(egreso).Reference(e => e.Sucursal).LoadAsync();
            return Results.Created($"/api/egresos/{egreso.Id}", ToEgresoDto(egreso));
        });

        group.MapPut("/{id:int}", async (int id, UpdateMovimientoRequest req, AppDbContext db, ClaimsPrincipal user) =>
        {
            var uid = GetUserId(user);
            var egreso = await db.Egresos.Include(e => e.Catalogo).Include(e => e.Sucursal)
                .FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == uid);
            if (egreso is null) return Results.NotFound();
            egreso.Fecha = Utc(req.Fecha);
            egreso.CatalogoId = req.CatalogoId;
            egreso.Cantidad = req.Cantidad;
            egreso.Notas = req.Notas;
            await db.SaveChangesAsync();
            await db.Entry(egreso).Reference(e => e.Catalogo).LoadAsync();
            return Results.Ok(ToEgresoDto(egreso));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var uid = GetUserId(user);
            var egreso = await db.Egresos.FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == uid);
            if (egreso is null) return Results.NotFound();
            db.Egresos.Remove(egreso);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        var val = user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? user.FindFirstValue("sub")
               ?? user.FindFirstValue("nameid");
        return int.TryParse(val, out var id) ? id : throw new UnauthorizedAccessException("Usuario no identificado.");
    }

    private static DateTime Utc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    private static MovimientoDto ToDto(Ingreso i) =>
        new(i.Id, i.Fecha, i.Catalogo?.Codigo ?? "", i.Catalogo?.Descripcion ?? "", i.Cantidad, i.Notas,
            i.Sucursal?.SimboloMoneda ?? "$");

    private static MovimientoDto ToEgresoDto(Egreso e) =>
        new(e.Id, e.Fecha, e.Catalogo?.Codigo ?? "", e.Catalogo?.Descripcion ?? "", e.Cantidad, e.Notas,
            e.Sucursal?.SimboloMoneda ?? "$");
}
