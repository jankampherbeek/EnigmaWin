// DeclinationMidpointsView.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public partial class DeclinationMidpointsView : UserControl
{
    private const double WideBreakpoint = 620.0;

    public DeclinationMidpointsView()
    {
        InitializeComponent();
        SizeChanged += (_, e) => UpdateLayout(e.NewSize.Width);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateLayout(e.NewSize.Width);
    }

    private void UpdateLayout(double width)
    {
        var wide = width >= WideBreakpoint;
        WideLayout.IsVisible   = wide;
        NarrowLayout.IsVisible = !wide;
    }

    private async void OnFactsheetClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var window = new DeclinationsFactsheetWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await window.ShowDialog(owner);
    }

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var window = new DeclinationsHelpWindow(rosetta, "declinations.midpoints.help");
        if (TopLevel.GetTopLevel(this) is Window owner)
            await window.ShowDialog(owner);
    }
}
