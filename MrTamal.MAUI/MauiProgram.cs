using Microsoft.Extensions.Logging;
using MrTamal.MAUI.Services;
using MrTamal.MAUI.ViewModels;
using MrTamal.MAUI.Views;

namespace MrTamal.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // HTTP
        builder.Services.AddSingleton<HttpClient>(_ => new HttpClient
        {
            BaseAddress = new Uri("https://10.0.2.2:7163") // Android emulator → localhost
        });

        // Services
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<SecureStorageService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<IngresosViewModel>();
        builder.Services.AddTransient<EgresosViewModel>();
        builder.Services.AddTransient<CatalogosViewModel>();
        builder.Services.AddTransient<ReportesViewModel>();

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<IngresosPage>();
        builder.Services.AddTransient<EgresosPage>();
        builder.Services.AddTransient<CatalogosPage>();
        builder.Services.AddTransient<ReportesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
