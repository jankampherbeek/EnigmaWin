// HarmonicsMatchesHelpWindow.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

public partial class HarmonicsMatchesHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public HarmonicsMatchesHelpWindow(IRosetta rosetta)
    {
        var raw = rosetta.GetText(RbFile.RadixHarmonics, "harmonics.matches.help");
        HelpParagraphs = raw.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries);
        LabelClose     = rosetta.GetText(RbFile.RadixHarmonics, "harmonics.help.close");

        Title = rosetta.GetText(RbFile.RadixHarmonics, "harmonics.help.title");

        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();
}
