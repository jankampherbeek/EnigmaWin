// ResearchProjectListRouteViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;

namespace EnigmaWin.ViewModels.Routes;

public static class ResearchProjectListPending
{
    public static ResearchProjectListMode Mode { get; set; } = ResearchProjectListMode.All;
}

public sealed class ResearchProjectAllListRouteViewModel : ObservableObject
{
    public ResearchProjectAllListRouteViewModel()
        => ResearchProjectListPending.Mode = ResearchProjectListMode.All;
}

public sealed class ResearchProjectSearchListRouteViewModel : ObservableObject
{
    public ResearchProjectSearchListRouteViewModel()
        => ResearchProjectListPending.Mode = ResearchProjectListMode.Search;
}
