using MrTamal.MAUI.Services;
using MrTamal.Shared.DTOs;

namespace MrTamal.MAUI.ViewModels;

public class DashboardViewModel(ApiService api, SecureStorageService storage) : BaseViewModel
{
    private ReporteResumen? _resumen;

    public string Bienvenida => $"Hola, {storage.GetNombre()}";
    public decimal TotalIngresos => _resumen?.TotalIngresos ?? 0;
    public decimal TotalEgresos => _resumen?.TotalEgresos ?? 0;
    public decimal Balance => _resumen?.Balance ?? 0;
    public string ColorBalance => Balance >= 0 ? "#4CAF50" : "#F44336";
    public List<ReporteDetalle> UltimosIngresos => _resumen?.Ingresos.TakeLast(5).ToList() ?? [];
    public List<ReporteDetalle> UltimosEgresos => _resumen?.Egresos.TakeLast(5).ToList() ?? [];

    public async Task CargarAsync()
    {
        IsBusy = true;
        _resumen = await api.GetReporteAsync(new(TipoReporte.Mensual, null, null, null, null, null));
        IsBusy = false;
        OnPropertyChanged(nameof(TotalIngresos));
        OnPropertyChanged(nameof(TotalEgresos));
        OnPropertyChanged(nameof(Balance));
        OnPropertyChanged(nameof(ColorBalance));
        OnPropertyChanged(nameof(UltimosIngresos));
        OnPropertyChanged(nameof(UltimosEgresos));
        OnPropertyChanged(nameof(Bienvenida));
    }
}
