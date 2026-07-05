// FixStarsHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars.UI;

public partial class FixStarsHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public FixStarsHelpWindow(IRosetta rosetta, string textKey)
    {
        var raw = rosetta.GetText(RbFile.RadixFixStars, textKey);
        HelpParagraphs = raw.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries);
        LabelClose     = rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.help.close");
        Title          = rosetta.GetText(RbFile.RadixFixStars, "view.fixstar.help.title");
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
