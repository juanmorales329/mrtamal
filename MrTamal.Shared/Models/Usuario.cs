namespace MrTamal.Shared.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = Roles.Usuario;
    public int? SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Gerente = "Gerente";
    public const string Contador = "Contador";
    public const string Usuario = "Usuario";

    // Permisos por rol
    public static readonly Dictionary<string, List<string>> Permisos = new()
    {
        [Admin] = ["inicio","ingresos","egresos","catalogos","reportes","carga-masiva","menu","sucursales","usuarios"],
        [Gerente] = ["inicio","ingresos","egresos","catalogos","reportes","carga-masiva","menu"],
        [Contador] = ["inicio","ingresos","egresos","reportes"],
        [Usuario] = ["inicio","ingresos","egresos"],
    };

    public static bool TienePermiso(string rol, string pagina) =>
        Permisos.TryGetValue(rol, out var perms) && perms.Contains(pagina);
}
