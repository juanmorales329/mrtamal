using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.Web.Services;

public class ApiService(HttpClient http, ILocalStorageService localStorage)
{
    private async Task EnsureTokenAsync()
    {
        var token = await localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Enviar sucursal activa como header
        var sucursalId = await localStorage.GetItemAsync<string>("sucursalActivaId");
        if (!string.IsNullOrEmpty(sucursalId))
        {
            http.DefaultRequestHeaders.Remove("X-Sucursal-Id");
            http.DefaultRequestHeaders.Add("X-Sucursal-Id", sucursalId);
        }
        else
        {
            http.DefaultRequestHeaders.Remove("X-Sucursal-Id");
        }
    }

    // Catalogos
    public async Task<List<CatalogoDto>?> GetCatalogosAsync(TipoCatalogo? tipo = null)
    {
        await EnsureTokenAsync();
        var url = tipo.HasValue ? $"/api/catalogos?tipo={(int)tipo}" : "/api/catalogos";
        return await http.GetFromJsonAsync<List<CatalogoDto>>(url);
    }

    public async Task<string?> GetSiguienteCodigoAsync(TipoCatalogo tipo)
    {
        await EnsureTokenAsync();
        var resp = await http.GetFromJsonAsync<SiguienteCodigoDto>($"/api/catalogos/siguiente-codigo/{(int)tipo}");
        return resp?.Codigo;
    }

    public async Task<CatalogoDto?> CreateCatalogoAsync(CreateCatalogoRequest req)
    { await EnsureTokenAsync(); return await PostAsync<CatalogoDto>("/api/catalogos", req); }

    public async Task<CatalogoDto?> UpdateCatalogoAsync(int id, UpdateCatalogoRequest req)
    { await EnsureTokenAsync(); return await PutAsync<CatalogoDto>($"/api/catalogos/{id}", req); }

    public async Task<bool> DeleteCatalogoAsync(int id)
    { await EnsureTokenAsync(); return await DeleteAsync($"/api/catalogos/{id}"); }

    // Ingresos
    public async Task<List<MovimientoDto>?> GetIngresosAsync(DateTime? desde = null, DateTime? hasta = null)
    { await EnsureTokenAsync(); return await http.GetFromJsonAsync<List<MovimientoDto>>(BuildUrl("/api/ingresos", desde, hasta)); }

    public async Task<MovimientoDto?> CreateIngresoAsync(CreateMovimientoRequest req)
    { await EnsureTokenAsync(); return await PostAsync<MovimientoDto>("/api/ingresos", req); }

    public async Task<MovimientoDto?> UpdateIngresoAsync(int id, UpdateMovimientoRequest req)
    { await EnsureTokenAsync(); return await PutAsync<MovimientoDto>($"/api/ingresos/{id}", req); }

    public async Task<bool> DeleteIngresoAsync(int id)
    { await EnsureTokenAsync(); return await DeleteAsync($"/api/ingresos/{id}"); }

    // Egresos
    public async Task<List<MovimientoDto>?> GetEgresosAsync(DateTime? desde = null, DateTime? hasta = null)
    { await EnsureTokenAsync(); return await http.GetFromJsonAsync<List<MovimientoDto>>(BuildUrl("/api/egresos", desde, hasta)); }

    public async Task<MovimientoDto?> CreateEgresoAsync(CreateMovimientoRequest req)
    { await EnsureTokenAsync(); return await PostAsync<MovimientoDto>("/api/egresos", req); }

    public async Task<MovimientoDto?> UpdateEgresoAsync(int id, UpdateMovimientoRequest req)
    { await EnsureTokenAsync(); return await PutAsync<MovimientoDto>($"/api/egresos/{id}", req); }

    public async Task<bool> DeleteEgresoAsync(int id)
    { await EnsureTokenAsync(); return await DeleteAsync($"/api/egresos/{id}"); }

    // Reportes
    public async Task<ReporteResumen?> GetReporteAsync(ReporteRequest req)
    { await EnsureTokenAsync(); return await PostAsync<ReporteResumen>("/api/reportes", req); }

