using MrTamal.MAUI.Services;
using MrTamal.MAUI.Views;

namespace MrTamal.MAUI;

public partial class App : Application
{
    private readonly SecureStorageService _storage;

    public App(SecureStorageService storage)
    {
        InitializeComponent();
        _storage = storage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var token = _storage.GetToken();
        Page startPage = string.IsNullOrEmpty(token)
            ? Handler!.MauiContext!.Services.GetRequiredService<LoginPage>()
            : (Page)new AppShell(Handler!.MauiContext!.Services);

        return new Window(startPage);
    }
}
