// FixStarsScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars.UI;

public partial class FixStarsScreen : UserControl
{
    public FixStarsScreen()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is FixStarsViewModel vm)
                vm.Calculate();
        };
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new FixStarsFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new FixStarsHelpWindow(rosetta, "view.fixstar.help.results") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
