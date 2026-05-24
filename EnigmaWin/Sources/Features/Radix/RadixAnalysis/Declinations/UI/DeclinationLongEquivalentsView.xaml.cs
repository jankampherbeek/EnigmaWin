// DeclinationLongEquivalentsView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public partial class DeclinationLongEquivalentsView : UserControl
{
    public DeclinationLongEquivalentsView()
    {
        InitializeComponent();
    }

    private void OnDialTypeClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DeclinationLongEquivalentsViewModel vm) return;
        var tag = (sender as Button)?.Tag?.ToString();
        var newType = tag switch
        {
            "360" => (DrawingTypes?)DrawingTypes.Dial360,
            "90"  => (DrawingTypes?)DrawingTypes.Dial90,
            "45"  => (DrawingTypes?)DrawingTypes.Dial45,
            _     => (DrawingTypes?)null
        };
        if (newType is not null)
            vm.SetDrawingType(newType.Value);
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new DeclinationsFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        // Export via WheelExportService can be wired here when needed.
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new DeclinationsHelpWindow(rosetta, "declinations.equiv.help") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
