namespace MrTamal.MAUI;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = services;
    }
}
