namespace MrTamal.Shared.DTOs;

public record SucursalDto(int Id, string Nombre, string Pais, string Moneda, string SimboloMoneda, string? Direccion, string? Telefono, bool Activa);
public record CreateSucursalRequest(string Nombre, string Pais, string? Direccion, string? Telefono);
public record UpdateSucursalRequest(string Nombre, string Pais, string? Direccion, string? Telefono, bool Activa);
