using Microsoft.EntityFrameworkCore;
using MrTamal.API.Data;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.API.Endpoints;

public static class CatalogoEndpoints
{
    public static void MapCatalogoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/catalogos").WithTags("Catalogos").RequireAuthorization();

        group.MapGet("/siguiente-codigo/{tipo}", async (TipoCatalogo tipo, AppDbContext db) =>
        {
            var rango = tipo == TipoCatalogo.Ingreso ? (1001, 1999) : (2001, 2999);
            var usados = await db.Catalogos.Where(c => c.Tipo == tipo).Select(c => c.Codigo).ToListAsync();
            var siguiente = Enumerable.Range(rango.Item1, rango.Item2 - rango.Item1 + 1)
                .Select(n => n.ToString())
                .FirstOrDefault(n => !usados.Contains(n));
            return Results.Ok(new { codigo = siguiente ?? "" });
        });

        group.MapGet("/", async (AppDbContext db, TipoCatalogo? tipo) =>
        {
            var query = db.Catalogos.AsQueryable();
            if (tipo.HasValue) query = query.Where(c => c.Tipo == tipo.Value);
            var lista = await query.OrderBy(c => c.Codigo).ToListAsync();
            return Results.Ok(lista.Select(c => new CatalogoDto(c.Id, c.Codigo, c.Descripcion, c.Tipo, c.Activo)));
        });

        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var c = await db.Catalogos.FindAsync(id);
            return c is null ? Results.NotFound() : Results.Ok(new CatalogoDto(c.Id, c.Codigo, c.Descripcion, c.Tipo, c.Activo));
        });

        group.MapPost("/", async (CreateCatalogoRequest req, AppDbContext db) =>
        {
            var codigoUpper = req.Codigo.ToUpper();
            if (await db.Catalogos.AnyAsync(c => c.Codigo == codigoUpper && c.Tipo == req.Tipo))
            {
                // Sugerir siguiente código libre
                var rango = req.Tipo == TipoCatalogo.Ingreso ? (1001, 1999) : (2001, 2999);
                var usados = await db.Catalogos
                    .Where(c => c.Tipo == req.Tipo)
                    .Select(c => c.Codigo)
                    .ToListAsync();
                var siguiente = Enumerable.Range(rango.Item1, rango.Item2 - rango.Item1 + 1)
                    .Select(n => n.ToString())
                    .FirstOrDefault(n => !usados.Contains(n));
                return Results.BadRequest(
                    $"El código '{codigoUpper}' ya existe. Siguiente disponible: {siguiente ?? "Sin códigos disponibles"}");
            }

            var catalogo = new Catalogo { Codigo = codigoUpper, Descripcion = req.Descripcion, Tipo = req.Tipo };
            db.Catalogos.Add(catalogo);
            try
            {
                await db.SaveChangesAsync();
                return Results.Created($"/api/catalogos/{catalogo.Id}",
                    new CatalogoDto(catalogo.Id, catalogo.Codigo, catalogo.Descripcion, catalogo.Tipo, catalogo.Activo));
            }
            catch (DbUpdateException)
            {
                return Results.BadRequest($"El código '{codigoUpper}' ya existe.");
            }
        });

        group.MapPut("/{id:int}", async (int id, UpdateCatalogoRequest req, AppDbContext db) =>
        {
            var catalogo = await db.Catalogos.FindAsync(id);
            if (catalogo is null) return Results.NotFound();
            var codigoUpper = req.Codigo.ToUpper();
            // Verificar duplicado en edición
            if (await db.Catalogos.AnyAsync(c => c.Codigo == codigoUpper && c.Tipo == catalogo.Tipo && c.Id != id))
                return Results.BadRequest($"El código '{codigoUpper}' ya existe.");
            catalogo.Codigo = codigoUpper;
            catalogo.Descripcion = req.Descripcion;
            catalogo.Activo = req.Activo;
            await db.SaveChangesAsync();
            return Results.Ok(new CatalogoDto(catalogo.Id, catalogo.Codigo, catalogo.Descripcion, catalogo.Tipo, catalogo.Activo));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var catalogo = await db.Catalogos.FindAsync(id);
            if (catalogo is null) return Results.NotFound();
            catalogo.Activo = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
