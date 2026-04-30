// ResearchProjectsListHelpWindow.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchProjectsListHelpWindow : Window
{
    public string LabelTitle { get; }
    public string HelpText   { get; }
    public string LabelClose { get; }

    public ResearchProjectsListHelpWindow(IRosetta rosetta, ResearchProjectListMode mode)
    {
        var helpKey = mode == ResearchProjectListMode.All
            ? "view.researchprojectlistscreen.help.text.all"
            : "view.researchprojectlistscreen.help.text.search";

        LabelTitle = rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.help.title");
        HelpText   = rosetta.GetText(RbFile.ResearchProjects, helpKey);
        LabelClose = rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectlistscreen.help.close");

        Title = LabelTitle;
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();
}
