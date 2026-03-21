using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixOverview.UI;

public partial class RadixOverviewScreen : UserControl
{
    private RadixOverviewViewModel? _vm;

    public RadixOverviewScreen()
    {
        InitializeComponent();

        if (Design.IsDesignMode || Application.Current is not App app)
            return;

        var chartSession        = app.Services.GetRequiredService<IChartSession>();
        var navigationService   = app.Services.GetRequiredService<INavigationService>();
        var rosetta             = app.Services.GetRequiredService<IRosetta>();
        var horoscopeRepository = app.Services.GetRequiredService<IHoroscopeRepository>();

        _vm = new RadixOverviewViewModel(chartSession, navigationService, rosetta, horoscopeRepository);
        DataContext = _vm;
    }

    private void OnNewChartClicked(object? sender, RoutedEventArgs e)
    {
        _vm?.NewChart();
    }

    private void OnSearchChartClicked(object? sender, RoutedEventArgs e)
    {
        _vm?.SearchChart();
    }

    private void OnSelectClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null && sender is Button { DataContext: SessionChartRow row })
            _vm.Select(row);
    }

    private async void OnEditClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null && sender is Button { DataContext: SessionChartRow row })
            await _vm.EditAsync(row);
    }

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new RadixOverviewHelpWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await helpWindow.ShowDialog(owner);
    }
}
