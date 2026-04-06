using MrTamal.MAUI.ViewModels;

namespace MrTamal.MAUI.Views;

public partial class ReportesPage : ContentPage
{
    private readonly ReportesViewModel _vm;

    public ReportesPage(ReportesViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        TipoPicker.SelectedIndex = 2; // Mensual por defecto
    }

    private void OnTipoChanged(object sender, EventArgs e)
    {
        var idx = TipoPicker.SelectedIndex;
        _vm.TipoSeleccionado = idx;
        AnioPanel.IsVisible = idx >= 3;
        AnioCompLabel.IsVisible = idx == 3;
        AnioCompEntry.IsVisible = idx == 3;
    }

    private async void OnGenerarClicked(object sender, EventArgs e)
    {
        if (int.TryParse(AnioEntry.Text, out var anio)) _vm.Anio = anio;
        if (int.TryParse(AnioCompEntry.Text, out var anioComp)) _vm.AnioComparacion = anioComp;

        Loader.IsVisible = Loader.IsRunning = true;
        await _vm.GenerarAsync();
        Loader.IsVisible = Loader.IsRunning = false;

        ResumenPanel.IsVisible = true;
        PdfBtn.IsVisible = true;
        IngresosResLabel.Text = $"Q{_vm.TotalIngresos:N2}";
        EgresosResLabel.Text = $"Q{_vm.TotalEgresos:N2}";
        BalanceResLabel.Text = $"Q{_vm.Balance:N2}";
        BalanceResLabel.TextColor = Color.FromArgb(_vm.ColorBalance);

        CompPanel.IsVisible = _vm.TieneComparaciones;
        CompLista.ItemsSource = _vm.Comparaciones;
    }

    private async void OnPdfClicked(object sender, EventArgs e)
    {
        Loader.IsVisible = Loader.IsRunning = true;
        await _vm.DescargarPdfAsync();
        Loader.IsVisible = Loader.IsRunning = false;
    }
}
