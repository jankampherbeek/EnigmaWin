// CountingsHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Countings.UI;

public partial class CountingsHelpWindow : Window
{
    public string HelpText { get; }
    public string LabelClose { get; }

    public CountingsHelpWindow(IRosetta rosetta)
    {
        HelpText = rosetta.GetText(RbFile.RadixCountings, "countings.help");
        LabelClose = rosetta.GetText(RbFile.RadixCountings, "countings.help.close");
        Title = rosetta.GetText(RbFile.RadixCountings, "countings.title");
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
