using Microsoft.EntityFrameworkCore;
using MrTamal.API.Data;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.API.Endpoints;

public static class SucursalEndpoints
{
    public static void MapSucursalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sucursales").WithTags("Sucursales").RequireAuthorization();

        group.MapGet("/", async (AppDbContext db) =>
        {
            var lista = await db.Sucursales.OrderBy(s => s.Nombre).ToListAsync();
            return Results.Ok(lista.Select(ToDto));
        });

        group.MapGet("/paises", () =>
            Results.Ok(MonedasPorPais.Catalogo.Select(kv => new
            {
                Codigo = kv.Key,
                Nombre = kv.Value.Nombre,
                Moneda = kv.Value.Moneda,
                Simbolo = kv.Value.Simbolo
            })));

        group.MapPost("/", async (CreateSucursalRequest req, AppDbContext db) =>
        {
            MonedasPorPais.Catalogo.TryGetValue(req.Pais, out var m);
            var s = new Sucursal
            {
                Nombre = req.Nombre, Pais = req.Pais,
                Moneda = m.Moneda ?? "USD", SimboloMoneda = m.Simbolo ?? "$",
                Direccion = req.Direccion, Telefono = req.Telefono
            };
            db.Sucursales.Add(s);
            await db.SaveChangesAsync();
            return Results.Created($"/api/sucursales/{s.Id}", ToDto(s));
        });

        group.MapPut("/{id:int}", async (int id, UpdateSucursalRequest req, AppDbContext db) =>
        {
            var s = await db.Sucursales.FindAsync(id);
            if (s is null) return Results.NotFound();
            MonedasPorPais.Catalogo.TryGetValue(req.Pais, out var m);
            s.Nombre = req.Nombre; s.Pais = req.Pais;
            s.Moneda = m.Moneda ?? "USD"; s.SimboloMoneda = m.Simbolo ?? "$";
            s.Direccion = req.Direccion; s.Telefono = req.Telefono; s.Activa = req.Activa;
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(s));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var s = await db.Sucursales.FindAsync(id);
            if (s is null) return Results.NotFound();
            s.Activa = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static SucursalDto ToDto(Sucursal s) =>
        new(s.Id, s.Nombre, s.Pais, s.Moneda, s.SimboloMoneda, s.Direccion, s.Telefono, s.Activa);
}
