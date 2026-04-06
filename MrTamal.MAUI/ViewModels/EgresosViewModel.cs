using System.Collections.ObjectModel;
using MrTamal.MAUI.Services;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.MAUI.ViewModels;

public class EgresosViewModel(ApiService api) : BaseViewModel
{
    public ObservableCollection<MovimientoDto> Egresos { get; } = [];
    public List<CatalogoDto> Catalogos { get; private set; } = [];
    public decimal Total => Egresos.Sum(e => e.Cantidad);

    public async Task CargarAsync()
    {
        IsBusy = true;
        Catalogos = await api.GetCatalogosAsync(TipoCatalogo.Egreso) ?? [];
        var lista = await api.GetEgresosAsync() ?? [];
        Egresos.Clear();
        foreach (var e in lista) Egresos.Add(e);
        IsBusy = false;
        OnPropertyChanged(nameof(Total));
    }

    public async Task EliminarAsync(MovimientoDto item)
    {
        var ok = await Shell.Current.DisplayAlert("Confirmar", $"¿Eliminar egreso de Q{item.Cantidad:N2}?", "Sí", "No");
        if (!ok) return;
        await api.DeleteEgresoAsync(item.Id);
        Egresos.Remove(item);
        OnPropertyChanged(nameof(Total));
    }

    public async Task GuardarAsync(DateTime fecha, int catalogoId, decimal cantidad, string? notas, int editandoId = 0)
    {
        IsBusy = true;
        if (editandoId > 0)
            await api.UpdateEgresoAsync(editandoId, new(fecha, catalogoId, cantidad, notas));
        else
            await api.CreateEgresoAsync(new(fecha, catalogoId, cantidad, notas));
        await CargarAsync();
    }
}
