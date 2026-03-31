// OccupiedMidpointsView.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints.UI;

public partial class OccupiedMidpointsView : UserControl
{
    public OccupiedMidpointsView()
    {
        InitializeComponent();
    }

    private async void OnFactsheetClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var factsheetWindow = new OccupiedMidpointsFactsheetWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await factsheetWindow.ShowDialog(owner);
    }

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new OccupiedMidpointsHelpWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await helpWindow.ShowDialog(owner);
    }
}
