using System.Net.Http.Json;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.MAUI.Services;

public class ApiService(HttpClient http)
{
    public Task<List<CatalogoDto>?> GetCatalogosAsync(TipoCatalogo? tipo = null)
    {
        var url = tipo.HasValue ? $"/api/catalogos?tipo={(int)tipo}" : "/api/catalogos";
        return http.GetFromJsonAsync<List<CatalogoDto>>(url);
    }

    public Task<List<MovimientoDto>?> GetIngresosAsync(DateTime? desde = null, DateTime? hasta = null) =>
        http.GetFromJsonAsync<List<MovimientoDto>>(BuildUrl("/api/ingresos", desde, hasta));

    public Task<List<MovimientoDto>?> GetEgresosAsync(DateTime? desde = null, DateTime? hasta = null) =>
        http.GetFromJsonAsync<List<MovimientoDto>>(BuildUrl("/api/egresos", desde, hasta));

    public async Task<bool> CreateIngresoAsync(CreateMovimientoRequest req)
    {
        var r = await http.PostAsJsonAsync("/api/ingresos", req);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateIngresoAsync(int id, UpdateMovimientoRequest req)
    {
        var r = await http.PutAsJsonAsync($"/api/ingresos/{id}", req);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteIngresoAsync(int id) =>
        (await http.DeleteAsync($"/api/ingresos/{id}")).IsSuccessStatusCode;

    public async Task<bool> CreateEgresoAsync(CreateMovimientoRequest req)
    {
        var r = await http.PostAsJsonAsync("/api/egresos", req);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateEgresoAsync(int id, UpdateMovimientoRequest req)
    {
        var r = await http.PutAsJsonAsync($"/api/egresos/{id}", req);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteEgresoAsync(int id) =>
        (await http.DeleteAsync($"/api/egresos/{id}")).IsSuccessStatusCode;

    public async Task<bool> CreateCatalogoAsync(CreateCatalogoRequest req)
    {
        var r = await http.PostAsJsonAsync("/api/catalogos", req);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCatalogoAsync(int id, UpdateCatalogoRequest req)
    {
        var r = await http.PutAsJsonAsync($"/api/catalogos/{id}", req);
        return r.IsSuccessStatusCode;
    }

    public Task<ReporteResumen?> GetReporteAsync(ReporteRequest req) =>
        PostAsync<ReporteResumen>("/api/reportes", req);

    public async Task<byte[]?> GetReportePdfAsync(ReporteRequest req)
    {
        var r = await http.PostAsJsonAsync("/api/reportes/pdf", req);
        return r.IsSuccessStatusCode ? await r.Content.ReadAsByteArrayAsync() : null;
    }

    private async Task<T?> PostAsync<T>(string url, object body)
    {
        var r = await http.PostAsJsonAsync(url, body);
        return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<T>() : default;
    }

    private static string BuildUrl(string baseUrl, DateTime? desde, DateTime? hasta)
    {
        var parts = new List<string>();
        if (desde.HasValue) parts.Add($"desde={desde.Value:yyyy-MM-dd}");
        if (hasta.HasValue) parts.Add($"hasta={hasta.Value:yyyy-MM-dd}");
        return parts.Any() ? $"{baseUrl}?{string.Join("&", parts)}" : baseUrl;
    }
}
