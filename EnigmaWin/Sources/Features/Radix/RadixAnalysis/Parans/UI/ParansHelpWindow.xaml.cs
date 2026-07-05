// ParansHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Parans.UI;

public partial class ParansHelpWindow : Window
{
    public string[] HelpParagraphs { get; }
    public string LabelClose       { get; }

    public ParansHelpWindow(IRosetta rosetta, string textKey)
    {
        var raw = rosetta.GetText(RbFile.RadixParans, textKey);
        HelpParagraphs = raw.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries);
        LabelClose = rosetta.GetText(RbFile.RadixParans, "view.parans.help.close");
        Title      = rosetta.GetText(RbFile.RadixParans, "view.parans.help.title");
        InitializeComponent();
        DataContext = this;
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
