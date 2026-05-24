// AllMidpointsView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints.UI;

public partial class AllMidpointsView : UserControl
{
    public AllMidpointsView()
    {
        InitializeComponent();
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var app = (App)Application.Current;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var factsheetWindow = new AllMidpointsFactsheetWindow(rosetta)
        {
            Owner = Window.GetWindow(this)
        };
        factsheetWindow.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var app = (App)Application.Current;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new AllMidpointsHelpWindow(rosetta)
        {
            Owner = Window.GetWindow(this)
        };
        helpWindow.ShowDialog();
    }
}
