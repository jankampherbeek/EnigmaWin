// LotsHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Lots.UI;

public partial class LotsHelpWindow : Window
{
    public string HelpText { get; }
    public string LabelClose { get; }

    public LotsHelpWindow(IRosetta rosetta)
    {
        HelpText = rosetta.GetText(RbFile.RadixLots, "lots.help.input");
        LabelClose = rosetta.GetText(RbFile.RadixLots, "lots.help.close");
        Title = rosetta.GetText(RbFile.RadixLots, "lots.navtitle");
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
