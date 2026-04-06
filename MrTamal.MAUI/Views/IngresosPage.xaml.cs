using MrTamal.MAUI.ViewModels;
using MrTamal.Shared.DTOs;

namespace MrTamal.MAUI.Views;

public partial class IngresosPage : ContentPage
{
    private readonly IngresosViewModel _vm;

    public IngresosPage(IngresosViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loader.IsVisible = Loader.IsRunning = true;
        await _vm.CargarAsync();
        Lista.ItemsSource = _vm.Ingresos;
        TotalLabel.Text = $"Total: Q{_vm.Total:N2}";
        Loader.IsVisible = Loader.IsRunning = false;
    }

    private async void OnNuevoClicked(object sender, EventArgs e) =>
        await MostrarFormulario(null);

    private async void OnEditarSwipe(object sender, EventArgs e)
    {
        if (sender is SwipeItem { BindingContext: MovimientoDto item })
            await MostrarFormulario(item);
    }

    private async void OnEliminarSwipe(object sender, EventArgs e)
    {
        if (sender is SwipeItem { BindingContext: MovimientoDto item })
        {
            await _vm.EliminarAsync(item);
            TotalLabel.Text = $"Total: Q{_vm.Total:N2}";
        }
    }

    private async Task MostrarFormulario(MovimientoDto? item)
    {
        if (!_vm.Catalogos.Any())
        {
            await DisplayAlert("Sin catálogos", "Primero crea tipos de ingreso en Catálogos.", "OK");
            return;
        }

        var opciones = _vm.Catalogos.Where(c => c.Activo).Select(c => $"[{c.Codigo}] {c.Descripcion}").ToArray();
        var seleccion = await DisplayActionSheet("Tipo de ingreso", "Cancelar", null, opciones);
        if (seleccion == "Cancelar" || seleccion is null) return;

        var catalogo = _vm.Catalogos.First(c => $"[{c.Codigo}] {c.Descripcion}" == seleccion);
        var cantidadStr = await DisplayPromptAsync("Cantidad", "Ingresa el monto:", initialValue: item?.Cantidad.ToString("N2") ?? "", keyboard: Keyboard.Numeric);
        if (!decimal.TryParse(cantidadStr, out var cantidad)) return;

        var notas = await DisplayPromptAsync("Notas", "Notas opcionales:", initialValue: item?.Notas ?? "");

        await _vm.GuardarAsync(DateTime.Today, catalogo.Id, cantidad, notas, item?.Id ?? 0);
        TotalLabel.Text = $"Total: Q{_vm.Total:N2}";
    }
}
