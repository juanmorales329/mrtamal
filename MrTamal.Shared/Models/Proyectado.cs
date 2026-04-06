namespace MrTamal.Shared.Models;

public class MetaAnual
{
    public int Id { get; set; }
    public int Anio { get; set; }
    public decimal MetaVentas { get; set; }
    public bool ExcluirDomingos { get; set; } = true;
    public int? SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
    public int UsuarioId { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

public class DiaNolaboral
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string? Descripcion { get; set; }
    public int? SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
    public int UsuarioId { get; set; }
}
