using MrTamal.MAUI.ViewModels;

namespace MrTamal.MAUI.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        _vm.Email = EmailEntry.Text ?? "";
        _vm.Password = PasswordEntry.Text ?? "";
        Loader.IsVisible = Loader.IsRunning = true;
        await _vm.LoginAsync();
        Loader.IsVisible = Loader.IsRunning = false;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        _vm.Email = EmailEntry.Text ?? "";
        _vm.Password = PasswordEntry.Text ?? "";
        Loader.IsVisible = Loader.IsRunning = true;
        await _vm.RegisterAsync();
        Loader.IsVisible = Loader.IsRunning = false;
    }
}
