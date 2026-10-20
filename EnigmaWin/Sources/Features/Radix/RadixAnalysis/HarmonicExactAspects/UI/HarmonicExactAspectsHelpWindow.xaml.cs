// HarmonicExactAspectsHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicExactAspects.UI;

public partial class HarmonicExactAspectsHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public HarmonicExactAspectsHelpWindow(IRosetta rosetta)
    {
        var raw = rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.help");
        HelpParagraphs = raw.Split(["\n"],StringSplitOptions.RemoveEmptyEntries);
        LabelClose = rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.help.close");
        Title = rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.title");
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
