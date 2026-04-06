using System.Collections.ObjectModel;
using MrTamal.MAUI.Services;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.MAUI.ViewModels;

public class IngresosViewModel(ApiService api) : BaseViewModel
{
    public ObservableCollection<MovimientoDto> Ingresos { get; } = [];
    public List<CatalogoDto> Catalogos { get; private set; } = [];
    public decimal Total => Ingresos.Sum(i => i.Cantidad);

    public async Task CargarAsync()
    {
        IsBusy = true;
        Catalogos = await api.GetCatalogosAsync(TipoCatalogo.Ingreso) ?? [];
        var lista = await api.GetIngresosAsync() ?? [];
        Ingresos.Clear();
        foreach (var i in lista) Ingresos.Add(i);
        IsBusy = false;
        OnPropertyChanged(nameof(Total));
    }

    public async Task EliminarAsync(MovimientoDto item)
    {
        var ok = await Shell.Current.DisplayAlert("Confirmar", $"¿Eliminar ingreso de Q{item.Cantidad:N2}?", "Sí", "No");
        if (!ok) return;
        await api.DeleteIngresoAsync(item.Id);
        Ingresos.Remove(item);
        OnPropertyChanged(nameof(Total));
    }

    public async Task GuardarAsync(DateTime fecha, int catalogoId, decimal cantidad, string? notas, int editandoId = 0)
    {
        IsBusy = true;
        if (editandoId > 0)
            await api.UpdateIngresoAsync(editandoId, new(fecha, catalogoId, cantidad, notas));
        else
            await api.CreateIngresoAsync(new(fecha, catalogoId, cantidad, notas));
        await CargarAsync();
    }
}
