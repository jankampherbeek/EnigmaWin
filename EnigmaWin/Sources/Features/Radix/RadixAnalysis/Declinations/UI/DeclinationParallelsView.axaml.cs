// DeclinationParallelsView.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public partial class DeclinationParallelsView : UserControl
{
    public DeclinationParallelsView()
    {
        InitializeComponent();
    }

    private void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        // Help window will be wired up in a future step.
    }
}
