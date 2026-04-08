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
    private const string UserIdKey = "userId";
    private const string UserNameKey = "userName";
    private const string SucursalActivaIdKey = "sucursalActivaId";
    private const string SucursalActivaNombreKey = "sucursalActivaNombre";
    private const string SucursalActivaSimboloKey = "sucursalActivaSimbolo";

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
        await localStorage.SetItemAsync(UserIdKey, auth.Id.ToString());
        await localStorage.SetItemAsync(UserNameKey, auth.Nombre);
        ((JwtAuthStateProvider)authProvider).NotifyUserAuthentication(auth.Token, auth.SimboloMoneda);
    }

    public async Task SetSucursalActivaAsync(int id, string nombre, string simbolo)
    {
        await localStorage.SetItemAsync(SucursalActivaIdKey, id.ToString());
        await localStorage.SetItemAsync(SucursalActivaNombreKey, nombre);
        await localStorage.SetItemAsync(SucursalActivaSimboloKey, simbolo);
        await localStorage.SetItemAsync(SimboloKey, simbolo);
    }

    public async Task<int?> GetSucursalActivaIdAsync()
    {
        var val = await localStorage.GetItemAsync<string>(SucursalActivaIdKey);
        return int.TryParse(val, out var id) ? id : null;
    }

    public async Task<string> GetSucursalActivaNombreAsync() =>
        await localStorage.GetItemAsync<string>(SucursalActivaNombreKey) ?? "";

    public async Task<string> GetSimboloMonedaAsync()
    {
        var s = await localStorage.GetItemAsync<string>(SucursalActivaSimboloKey);
        if (!string.IsNullOrEmpty(s)) return s;
        s = await localStorage.GetItemAsync<string>(SimboloKey);
        return !string.IsNullOrEmpty(s) ? s : "$";
    }

    public async Task<string> GetRolAsync() =>
        await localStorage.GetItemAsync<string>(RolKey) ?? "Usuario";

    public async Task<string> GetUserIdAsync() =>
        await localStorage.GetItemAsync<string>(UserIdKey) ?? "0";

    public async Task LogoutAsync()
    {
        await localStorage.RemoveItemAsync(TokenKey);
        await localStorage.RemoveItemAsync(SimboloKey);
        await localStorage.RemoveItemAsync(RolKey);
        await localStorage.RemoveItemAsync(UserIdKey);
        await localStorage.RemoveItemAsync(UserNameKey);
        await localStorage.RemoveItemAsync(SucursalActivaIdKey);
        await localStorage.RemoveItemAsync(SucursalActivaNombreKey);
        await localStorage.RemoveItemAsync(SucursalActivaSimboloKey);
        ((JwtAuthStateProvider)authProvider).NotifyUserLogout();
    }
}
