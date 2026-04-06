using Microsoft.EntityFrameworkCore;
using MrTamal.API.Data;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.API.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/usuarios").WithTags("Usuarios").RequireAuthorization();

        group.MapGet("/", async (AppDbContext db) =>
        {
            var lista = await db.Usuarios.Include(u => u.Sucursal)
                .OrderBy(u => u.Nombre).ToListAsync();
            return Results.Ok(lista.Select(ToDto));
        });

        // Crear usuario (solo Admin)
        group.MapPost("/", async (CreateUsuarioRequest req, AppDbContext db) =>
        {
            if (await db.Usuarios.AnyAsync(u => u.Username == req.Username.ToLower().Trim()))
                return Results.BadRequest("El nombre de usuario ya existe.");

            var u = new Usuario
            {
                Nombre = req.Nombre,
                Username = req.Username.ToLower().Trim(),
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Rol = req.Rol,
                SucursalId = req.SucursalId,
                Activo = true
            };
            db.Usuarios.Add(u);
            await db.SaveChangesAsync();
            await db.Entry(u).Reference(x => x.Sucursal).LoadAsync();
            return Results.Created($"/api/usuarios/{u.Id}", ToDto(u));
        });

        // Historial de asignaciones
        group.MapGet("/{id:int}/asignaciones", async (int id, AppDbContext db) =>
        {
            var lista = await db.AsignacionesSucursal
                .Include(a => a.Sucursal)
                .Where(a => a.UsuarioId == id)
                .OrderByDescending(a => a.FechaInicio)
                .ToListAsync();
            return Results.Ok(lista.Select(a => new
            {
                a.Id, a.SucursalId,
                SucursalNombre = a.Sucursal?.Nombre,
                Pais = a.Sucursal?.Pais,
                Moneda = a.Sucursal?.SimboloMoneda,
                a.FechaInicio, a.FechaFin, a.Motivo, a.Activa
            }));
        });

        // Actualizar rol/estado
        group.MapPut("/{id:int}", async (int id, UpdateUsuarioRequest req, AppDbContext db) =>
        {
            var u = await db.Usuarios.FindAsync(id);
            if (u is null) return Results.NotFound();
            u.Nombre = req.Nombre;
            u.Rol = req.Rol;
            u.Activo = req.Activo;
            if (u.SucursalId != req.SucursalId)
                await RegistrarTraslado(db, u, req.SucursalId, "Cambio desde administración");
            u.SucursalId = req.SucursalId;
            await db.SaveChangesAsync();
            await db.Entry(u).Reference(x => x.Sucursal).LoadAsync();
            return Results.Ok(ToDto(u));
        });

        // Cambiar contraseña
        group.MapPut("/{id:int}/password", async (int id, CambiarPasswordRequest req, AppDbContext db) =>
        {
            var u = await db.Usuarios.FindAsync(id);
            if (u is null) return Results.NotFound();
            u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NuevoPassword);
            await db.SaveChangesAsync();
            return Results.Ok(new { mensaje = "Contraseña actualizada." });
        });

        // Traslado explícito
        group.MapPost("/{id:int}/trasladar", async (int id, TrasladoRequest req, AppDbContext db) =>
        {
            var u = await db.Usuarios.FindAsync(id);
            if (u is null) return Results.NotFound();
            if (u.SucursalId == req.SucursalId)
                return Results.BadRequest("El usuario ya está en esa sucursal.");
            await RegistrarTraslado(db, u, req.SucursalId, req.Motivo);
            u.SucursalId = req.SucursalId;
            await db.SaveChangesAsync();
            await db.Entry(u).Reference(x => x.Sucursal).LoadAsync();
            return Results.Ok(new { mensaje = "Traslado registrado.", sucursal = u.Sucursal?.Nombre });
        });

        // Eliminar (desactivar)
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var u = await db.Usuarios.FindAsync(id);
            if (u is null) return Results.NotFound();
            u.Activo = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static async Task RegistrarTraslado(AppDbContext db, Usuario usuario, int? nuevaSucursalId, string? motivo)
    {
        var activa = await db.AsignacionesSucursal
            .FirstOrDefaultAsync(a => a.UsuarioId == usuario.Id && a.Activa);
        if (activa is not null) { activa.FechaFin = DateTime.UtcNow; activa.Activa = false; }
        if (nuevaSucursalId.HasValue)
            db.AsignacionesSucursal.Add(new AsignacionSucursal
            {
                UsuarioId = usuario.Id, SucursalId = nuevaSucursalId.Value,
                FechaInicio = DateTime.UtcNow, Motivo = motivo, Activa = true
            });
    }

    private static UsuarioDto ToDto(Usuario u) =>
        new(u.Id, u.Nombre, u.Username, u.Email, u.Rol, u.SucursalId, u.Sucursal?.Nombre, u.Activo);
}

public record TrasladoRequest(int? SucursalId, string? Motivo);
