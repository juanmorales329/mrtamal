namespace MrTamal.Shared.Models;

public class Ingreso
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int CatalogoId { get; set; }
    public Catalogo? Catalogo { get; set; }
    public decimal Cantidad { get; set; }
    public string? Notas { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public int? SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
