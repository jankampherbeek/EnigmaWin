// SynastryHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Synastry.UI;

public partial class SynastryHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public SynastryHelpWindow(IRosetta rosetta, string textKey)
    {
        var raw = rosetta.GetText(RbFile.Synastry, textKey);
        HelpParagraphs = raw.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries);
        LabelClose     = rosetta.GetText(RbFile.Synastry, "view.synastry.help.close");
        Title          = rosetta.GetText(RbFile.Synastry, "view.synastry.help.title");
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
