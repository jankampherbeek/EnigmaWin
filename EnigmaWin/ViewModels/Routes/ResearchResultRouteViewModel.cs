// ResearchResultRouteViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Analysis;

namespace EnigmaWin.ViewModels.Routes;

public sealed class ResearchResultRouteViewModel : ObservableObject
{
    public static ResearchResultRouteViewModel? Pending { get; private set; }

    public AnalysisResult? Result        { get; }
    public ResearchProject? Project      { get; }
    public int              CgMultiplier { get; }

    public ResearchResultRouteViewModel(
        AnalysisResult result,
        ResearchProject project,
        int cgMultiplier)
    {
        Result        = result;
        Project       = project;
        CgMultiplier  = cgMultiplier;
        Pending       = this;
    }
}
