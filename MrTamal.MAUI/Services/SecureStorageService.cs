namespace MrTamal.MAUI.Services;

public class SecureStorageService
{
    private const string TokenKey = "auth_token";
    private const string NombreKey = "user_nombre";
    private const string EmailKey = "user_email";

    public string? GetToken() => Preferences.Get(TokenKey, null);
    public string? GetNombre() => Preferences.Get(NombreKey, null);
    public string? GetEmail() => Preferences.Get(EmailKey, null);

    public void Guardar(string token, string nombre, string email)
    {
        Preferences.Set(TokenKey, token);
        Preferences.Set(NombreKey, nombre);
        Preferences.Set(EmailKey, email);
    }

    public void Limpiar()
    {
        Preferences.Remove(TokenKey);
        Preferences.Remove(NombreKey);
        Preferences.Remove(EmailKey);
    }

    public bool EstaAutenticado() => !string.IsNullOrEmpty(GetToken());
}
