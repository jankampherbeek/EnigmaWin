// ResearchProjectConfigRouteViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Features.Research.UI;

namespace EnigmaWin.ViewModels.Routes;

public sealed class ResearchProjectConfigRouteViewModel : ObservableObject
{
    /// <summary>Stash the draft here before the DataTemplate instantiates the screen.</summary>
    public static ResearchProjectDraft? PendingDraft { get; set; }

    public ResearchProjectConfigRouteViewModel(ResearchProjectDraft draft)
    {
        PendingDraft = draft;
    }
}
