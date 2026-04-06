namespace MrTamal.Shared.Models;

public class Catalogo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public TipoCatalogo Tipo { get; set; }
    public bool Activo { get; set; } = true;
}

public enum TipoCatalogo
{
    Ingreso = 1,
    Egreso = 2
}
