// ResearchProjectOpenRouteViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;

namespace EnigmaWin.ViewModels.Routes;

public sealed class ResearchProjectOpenRouteViewModel : ObservableObject
{
    public static ResearchProjectOpenRouteViewModel? Pending { get; private set; }

    public Sources.Domain.ResearchProject? Project { get; }

    public ResearchProjectOpenRouteViewModel(int projectId, IResearchProjectRepository repository)
    {
        Project = repository.FetchById(projectId);
        Pending = this;
    }
}
