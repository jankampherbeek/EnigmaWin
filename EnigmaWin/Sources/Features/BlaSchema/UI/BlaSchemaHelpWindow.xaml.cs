// BlaSchemaHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.BlaSchema.UI;

/// <summary>Shows the help text for whichever BLA schema section is active when the help button
/// is clicked (each of the 6 sections has its own text, unlike the single-topic help windows
/// used elsewhere in the app).</summary>
public partial class BlaSchemaHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public BlaSchemaHelpWindow(IRosetta rosetta, string sectionTitle, string helpText)
    {
        HelpParagraphs = helpText.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries);
        LabelClose = rosetta.GetText(RbFile.RadixBlaSchema, "blaschema.help.close");
        Title = sectionTitle;
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
