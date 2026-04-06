namespace MrTamal.Shared.DTOs;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Nombre, string Email, string Password);
public record AuthResponse(string Token, string Nombre, string Email, string Rol, int? SucursalId, string SimboloMoneda);
