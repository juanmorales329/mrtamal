using System.Collections.ObjectModel;
using MrTamal.MAUI.Services;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.MAUI.ViewModels;

public class CatalogosViewModel(ApiService api) : BaseViewModel
{
    public ObservableCollection<CatalogoDto> Catalogos { get; } = [];

    public async Task CargarAsync()
    {
        IsBusy = true;
        var lista = await api.GetCatalogosAsync() ?? [];
        Catalogos.Clear();
        foreach (var c in lista) Catalogos.Add(c);
        IsBusy = false;
    }

    public async Task CrearAsync(string codigo, string descripcion, TipoCatalogo tipo)
    {
        IsBusy = true;
        await api.CreateCatalogoAsync(new(codigo, descripcion, tipo));
        await CargarAsync();
    }

    public async Task ActualizarAsync(int id, string codigo, string descripcion, bool activo)
    {
        IsBusy = true;
        await api.UpdateCatalogoAsync(id, new(codigo, descripcion, activo));
        await CargarAsync();
    }
}
