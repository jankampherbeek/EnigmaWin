// RadixSearchScreen.xaml.cs
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

namespace EnigmaWin.Sources.Features.Radix.RadixSearch.UI;

public partial class RadixSearchScreen : UserControl
{
    private RadixSearchViewModel? _vm;

    public RadixSearchScreen()
    {
        InitializeComponent();

        if (Application.Current is not App app) return;

        var repository        = app.Services.GetRequiredService<IHoroscopeRepository>();
        var navigationService = app.Services.GetRequiredService<INavigationService>();
        var chartSession      = app.Services.GetRequiredService<IChartSession>();
        var rosetta           = app.Services.GetRequiredService<IRosetta>();
        var configContext     = app.Services.GetService<IConfigContext>();

        _vm = new RadixSearchViewModel(repository, navigationService, chartSession, rosetta, configContext);
        DataContext = _vm;
    }

    private async void OnSearchClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is not null)
            await _vm.SearchAsync();
    }

    private void OnRowClicked(object sender, MouseButtonEventArgs e)
    {
        if (_vm is not null && sender is Grid { DataContext: HoroscopeSearchRow row })
            _vm.Select(row);
    }

    private async void OnDeleteMenuClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null || sender is not MenuItem { Tag: HoroscopeSearchRow row }) return;

        var msg = string.Format(_vm.LabelDeleteMessage.Replace("%s", "{0}"), row.Name);
        var result = MessageBox.Show(Window.GetWindow(this), msg, _vm.LabelDeleteTitle,
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
            await _vm.DeleteAsync(row);
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var msg = _vm?.TooltipHelp ?? string.Empty;
        MessageBox.Show(Window.GetWindow(this), msg, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
