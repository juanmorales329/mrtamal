using MrTamal.MAUI.Services;
using MrTamal.MAUI.Views;

namespace MrTamal.MAUI.ViewModels;

public class LoginViewModel(AuthService authSvc, IServiceProvider services) : BaseViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlert("Error", "Ingresa email y contraseña.", "OK");
            return;
        }
        IsBusy = true;
        var (ok, error) = await authSvc.LoginAsync(Email, Password);
        IsBusy = false;
        if (ok)
            Application.Current!.Windows[0].Page = new AppShell(services);
        else
            await Shell.Current.DisplayAlert("Error", error, "OK");
    }

    public async Task RegisterAsync()
    {
        var nombre = await Shell.Current.DisplayPromptAsync("Registro", "Tu nombre:");
        if (string.IsNullOrWhiteSpace(nombre)) return;
        IsBusy = true;
        var (ok, error) = await authSvc.RegisterAsync(nombre, Email, Password);
        IsBusy = false;
        if (ok)
            Application.Current!.Windows[0].Page = new AppShell(services);
        else
            await Shell.Current.DisplayAlert("Error", error, "OK");
    }
}
