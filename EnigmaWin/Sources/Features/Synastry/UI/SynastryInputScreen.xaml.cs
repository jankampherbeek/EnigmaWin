// SynastryInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixSearch.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Synastry.UI;

public partial class SynastryInputScreen : UserControl
{
    public SynastryInputScreen() => InitializeComponent();

    private SynastryInputViewModel? Vm => DataContext as SynastryInputViewModel;

    private void OnSearchRowClicked(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: HoroscopeSearchRow row })
            Vm?.AddFromSearch(row);
    }

    private void OnRemoveClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: NamedChart chart })
            Vm?.RemoveChart(chart);
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new SynastryHelpWindow(rosetta, "view.synastry.help.input") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
