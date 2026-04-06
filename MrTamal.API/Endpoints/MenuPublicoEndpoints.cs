using Microsoft.EntityFrameworkCore;
using MrTamal.API.Data;
using MrTamal.Shared.Models;

namespace MrTamal.API.Endpoints;

public static class MenuPublicoEndpoints
{
    public static void MapMenuPublicoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/menu").WithTags("Menu");

        // Público - ver menú
        group.MapGet("/", async (AppDbContext db) =>
        {
            var items = await db.MenuItems.Where(m => m.Activo).OrderBy(m => m.Categoria).ThenBy(m => m.Nombre).ToListAsync();
            return Results.Ok(items);
        });

        // Admin - crear item del menú
        group.MapPost("/", async (HttpRequest request, AppDbContext db) =>
        {
            var form = await request.ReadFormAsync();
            var nombre = form["nombre"].ToString();
            var descripcion = form["descripcion"].ToString();
            var precio = decimal.TryParse(form["precio"], out var p) ? p : 0;
            var categoria = form["categoria"].ToString();
            var archivo = form.Files.FirstOrDefault();

            string? imagenUrl = null;
            if (archivo is not null)
            {
                var ext = Path.GetExtension(archivo.FileName);
                var nombreArchivo = $"{Guid.NewGuid()}{ext}";
                var carpeta = Path.Combine("wwwroot", "menu-images");
                Directory.CreateDirectory(carpeta);
                var ruta = Path.Combine(carpeta, nombreArchivo);
                using var stream = File.Create(ruta);
                await archivo.CopyToAsync(stream);
                imagenUrl = $"/menu-images/{nombreArchivo}";
            }

            var item = new MenuItem
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Precio = precio,
                Categoria = categoria,
                ImagenUrl = imagenUrl,
                Activo = true
            };
            db.MenuItems.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/menu/{item.Id}", item);
        }).RequireAuthorization();

        // Admin - actualizar
        group.MapPut("/{id:int}", async (int id, HttpRequest request, AppDbContext db) =>
        {
            var item = await db.MenuItems.FindAsync(id);
            if (item is null) return Results.NotFound();

            var form = await request.ReadFormAsync();
            item.Nombre = form["nombre"].ToString();
            item.Descripcion = form["descripcion"].ToString();
            item.Precio = decimal.TryParse(form["precio"], out var p) ? p : item.Precio;
            item.Categoria = form["categoria"].ToString();
            item.Activo = form["activo"].ToString() != "false";

            var archivo = form.Files.FirstOrDefault();
            if (archivo is not null)
            {
                var ext = Path.GetExtension(archivo.FileName);
                var nombreArchivo = $"{Guid.NewGuid()}{ext}";
                var carpeta = Path.Combine("wwwroot", "menu-images");
                Directory.CreateDirectory(carpeta);
                var ruta = Path.Combine(carpeta, nombreArchivo);
                using var stream = File.Create(ruta);
                await archivo.CopyToAsync(stream);
                item.ImagenUrl = $"/menu-images/{nombreArchivo}";
            }

            await db.SaveChangesAsync();
            return Results.Ok(item);
        }).RequireAuthorization();

        // Admin - eliminar
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var item = await db.MenuItems.FindAsync(id);
            if (item is null) return Results.NotFound();
            item.Activo = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
