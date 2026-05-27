// SecondaryHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.TransitSecDir.UI;

public partial class SecondaryHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public SecondaryHelpWindow(IRosetta rosetta, string helpKey)
    {
        var raw = rosetta.GetText(RbFile.Secondary, helpKey);
        HelpParagraphs = raw.Split(["\n\n"], System.StringSplitOptions.RemoveEmptyEntries);
        LabelClose     = rosetta.GetText(RbFile.Secondary, "view.secondary.help.close");
        Title          = rosetta.GetText(RbFile.Secondary, "view.secondary.help.title");
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
