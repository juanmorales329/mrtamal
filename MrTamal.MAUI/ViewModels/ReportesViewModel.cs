using System.Collections.ObjectModel;
using MrTamal.MAUI.Services;
using MrTamal.Shared.DTOs;

namespace MrTamal.MAUI.ViewModels;

public class ReportesViewModel(ApiService api) : BaseViewModel
{
    private ReporteResumen? _resumen;

    public int TipoSeleccionado { get; set; } = 2; // Mensual por defecto
    public int Anio { get; set; } = DateTime.Now.Year;
    public int AnioComparacion { get; set; } = DateTime.Now.Year - 1;

    public decimal TotalIngresos => _resumen?.TotalIngresos ?? 0;
    public decimal TotalEgresos => _resumen?.TotalEgresos ?? 0;
    public decimal Balance => _resumen?.Balance ?? 0;
    public string ColorBalance => Balance >= 0 ? "#4CAF50" : "#F44336";

    public ObservableCollection<ReporteDetalle> Ingresos { get; } = [];
    public ObservableCollection<ReporteDetalle> Egresos { get; } = [];
    public ObservableCollection<ReporteComparacion> Comparaciones { get; } = [];
    public bool TieneComparaciones => Comparaciones.Any();

    public async Task GenerarAsync()
    {
        IsBusy = true;
        var req = new ReporteRequest(
            (TipoReporte)TipoSeleccionado, null, null,
            TipoSeleccionado >= 3 ? Anio : null, null,
            TipoSeleccionado == 3 ? AnioComparacion : null
        );
        _resumen = await api.GetReporteAsync(req);
        if (_resumen is not null)
        {
            Ingresos.Clear(); foreach (var i in _resumen.Ingresos) Ingresos.Add(i);
            Egresos.Clear(); foreach (var e in _resumen.Egresos) Egresos.Add(e);
            Comparaciones.Clear(); foreach (var c in _resumen.Comparaciones ?? []) Comparaciones.Add(c);
        }
        IsBusy = false;
        OnPropertyChanged(nameof(TotalIngresos));
        OnPropertyChanged(nameof(TotalEgresos));
        OnPropertyChanged(nameof(Balance));
        OnPropertyChanged(nameof(ColorBalance));
        OnPropertyChanged(nameof(TieneComparaciones));
    }

    public async Task DescargarPdfAsync()
    {
        IsBusy = true;
        var req = new ReporteRequest(
            (TipoReporte)TipoSeleccionado, null, null,
            TipoSeleccionado >= 3 ? Anio : null, null,
            TipoSeleccionado == 3 ? AnioComparacion : null
        );
        var pdf = await api.GetReportePdfAsync(req);
        if (pdf is not null)
        {
            var path = Path.Combine(FileSystem.CacheDirectory, $"reporte_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            await File.WriteAllBytesAsync(path, pdf);
            await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
        }
        IsBusy = false;
    }
}
