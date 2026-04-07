using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MrTamal.Shared.DTOs;

namespace MrTamal.Web.Services;

public class AuthService(HttpClient http, ILocalStorageService localStorage, AuthenticationStateProvider authProvider)
{
    private const string TokenKey = "authToken";
    private const string SimboloKey = "simboloMoneda";
    private const string RolKey = "userRol";

    public async Task<AuthResponse?> LoginAsync(LoginRequest req)
    {
        var resp = await http.PostAsJsonAsync("/api/auth/login", req);
        if (!resp.IsSuccessStatusCode) return null;
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is not null) await GuardarSesion(auth);
        return auth;
    }

    public async Task<AuthResponse?> SetupAsync(string nombre, string username, string email, string password)
    {
        var resp = await http.PostAsJsonAsync("/api/auth/setup",
            new { nombre, username, email, password });
        if (!resp.IsSuccessStatusCode) return null;
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is not null) await GuardarSesion(auth);
        return auth;
    }

    private async Task GuardarSesion(AuthResponse auth)
    {
        await localStorage.SetItemAsync(TokenKey, auth.Token);
        await localStorage.SetItemAsync(SimboloKey, auth.SimboloMoneda);
        await localStorage.SetItemAsync(RolKey, auth.Rol);
        ((JwtAuthStateProvider)authProvider).NotifyUserAuthentication(auth.Token, auth.SimboloMoneda);
    }

    public async Task<string> GetSimboloMonedaAsync() =>
        await localStorage.GetItemAsync<string>(SimboloKey) is string s && !string.IsNullOrEmpty(s) ? s : "$";

    public async Task<string> GetRolAsync() =>
        await localStorage.GetItemAsync<string>(RolKey) ?? "Usuario";

    public async Task LogoutAsync()
    {
        await localStorage.RemoveItemAsync(TokenKey);
        await localStorage.RemoveItemAsync(SimboloKey);
        await localStorage.RemoveItemAsync(RolKey);
        ((JwtAuthStateProvider)authProvider).NotifyUserLogout();
    }
}
