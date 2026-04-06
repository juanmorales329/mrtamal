namespace MrTamal.Shared.Models;

/// <summary>
/// Historial de asignaciones de un usuario a sucursales.
/// Permite saber en qué sucursal estuvo cada persona y en qué fechas.
/// </summary>
public class AsignacionSucursal
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }  // null = actualmente en esta sucursal
    public string? Motivo { get; set; }       // traslado, apertura, etc.
    public bool Activa { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
