namespace MrTamal.Shared.DTOs;

public record UsuarioDto(int Id, string Nombre, string Email, string Rol, int? SucursalId, string? SucursalNombre, bool Activo);
public record UpdateUsuarioRequest(string Nombre, string Rol, int? SucursalId, bool Activo);
