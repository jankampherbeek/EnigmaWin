// DeclinationLongEquivalentsView.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public partial class DeclinationLongEquivalentsView : UserControl
{
    private const double WideThreshold = 660;

    public DeclinationLongEquivalentsView()
    {
        InitializeComponent();
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is not DeclinationLongEquivalentsViewModel vm || !vm.HasData) return;
        var wide = e.NewSize.Width >= WideThreshold;
        WideLayout.IsVisible   = wide;
        NarrowLayout.IsVisible = !wide;
    }

    private void OnDialTypeClicked(object? sender, RoutedEventArgs e)
    {
        var vm = DataContext as DeclinationLongEquivalentsViewModel;
        if (vm is null) return;

        var tag = (sender as Avalonia.Controls.Primitives.ToggleButton)?.Tag?.ToString()
               ?? (sender as Button)?.Tag?.ToString();

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

    private void OnExportClicked(object? sender, RoutedEventArgs e)
    {
        // Export will be wired up in a future step.
    }

    private void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        // Help window will be wired up in a future step.
    }
}
