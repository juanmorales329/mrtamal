using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using MrTamal.API.Data;
using MrTamal.Shared.Models;

namespace MrTamal.API.Endpoints;

public static class CargaMasivaEndpoints
{
    public static void MapCargaMasivaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/carga-masiva").WithTags("CargaMasiva").RequireAuthorization();

        // Carga desde Excel o CSV pegado como texto
        group.MapPost("/ingresos", async (HttpRequest request, AppDbContext db, ClaimsPrincipal user) =>
            await ProcesarCarga(request, db, user, TipoCatalogo.Ingreso));

        group.MapPost("/egresos", async (HttpRequest request, AppDbContext db, ClaimsPrincipal user) =>
            await ProcesarCarga(request, db, user, TipoCatalogo.Egreso));

        // Descargar plantilla Excel
        group.MapGet("/plantilla/{tipo}", (string tipo) =>
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var pkg = new ExcelPackage();
            var ws = pkg.Workbook.Worksheets.Add("Datos");

            // Encabezados
            ws.Cells[1, 1].Value = "Fecha";
            ws.Cells[1, 2].Value = "Codigo";
            ws.Cells[1, 3].Value = "Cantidad";
            ws.Cells[1, 4].Value = "Notas";

            // Formato encabezado
            using var headerRange = ws.Cells[1, 1, 1, 4];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(33, 150, 243));
            headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);

            // Ejemplos
            ws.Cells[2, 1].Value = DateTime.Today.ToString("dd/MM/yyyy");
            ws.Cells[2, 2].Value = "SAL";
            ws.Cells[2, 3].Value = 5000.00;
            ws.Cells[2, 4].Value = "Salario enero";

            ws.Cells[3, 1].Value = DateTime.Today.ToString("dd/MM/yyyy");
            ws.Cells[3, 2].Value = "BON";
            ws.Cells[3, 3].Value = 1000.00;
            ws.Cells[3, 4].Value = "Bono";

            ws.Cells.AutoFitColumns();

            var bytes = pkg.GetAsByteArray();
            return Results.File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"plantilla_{tipo}.xlsx");
        });
    }

    private static async Task<IResult> ProcesarCarga(
        HttpRequest request, AppDbContext db, ClaimsPrincipal user, TipoCatalogo tipoCatalogo)
    {
        try
        {
            var uid = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var catalogos = await db.Catalogos
                .Where(c => c.Tipo == tipoCatalogo && c.Activo)
                .ToListAsync();

        var filas = new List<FilaCarga>();
        var errores = new List<string>();

        // Leer según content type
        if (request.ContentType?.Contains("multipart") == true)
        {
            // Archivo Excel
            var form = await request.ReadFormAsync();
            var archivo = form.Files.FirstOrDefault();
            if (archivo is null) return Results.BadRequest("No se recibió archivo.");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var stream = archivo.OpenReadStream();
            using var pkg = new ExcelPackage(stream);
            var ws = pkg.Workbook.Worksheets[0];
            var totalFilas = ws.Dimension?.Rows ?? 0;

            for (int r = 2; r <= totalFilas; r++)
            {
                var fechaStr = ws.Cells[r, 1].Text.Trim();
                var codigo = ws.Cells[r, 2].Text.Trim().ToUpper();
                var cantidadStr = ws.Cells[r, 3].Text.Trim();
                var notas = ws.Cells[r, 4].Text.Trim();

                if (string.IsNullOrEmpty(fechaStr) && string.IsNullOrEmpty(codigo)) continue;
                filas.Add(new FilaCarga(r, fechaStr, codigo, cantidadStr, notas));
            }
        }
        else
        {
            // Texto pegado (TSV o CSV)
            using var reader = new StreamReader(request.Body);
            var texto = await reader.ReadToEndAsync();
            var lineas = texto.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            int fila = 1;
            foreach (var linea in lineas)
            {
                fila++;
                var partes = linea.Contains('\t')
                    ? linea.Split('\t')
                    : linea.Split(',');
                if (partes.Length < 3) continue;
                filas.Add(new FilaCarga(fila,
                    partes[0].Trim(),
                    partes[1].Trim().ToUpper(),
                    partes[2].Trim(),
                    partes.Length > 3 ? partes[3].Trim() : ""));
            }
        }

        // Procesar filas
        var guardados = 0;
        foreach (var fila in filas)
        {
            if (!DateTime.TryParseExact(fila.FechaStr,
                new[] { "dd/MM/yyyy", "d/MM/yyyy", "M/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "d/M/yyyy" },
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var fecha))
            {
                errores.Add($"Fila {fila.Numero}: fecha inválida '{fila.FechaStr}'");
                continue;
            }

            if (!decimal.TryParse(fila.CantidadStr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var cantidad))
            {
                errores.Add($"Fila {fila.Numero}: cantidad inválida '{fila.CantidadStr}'");
                continue;
            }

            var catalogo = catalogos.FirstOrDefault(c => c.Codigo == fila.Codigo)
                        ?? catalogos.FirstOrDefault(c => c.Codigo == fila.Codigo.TrimStart('0'));
            if (catalogo is null)
            {
                errores.Add($"Fila {fila.Numero}: código '{fila.Codigo}' no existe en catálogos.");
                continue;
            }

            var fechaUtc = DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
            if (tipoCatalogo == TipoCatalogo.Ingreso)
                db.Ingresos.Add(new Ingreso { Fecha = fechaUtc, CatalogoId = catalogo.Id, Cantidad = cantidad, Notas = fila.Notas, UsuarioId = uid, CreadoEn = DateTime.UtcNow });
            else
                db.Egresos.Add(new Egreso { Fecha = fechaUtc, CatalogoId = catalogo.Id, Cantidad = cantidad, Notas = fila.Notas, UsuarioId = uid, CreadoEn = DateTime.UtcNow });

            guardados++;
        }

        await db.SaveChangesAsync();

        return Results.Ok(new { guardados, errores, mensaje = $"{guardados} registros importados. {errores.Count} errores." });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error interno: {ex.Message}");
        }
    }

    private record FilaCarga(int Numero, string FechaStr, string Codigo, string CantidadStr, string Notas);
}
