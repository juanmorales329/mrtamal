using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace MrTamal.Web.Services;

public class JwtAuthStateProvider(ILocalStorageService localStorage, HttpClient http) : AuthenticationStateProvider
{
    private const string TokenKey = "authToken";
    private const string SimboloKey = "simboloMoneda";
    private readonly AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorage.GetItemAsync<string>(TokenKey);
        if (string.IsNullOrWhiteSpace(token)) return _anonymous;

        var claims = ParseClaimsFromJwt(token);
        var expiry = claims.FirstOrDefault(c => c.Type == "exp");
        if (expiry is not null && DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry.Value)) < DateTimeOffset.UtcNow)
        {
            await localStorage.RemoveItemAsync(TokenKey);
            return _anonymous;
        }

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
    }

    public void NotifyUserAuthentication(string token, string simbolo)
    {
        var claims = ParseClaimsFromJwt(token);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var state = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    public void NotifyUserLogout()
    {
        http.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }

    public string GetRol(ClaimsPrincipal user) =>
        user.Claims.FirstOrDefault(c => c.Type == "rol")?.Value
        ?? user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
        ?? "Usuario";

    private static IEnumerable<Claim> ParseClaimsFromJwt(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims;
    }
}
