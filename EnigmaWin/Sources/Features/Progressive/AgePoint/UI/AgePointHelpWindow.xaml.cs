// AgePointHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.AgePoint.UI;

public partial class AgePointHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public AgePointHelpWindow(IRosetta rosetta, string helpKey)
    {
        var raw    = rosetta.GetText(RbFile.AgePoint, helpKey);
        HelpParagraphs = raw.Split(["\n\n"], System.StringSplitOptions.RemoveEmptyEntries);
        LabelClose     = rosetta.GetText(RbFile.AgePoint, "view.agepoint.help.close");
        Title          = rosetta.GetText(RbFile.AgePoint, "view.agepoint.help.title");
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
