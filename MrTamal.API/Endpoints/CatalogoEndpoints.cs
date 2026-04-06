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
            var catalogo = new Catalogo { Codigo = req.Codigo.ToUpper(), Descripcion = req.Descripcion, Tipo = req.Tipo };
            db.Catalogos.Add(catalogo);
            await db.SaveChangesAsync();
            return Results.Created($"/api/catalogos/{catalogo.Id}", new CatalogoDto(catalogo.Id, catalogo.Codigo, catalogo.Descripcion, catalogo.Tipo, catalogo.Activo));
        });

        group.MapPut("/{id:int}", async (int id, UpdateCatalogoRequest req, AppDbContext db) =>
        {
            var catalogo = await db.Catalogos.FindAsync(id);
            if (catalogo is null) return Results.NotFound();
            catalogo.Codigo = req.Codigo.ToUpper();
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
