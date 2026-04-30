// ResearchProjectInputHelpWindow.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchProjectInputHelpWindow : Window
{
    public string LabelTitle { get; }
    public string HelpText   { get; }
    public string LabelClose { get; }

    public ResearchProjectInputHelpWindow(IRosetta rosetta)
    {
        LabelTitle = rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.help.title");
        HelpText   = rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.help.text");
        LabelClose = rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectinputscreen.help.close");

        Title = LabelTitle;
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();
}
