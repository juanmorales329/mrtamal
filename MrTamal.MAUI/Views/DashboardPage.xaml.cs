using MrTamal.MAUI.ViewModels;

namespace MrTamal.MAUI.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _vm;

    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CargarAsync();
        ActualizarUI();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await _vm.CargarAsync();
        ActualizarUI();
        Refresher.IsRefreshing = false;
    }

    private void ActualizarUI()
    {
        BienvenidaLabel.Text = _vm.Bienvenida;
        IngresosLabel.Text = $"Q{_vm.TotalIngresos:N2}";
        EgresosLabel.Text = $"Q{_vm.TotalEgresos:N2}";
        BalanceLabel.Text = $"Q{_vm.Balance:N2}";
        BalanceLabel.TextColor = Color.FromArgb(_vm.ColorBalance);
        IngresosRecientes.ItemsSource = _vm.UltimosIngresos;
        EgresosRecientes.ItemsSource = _vm.UltimosEgresos;
    }
}
