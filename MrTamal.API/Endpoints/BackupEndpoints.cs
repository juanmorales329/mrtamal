using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MrTamal.API.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SysColor = System.Drawing.Color;

namespace MrTamal.API.Endpoints;

public static class BackupEndpoints
{
    public static void MapBackupEndpoints(this WebApplication app)
    {
        app.MapGet("/api/backup", async (AppDbContext db) =>
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var pkg = new ExcelPackage();

            // Ingresos
            var ingresos = await db.Ingresos
                .Include(i => i.Catalogo).Include(i => i.Sucursal).Include(i => i.Usuario)
                .OrderByDescending(i => i.Fecha).ToListAsync();
            var wsI = pkg.Workbook.Worksheets.Add("Ingresos");
            var hI = new[] { "Id", "Fecha", "Sucursal", "Usuario", "Código", "Descripción", "Cantidad", "Notas", "CreadoEn" };
            WriteHeader(wsI, hI, SysColor.FromArgb(46, 125, 50));
            for (int r = 0; r < ingresos.Count; r++)
            {
                var i = ingresos[r];
                wsI.Cells[r+2,1].Value = i.Id;
                wsI.Cells[r+2,2].Value = i.Fecha.ToString("yyyy-MM-dd");
                wsI.Cells[r+2,3].Value = i.Sucursal?.Nombre ?? "";
                wsI.Cells[r+2,4].Value = i.Usuario?.Nombre ?? "";
                wsI.Cells[r+2,5].Value = i.Catalogo?.Codigo ?? "";
                wsI.Cells[r+2,6].Value = i.Catalogo?.Descripcion ?? "";
                wsI.Cells[r+2,7].Value = i.Cantidad;
                wsI.Cells[r+2,7].Style.Numberformat.Format = "#,##0.00";
                wsI.Cells[r+2,8].Value = i.Notas ?? "";
                wsI.Cells[r+2,9].Value = i.CreadoEn.ToString("yyyy-MM-dd HH:mm");
            }
            wsI.Cells[wsI.Dimension.Address].AutoFitColumns();

            // Egresos
            var egresos = await db.Egresos
                .Include(e => e.Catalogo).Include(e => e.Sucursal).Include(e => e.Usuario)
                .OrderByDescending(e => e.Fecha).ToListAsync();
            var wsE = pkg.Workbook.Worksheets.Add("Egresos");
            var hE = new[] { "Id", "Fecha", "Sucursal", "Usuario", "Código", "Descripción", "Cantidad", "Notas", "CreadoEn" };
            WriteHeader(wsE, hE, SysColor.FromArgb(198, 40, 40));
            for (int r = 0; r < egresos.Count; r++)
            {
                var e = egresos[r];
                wsE.Cells[r+2,1].Value = e.Id;
                wsE.Cells[r+2,2].Value = e.Fecha.ToString("yyyy-MM-dd");
                wsE.Cells[r+2,3].Value = e.Sucursal?.Nombre ?? "";
                wsE.Cells[r+2,4].Value = e.Usuario?.Nombre ?? "";
                wsE.Cells[r+2,5].Value = e.Catalogo?.Codigo ?? "";
                wsE.Cells[r+2,6].Value = e.Catalogo?.Descripcion ?? "";
                wsE.Cells[r+2,7].Value = e.Cantidad;
                wsE.Cells[r+2,7].Style.Numberformat.Format = "#,##0.00";
                wsE.Cells[r+2,8].Value = e.Notas ?? "";
                wsE.Cells[r+2,9].Value = e.CreadoEn.ToString("yyyy-MM-dd HH:mm");
            }
            wsE.Cells[wsE.Dimension.Address].AutoFitColumns();

            // Catálogos
            var catalogos = await db.Catalogos.OrderBy(c => c.Tipo).ThenBy(c => c.Codigo).ToListAsync();
            var wsC = pkg.Workbook.Worksheets.Add("Catálogos");
            WriteHeader(wsC, new[] { "Id", "Código", "Descripción", "Tipo", "Activo" }, SysColor.FromArgb(33, 150, 243));
            for (int r = 0; r < catalogos.Count; r++)
            {
                var c = catalogos[r];
                wsC.Cells[r+2,1].Value = c.Id; wsC.Cells[r+2,2].Value = c.Codigo;
                wsC.Cells[r+2,3].Value = c.Descripcion; wsC.Cells[r+2,4].Value = c.Tipo.ToString();
                wsC.Cells[r+2,5].Value = c.Activo ? "Sí" : "No";
            }
            wsC.Cells[wsC.Dimension.Address].AutoFitColumns();

            // Sucursales
            var sucursales = await db.Sucursales.ToListAsync();
            var wsS = pkg.Workbook.Worksheets.Add("Sucursales");
            WriteHeader(wsS, new[] { "Id", "Nombre", "País", "Moneda", "Símbolo", "Activa" }, SysColor.FromArgb(156, 39, 176));
            for (int r = 0; r < sucursales.Count; r++)
            {
                var s = sucursales[r];
                wsS.Cells[r+2,1].Value = s.Id; wsS.Cells[r+2,2].Value = s.Nombre;
                wsS.Cells[r+2,3].Value = s.Pais; wsS.Cells[r+2,4].Value = s.Moneda;
                wsS.Cells[r+2,5].Value = s.SimboloMoneda; wsS.Cells[r+2,6].Value = s.Activa ? "Sí" : "No";
            }
            wsS.Cells[wsS.Dimension.Address].AutoFitColumns();

            // Hoja resumen
            var wsR = pkg.Workbook.Worksheets.Add("Resumen");
            wsR.Cells[1,1].Value = "Backup MrTamal"; wsR.Cells[1,1].Style.Font.Bold = true; wsR.Cells[1,1].Style.Font.Size = 14;
            wsR.Cells[2,1].Value = $"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
            wsR.Cells[4,1].Value = "Tabla"; wsR.Cells[4,2].Value = "Registros";
            wsR.Cells[4,1].Style.Font.Bold = true; wsR.Cells[4,2].Style.Font.Bold = true;
            wsR.Cells[5,1].Value = "Ingresos"; wsR.Cells[5,2].Value = ingresos.Count;
            wsR.Cells[6,1].Value = "Egresos"; wsR.Cells[6,2].Value = egresos.Count;
            wsR.Cells[7,1].Value = "Catálogos"; wsR.Cells[7,2].Value = catalogos.Count;
            wsR.Cells[8,1].Value = "Sucursales"; wsR.Cells[8,2].Value = sucursales.Count;
            wsR.Cells[wsR.Dimension.Address].AutoFitColumns();

            var bytes = pkg.GetAsByteArray();
            var filename = $"backup_mrtamal_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";
            return Results.File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                filename);
        }).WithTags("Backup").RequireAuthorization(policy => policy.RequireRole("Admin"));
    }

    private static void WriteHeader(ExcelWorksheet ws, string[] headers, SysColor color)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i+1].Value = headers[i];
            ws.Cells[1, i+1].Style.Font.Bold = true;
            ws.Cells[1, i+1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, i+1].Style.Fill.BackgroundColor.SetColor(color);
            ws.Cells[1, i+1].Style.Font.Color.SetColor(SysColor.White);
        }
    }
}
