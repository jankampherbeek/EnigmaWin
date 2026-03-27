// RadixAspectsHelpWindow.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects.UI;

public partial class RadixAspectsHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public RadixAspectsHelpWindow(IRosetta rosetta)
    {
        var raw = rosetta.GetText(RbFile.RadixAspects, "aspects.help");
        HelpParagraphs = raw.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries);
        LabelClose     = rosetta.GetText(RbFile.RadixAspects, "aspects.help.close");

        Title = rosetta.GetText(RbFile.RadixAspects, "aspects.help.title");

        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();
}
