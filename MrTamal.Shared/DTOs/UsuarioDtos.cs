namespace MrTamal.Shared.DTOs;

public record UsuarioDto(int Id, string Nombre, string Username, string Email, string Rol, int? SucursalId, string? SucursalNombre, bool Activo);
public record CreateUsuarioRequest(string Nombre, string Username, string Email, string Password, string Rol, int? SucursalId);
public record UpdateUsuarioRequest(string Nombre, string Rol, int? SucursalId, bool Activo);
public record CambiarPasswordRequest(string NuevoPassword);
