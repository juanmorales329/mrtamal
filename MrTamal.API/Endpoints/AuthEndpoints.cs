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

        group.MapPost("/register", async (RegisterRequest req, AppDbContext db, TokenService tokenSvc) =>
        {
            if (await db.Usuarios.AnyAsync(u => u.Email == req.Email))
                return Results.BadRequest("El email ya está registrado.");

            // Primer usuario es Admin
            var esAdmin = !await db.Usuarios.AnyAsync();
            var usuario = new Usuario
            {
                Nombre = req.Nombre,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Rol = esAdmin ? Roles.Admin : Roles.Usuario
            };
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();

            var simbolo = ObtenerSimbolo(null);
            var token = tokenSvc.GenerarToken(usuario);
            return Results.Ok(new AuthResponse(token, usuario.Nombre, usuario.Email, usuario.Rol, null, simbolo));
        });

        group.MapPost("/login", async (LoginRequest req, AppDbContext db, TokenService tokenSvc) =>
        {
            var usuario = await db.Usuarios.Include(u => u.Sucursal)
                .FirstOrDefaultAsync(u => u.Email == req.Email && u.Activo);
            if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Password, usuario.PasswordHash))
                return Results.Unauthorized();

            var simbolo = ObtenerSimbolo(usuario.Sucursal);
            var token = tokenSvc.GenerarToken(usuario);
            return Results.Ok(new AuthResponse(token, usuario.Nombre, usuario.Email, usuario.Rol, usuario.SucursalId, simbolo));
        });
    }

    private static string ObtenerSimbolo(Sucursal? sucursal)
    {
        if (sucursal is null) return "$";
        MonedasPorPais.Catalogo.TryGetValue(sucursal.Pais, out var m);
        return m.Simbolo ?? "$";
    }
}
