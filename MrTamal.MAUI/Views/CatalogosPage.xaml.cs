using MrTamal.MAUI.ViewModels;
using MrTamal.Shared.DTOs;
using MrTamal.Shared.Models;

namespace MrTamal.MAUI.Views;

public partial class CatalogosPage : ContentPage
{
    private readonly CatalogosViewModel _vm;

    public CatalogosPage(CatalogosViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CargarAsync();
        Lista.ItemsSource = _vm.Catalogos;
    }

    private async void OnNuevoClicked(object sender, EventArgs e)
    {
        var tipoStr = await DisplayActionSheet("Tipo", "Cancelar", null, "Ingreso", "Egreso");
        if (tipoStr == "Cancelar" || tipoStr is null) return;
        var tipo = tipoStr == "Ingreso" ? TipoCatalogo.Ingreso : TipoCatalogo.Egreso;

        var codigo = await DisplayPromptAsync("Código", "Ej: SAL, ALQ, COM", maxLength: 10);
        if (string.IsNullOrWhiteSpace(codigo)) return;

        var descripcion = await DisplayPromptAsync("Descripción", "Descripción del tipo");
        if (string.IsNullOrWhiteSpace(descripcion)) return;

        await _vm.CrearAsync(codigo.ToUpper(), descripcion, tipo);
    }

    private async void OnEditarSwipe(object sender, EventArgs e)
    {
        if (sender is not SwipeItem { BindingContext: CatalogoDto cat }) return;

        var codigo = await DisplayPromptAsync("Código", "Código:", initialValue: cat.Codigo, maxLength: 10);
        if (string.IsNullOrWhiteSpace(codigo)) return;

        var descripcion = await DisplayPromptAsync("Descripción", "Descripción:", initialValue: cat.Descripcion);
        if (string.IsNullOrWhiteSpace(descripcion)) return;

        var estadoStr = await DisplayActionSheet("Estado", "Cancelar", null, "Activo", "Inactivo");
        if (estadoStr == "Cancelar" || estadoStr is null) return;

        await _vm.ActualizarAsync(cat.Id, codigo.ToUpper(), descripcion, estadoStr == "Activo");
    }
}