    public async Task<byte[]?> GetReportePdfAsync(ReporteRequest req)
    {
        await EnsureTokenAsync();
        var resp = await http.PostAsJsonAsync("/api/reportes/pdf", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadAsByteArrayAsync() : null;
    }

    public async Task<byte[]?> GetProyectadoPdfAsync(int anio)
    {
        await EnsureTokenAsync();
        var resp = await http.GetAsync($"/api/proyectado/{anio}/pdf");
        return resp.IsSuccessStatusCode ? await resp.Content.ReadAsByteArrayAsync() : null;
    }

    public async Task<byte[]?> GetProyectadoExcelAsync(int anio)
    {
        await EnsureTokenAsync();
        var resp = await http.GetAsync($"/api/proyectado/{anio}/excel");
        return resp.IsSuccessStatusCode ? await resp.Content.ReadAsByteArrayAsync() : null;
    }

    public async Task<byte[]?> GetReporteExcelAsync(ReporteRequest req)
    {
        await EnsureTokenAsync();
        var resp = await http.PostAsJsonAsync("/api/reportes/excel", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadAsByteArrayAsync() : null;
    }
    public async Task<CargaResultado?> CargaMasivaTextoAsync(string tipo, string texto)
    {
        await EnsureTokenAsync();
        var content = new StringContent(texto, System.Text.Encoding.UTF8, "text/plain");
        var resp = await http.PostAsync($"/api/carga-masiva/{tipo}", content);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<CargaResultado>() : null;
    }

    public async Task<CargaResultado?> CargaMasivaExcelAsync(string tipo, Stream stream, string fileName)
    {
        await EnsureTokenAsync();
        using var content = new MultipartFormDataContent();
        using var sc = new StreamContent(stream);
        sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(sc, "archivo", fileName);
        var resp = await http.PostAsync($"/api/carga-masiva/{tipo}", content);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<CargaResultado>() : null;
    }

    public async Task<byte[]?> DescargarPlantillaAsync(string tipo)
    {
        await EnsureTokenAsync();
        var resp = await http.GetAsync($"/api/carga-masiva/plantilla/{tipo}");
        return resp.IsSuccessStatusCode ? await resp.Content.ReadAsByteArrayAsync() : null;
    }

    // Sucursales
    public async Task<List<SucursalDto>?> GetSucursalesAsync()
    { await EnsureTokenAsync(); return await http.GetFromJsonAsync<List<SucursalDto>>("/api/sucursales"); }

    public async Task<List<PaisDto>?> GetPaisesAsync()
    { await EnsureTokenAsync(); return await http.GetFromJsonAsync<List<PaisDto>>("/api/sucursales/paises"); }

    public async Task<SucursalDto?> CreateSucursalAsync(CreateSucursalRequest req)
    { await EnsureTokenAsync(); return await PostAsync<SucursalDto>("/api/sucursales", req); }

    public async Task<SucursalDto?> UpdateSucursalAsync(int id, UpdateSucursalRequest req)
    { await EnsureTokenAsync(); return await PutAsync<SucursalDto>($"/api/sucursales/{id}", req); }

    // Usuarios
    public async Task<List<UsuarioDto>?> GetUsuariosAsync()
    { await EnsureTokenAsync(); return await http.GetFromJsonAsync<List<UsuarioDto>>("/api/usuarios"); }

    public async Task<UsuarioDto?> CreateUsuarioAsync(CreateUsuarioRequest req)
    { await EnsureTokenAsync(); return await PostAsync<UsuarioDto>("/api/usuarios", req); }

    public async Task<UsuarioDto?> UpdateUsuarioAsync(int id, UpdateUsuarioRequest req)
    { await EnsureTokenAsync(); return await PutAsync<UsuarioDto>($"/api/usuarios/{id}", req); }

    public async Task<bool> DeleteUsuarioAsync(int id)
    { await EnsureTokenAsync(); return await DeleteAsync($"/api/usuarios/{id}"); }

    public async Task<bool> CambiarPasswordAsync(int id, string nuevaPassword)
    {
        await EnsureTokenAsync();
        var resp = await http.PutAsJsonAsync($"/api/usuarios/{id}/password", new { nuevoPassword = nuevaPassword });
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> AsignarSucursalAsync(int usuarioId, int sucursalId)
    {
        await EnsureTokenAsync();
        var resp = await http.PostAsJsonAsync($"/api/usuarios/{usuarioId}/asignar-sucursal",
            new { sucursalId });
        return resp.IsSuccessStatusCode;
    }

    // Helpers
    private async Task<T?> PostAsync<T>(string url, object body)
    {
        var resp = await http.PostAsJsonAsync(url, body);
        if (resp.IsSuccessStatusCode) return await resp.Content.ReadFromJsonAsync<T>();
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            throw new Exception((await resp.Content.ReadAsStringAsync()).Trim('"'));
        return default;
    }

    private async Task<T?> PutAsync<T>(string url, object body)
    {
        var resp = await http.PutAsJsonAsync(url, body);
        if (resp.IsSuccessStatusCode) return await resp.Content.ReadFromJsonAsync<T>();
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            throw new Exception((await resp.Content.ReadAsStringAsync()).Trim('"'));
        return default;
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

    public class CargaResultado
    {
        public int Guardados { get; set; }
        public List<string> Errores { get; set; } = [];
        public string Mensaje { get; set; } = "";
    }

    public class PaisDto
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Moneda { get; set; } = "";
        public string Simbolo { get; set; } = "";
    }

    private class SiguienteCodigoDto { public string Codigo { get; set; } = ""; }
}
