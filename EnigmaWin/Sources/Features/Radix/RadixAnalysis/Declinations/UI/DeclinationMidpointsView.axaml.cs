// DeclinationMidpointsView.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Controls;
using Avalonia.Interactivity;

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

    private void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        // Help popup not yet implemented
    }
}
