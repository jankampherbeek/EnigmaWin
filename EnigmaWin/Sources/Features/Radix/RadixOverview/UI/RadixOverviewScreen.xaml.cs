// RadixOverviewScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixOverview.UI;

public partial class RadixOverviewScreen : UserControl
{
    private RadixOverviewViewModel? _vm;

    public RadixOverviewScreen()
    {
        InitializeComponent();

        if (Application.Current is not App app) return;

        var chartSession        = app.Services.GetRequiredService<IChartSession>();
        var navigationService   = app.Services.GetRequiredService<INavigationService>();
        var rosetta             = app.Services.GetRequiredService<IRosetta>();
        var horoscopeRepository = app.Services.GetRequiredService<IHoroscopeRepository>();

        _vm = new RadixOverviewViewModel(chartSession, navigationService, rosetta, horoscopeRepository);
        DataContext = _vm;
    }

    private void OnNewChartClicked(object sender, RoutedEventArgs e)
    {
        _vm?.NewChart();
    }

    private void OnSearchChartClicked(object sender, RoutedEventArgs e)
    {
        _vm?.SearchChart();
    }

    private void OnRowClicked(object sender, MouseButtonEventArgs e)
    {
        if (_vm is not null && sender is Grid { DataContext: SessionChartRow row })
            _vm.Select(row);
    }

    private async void OnEditMenuClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is not null && sender is MenuItem { Tag: SessionChartRow row })
            await _vm.EditAsync(row);
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var msg = _vm?.TooltipHelp ?? string.Empty;
        MessageBox.Show(Window.GetWindow(this), msg, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
