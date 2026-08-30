// HarmonicOrbsHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicOrbs.UI;

/// <summary>HarmonicOrbs has a single help text (used by both the Input and Results screens),
/// unlike BLA schema's per-section help — still takes a text key for consistency with sibling
/// help windows in this codebase.</summary>
public partial class HarmonicOrbsHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public HarmonicOrbsHelpWindow(IRosetta rosetta, string textKey = "harmonicorbs.help.input")
    {
        var raw = rosetta.GetText(RbFile.RadixHarmonicOrbs, textKey);
        HelpParagraphs = raw.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries);
        LabelClose = rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.help.close");
        Title = rosetta.GetText(RbFile.RadixAnalysis, "analysis.btn.harmonicorbs");
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
