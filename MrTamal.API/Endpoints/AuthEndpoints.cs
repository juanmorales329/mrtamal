using Microsoft.EntityFrameworkCore;
using MrTamal.API.Data;
using MrTamal.API.Services;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        // Solo para crear el primer Admin si no existe ningún usuario
        group.MapPost("/setup", async (SetupRequest req, AppDbContext db, TokenService tokenSvc) =>
        {
            if (await db.Usuarios.AnyAsync())
                return Results.BadRequest("El sistema ya está configurado.");

            var admin = new Usuario
            {
                Nombre = req.Nombre,
                Username = req.Username.ToLower().Trim(),
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Rol = Roles.Admin
            };
            db.Usuarios.Add(admin);
            await db.SaveChangesAsync();

            var token = tokenSvc.GenerarToken(admin);
            return Results.Ok(new AuthResponse(token, admin.Nombre, admin.Username, admin.Rol, null, "$"));
        });

        // Login por username
        group.MapPost("/login", async (LoginRequest req, AppDbContext db, TokenService tokenSvc) =>
        {
            var usuario = await db.Usuarios.Include(u => u.Sucursal)
                .FirstOrDefaultAsync(u => u.Username == req.Username.ToLower().Trim() && u.Activo);
            if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Password, usuario.PasswordHash))
                return Results.Unauthorized();

            var simbolo = ObtenerSimbolo(usuario.Sucursal);
            var token = tokenSvc.GenerarToken(usuario);
            return Results.Ok(new AuthResponse(token, usuario.Nombre, usuario.Username, usuario.Rol, usuario.SucursalId, simbolo));
        });

        // Verificar si necesita setup inicial
        group.MapGet("/needs-setup", async (AppDbContext db) =>
            Results.Ok(new { needsSetup = !await db.Usuarios.AnyAsync() }));
    }

    private static string ObtenerSimbolo(Sucursal? sucursal)
    {
        if (sucursal is null) return "$";
        MonedasPorPais.Catalogo.TryGetValue(sucursal.Pais, out var m);
        return m.Simbolo ?? "$";
    }
}

public record SetupRequest(string Nombre, string Username, string Email, string Password);
