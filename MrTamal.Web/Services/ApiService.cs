using System.Net.Http.Json;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.Web.Services;

public class ApiService(HttpClient http)
{
    // Catalogos
    public Task<List<CatalogoDto>?> GetCatalogosAsync(TipoCatalogo? tipo = null)
    {
        var url = tipo.HasValue ? $"/api/catalogos?tipo={(int)tipo}" : "/api/catalogos";
        return http.GetFromJsonAsync<List<CatalogoDto>>(url);
    }

    public Task<CatalogoDto?> CreateCatalogoAsync(CreateCatalogoRequest req) =>
        PostAsync<CatalogoDto>("/api/catalogos", req);

    public Task<CatalogoDto?> UpdateCatalogoAsync(int id, UpdateCatalogoRequest req) =>
        PutAsync<CatalogoDto>($"/api/catalogos/{id}", req);

    public Task<bool> DeleteCatalogoAsync(int id) =>
        DeleteAsync($"/api/catalogos/{id}");

    // Ingresos
    public Task<List<MovimientoDto>?> GetIngresosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        var url = BuildUrl("/api/ingresos", desde, hasta);
        return http.GetFromJsonAsync<List<MovimientoDto>>(url);
    }

    public Task<MovimientoDto?> CreateIngresoAsync(CreateMovimientoRequest req) =>
        PostAsync<MovimientoDto>("/api/ingresos", req);

    public Task<MovimientoDto?> UpdateIngresoAsync(int id, UpdateMovimientoRequest req) =>
        PutAsync<MovimientoDto>($"/api/ingresos/{id}", req);

    public Task<bool> DeleteIngresoAsync(int id) => DeleteAsync($"/api/ingresos/{id}");

    // Egresos
    public Task<List<MovimientoDto>?> GetEgresosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        var url = BuildUrl("/api/egresos", desde, hasta);
        return http.GetFromJsonAsync<List<MovimientoDto>>(url);
    }

    public Task<MovimientoDto?> CreateEgresoAsync(CreateMovimientoRequest req) =>
        PostAsync<MovimientoDto>("/api/egresos", req);

    public Task<MovimientoDto?> UpdateEgresoAsync(int id, UpdateMovimientoRequest req) =>
        PutAsync<MovimientoDto>($"/api/egresos/{id}", req);

    public Task<bool> DeleteEgresoAsync(int id) => DeleteAsync($"/api/egresos/{id}");

    // Reportes
    public Task<ReporteResumen?> GetReporteAsync(ReporteRequest req) =>
        PostAsync<ReporteResumen>("/api/reportes", req);

    public async Task<byte[]?> GetReportePdfAsync(ReporteRequest req)
    {
        var resp = await http.PostAsJsonAsync("/api/reportes/pdf", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadAsByteArrayAsync() : null;
    }

    // Carga masiva
    public async Task<dynamic?> CargaMasivaTextoAsync(string tipo, string texto)
    {
        var content = new StringContent(texto, System.Text.Encoding.UTF8, "text/plain");
        var resp = await http.PostAsync($"/api/carga-masiva/{tipo}", content);
        return resp.IsSuccessStatusCode
            ? await resp.Content.ReadFromJsonAsync<CargaResultado>()
            : null;
    }

    public async Task<CargaResultado?> CargaMasivaExcelAsync(string tipo, Stream stream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        using var sc = new StreamContent(stream);
        sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(sc, "archivo", fileName);
        var resp = await http.PostAsync($"/api/carga-masiva/{tipo}", content);
        return resp.IsSuccessStatusCode
            ? await resp.Content.ReadFromJsonAsync<CargaResultado>()
            : null;
    }

    public async Task<byte[]?> DescargarPlantillaAsync(string tipo)
    {
        var resp = await http.GetAsync($"/api/carga-masiva/plantilla/{tipo}");
        return resp.IsSuccessStatusCode ? await resp.Content.ReadAsByteArrayAsync() : null;
    }

    public class CargaResultado
    {
        public int Guardados { get; set; }
        public List<string> Errores { get; set; } = [];
        public string Mensaje { get; set; } = "";
    }

    // Sucursales
    public Task<List<SucursalDto>?> GetSucursalesAsync() =>
        http.GetFromJsonAsync<List<SucursalDto>>("/api/sucursales");

    public Task<List<PaisDto>?> GetPaisesAsync() =>
        http.GetFromJsonAsync<List<PaisDto>>("/api/sucursales/paises");

    public async Task<SucursalDto?> CreateSucursalAsync(CreateSucursalRequest req) =>
        await PostAsync<SucursalDto>("/api/sucursales", req);

    public async Task<SucursalDto?> UpdateSucursalAsync(int id, UpdateSucursalRequest req) =>
        await PutAsync<SucursalDto>($"/api/sucursales/{id}", req);

    // Usuarios
    public Task<List<UsuarioDto>?> GetUsuariosAsync() =>
        http.GetFromJsonAsync<List<UsuarioDto>>("/api/usuarios");

    public async Task<UsuarioDto?> CreateUsuarioAsync(CreateUsuarioRequest req) =>
        await PostAsync<UsuarioDto>("/api/usuarios", req);

    public async Task<UsuarioDto?> UpdateUsuarioAsync(int id, UpdateUsuarioRequest req) =>
        await PutAsync<UsuarioDto>($"/api/usuarios/{id}", req);

    public async Task<bool> DeleteUsuarioAsync(int id) =>
        (await http.DeleteAsync($"/api/usuarios/{id}")).IsSuccessStatusCode;

    public async Task<bool> CambiarPasswordAsync(int id, string nuevaPassword)
    {
        var resp = await http.PutAsJsonAsync($"/api/usuarios/{id}/password", new { nuevoPassword = nuevaPassword });
        return resp.IsSuccessStatusCode;
    }

    public class PaisDto
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Moneda { get; set; } = "";
        public string Simbolo { get; set; } = "";
    }

    // Helpers
    private async Task<T?> PostAsync<T>(string url, object body)
    {
        var resp = await http.PostAsJsonAsync(url, body);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<T>() : default;
    }

    private async Task<T?> PutAsync<T>(string url, object body)
    {
        var resp = await http.PutAsJsonAsync(url, body);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<T>() : default;
    }

    private async Task<bool> DeleteAsync(string url)
    {
        var resp = await http.DeleteAsync(url);
        return resp.IsSuccessStatusCode;
    }

    private static string BuildUrl(string base_, DateTime? desde, DateTime? hasta)
    {
        var parts = new List<string>();
        if (desde.HasValue) parts.Add($"desde={desde.Value:yyyy-MM-dd}");
        if (hasta.HasValue) parts.Add($"hasta={hasta.Value:yyyy-MM-dd}");
        return parts.Any() ? $"{base_}?{string.Join("&", parts)}" : base_;
    }
}
