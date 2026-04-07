namespace MrTamal.Shared.DTOs;

public record LoginRequest(string Username, string Password);
public record AuthResponse(string Token, string Nombre, string Username, string Rol, int? SucursalId, string SimboloMoneda, int Id = 0);
