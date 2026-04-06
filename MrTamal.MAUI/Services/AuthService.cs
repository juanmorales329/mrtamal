using System.Net.Http.Headers;
using System.Net.Http.Json;
using MrTamal.Shared.DTOs;

namespace MrTamal.MAUI.Services;

public class AuthService(HttpClient http, SecureStorageService storage)
{
    public async Task<(bool ok, string? error)> LoginAsync(string email, string password)
    {
        try
        {
            var resp = await http.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
            if (!resp.IsSuccessStatusCode) return (false, "Email o contraseña incorrectos.");
            var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth is null) return (false, "Error al procesar respuesta.");
            storage.Guardar(auth.Token, auth.Nombre, auth.Email);
            SetToken(auth.Token);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool ok, string? error)> RegisterAsync(string nombre, string email, string password)
    {
        try
        {
            var resp = await http.PostAsJsonAsync("/api/auth/register", new RegisterRequest(nombre, email, password));
            if (!resp.IsSuccessStatusCode) return (false, "No se pudo registrar. El email puede estar en uso.");
            var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth is null) return (false, "Error al procesar respuesta.");
            storage.Guardar(auth.Token, auth.Nombre, auth.Email);
            SetToken(auth.Token);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public void SetToken(string token) =>
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public void InicializarToken()
    {
        var token = storage.GetToken();
        if (!string.IsNullOrEmpty(token)) SetToken(token);
    }

    public void Logout()
    {
        storage.Limpiar();
        http.DefaultRequestHeaders.Authorization = null;
    }
}
