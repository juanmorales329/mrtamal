namespace MrTamal.Shared.Models;

public class Sucursal
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Pais { get; set; } = "US";
    public string Moneda { get; set; } = "USD";
    public string SimboloMoneda { get; set; } = "$";
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

// Catálogo de monedas por país
public static class MonedasPorPais
{
    public static readonly Dictionary<string, (string Moneda, string Simbolo, string Nombre)> Catalogo = new()
    {
        { "US", ("USD", "$",  "Dólar Estadounidense") },
        { "MX", ("MXN", "$", "Peso Mexicano") },
        { "GT", ("GTQ", "Q", "Quetzal Guatemalteco") },
        { "HN", ("HNL", "L", "Lempira Hondureño") },
        { "SV", ("USD", "$", "Dólar (El Salvador)") },
        { "CR", ("CRC", "₡", "Colón Costarricense") },
        { "PA", ("USD", "$", "Dólar (Panamá)") },
        { "CO", ("COP", "$", "Peso Colombiano") },
        { "PE", ("PEN", "S/", "Sol Peruano") },
        { "AR", ("ARS", "$", "Peso Argentino") },
        { "CL", ("CLP", "$", "Peso Chileno") },
        { "ES", ("EUR", "€", "Euro (España)") },
        { "CA", ("CAD", "$", "Dólar Canadiense") },
    };
}
