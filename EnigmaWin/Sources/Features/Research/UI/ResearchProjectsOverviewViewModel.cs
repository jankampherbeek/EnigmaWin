// ResearchProjectsOverviewViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Research.UI;

public sealed partial class ResearchProjectsOverviewViewModel : ObservableObject
{
    private readonly IRosetta          _rosetta;
    private readonly INavigationService _navigationService;

    public string LabelTitle    => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectsscreen.title");
    public string LabelSubtitle => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectsscreen.subtitle");
    public string LabelNew      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectsscreen.btn.new");
    public string LabelSearch   => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectsscreen.btn.search");
    public string LabelAll      => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectsscreen.btn.all");
    public string TooltipHelp   => _rosetta.GetText(RbFile.ResearchProjects, "view.researchprojectsscreen.help.tooltip");

    public ResearchProjectsOverviewViewModel(IRosetta rosetta, INavigationService navigationService)
    {
        _rosetta           = rosetta;
        _navigationService = navigationService;
    }

    internal void NewProject()      => _navigationService.NavigateMain(AppRoutes.ResearchProjectInput);
    internal void SearchProject()   => _navigationService.NavigateMain(AppRoutes.ResearchProjectListSearch);
    internal void ShowAllProjects() => _navigationService.NavigateMain(AppRoutes.ResearchProjectListAll);
}
