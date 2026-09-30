// AltZodiacStartHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.AltZodiacStart.UI;

/// <summary>Help window for the AltZodiacStart screens; <c>helpKey</c> selects the input or the results help text.</summary>
public partial class AltZodiacStartHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public AltZodiacStartHelpWindow(IRosetta rosetta, string helpKey)
    {
        var raw = rosetta.GetText(RbFile.RadixAltZodiacStart, helpKey);
        HelpParagraphs = raw.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries);
        LabelClose     = rosetta.GetText(RbFile.RadixAltZodiacStart, "altzodiacstart.help.close");
        Title          = rosetta.GetText(RbFile.RadixAltZodiacStart, "altzodiacstart.help.title");
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
