// RadixPositionsHelpWindow.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixPositions.UI;

public partial class RadixPositionsHelpWindow : Window
{
    public string HelpText   { get; }
    public string LabelClose { get; }

    public RadixPositionsHelpWindow(IRosetta rosetta)
    {
        HelpText   = rosetta.GetText(RbFile.RadixPositions, "positions.help");
        LabelClose = rosetta.GetText(RbFile.RadixPositions, "positions.help.close");

        Title = rosetta.GetText(RbFile.RadixPositions, "positions.help.title");

        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();
}
