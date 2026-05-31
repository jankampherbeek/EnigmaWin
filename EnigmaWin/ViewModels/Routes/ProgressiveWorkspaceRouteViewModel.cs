// ProgressiveWorkspaceRouteViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.ViewModels.Routes;

public sealed class ProgressiveWorkspaceRouteViewModel
{
    public string Title       { get; }
    public string Description { get; }

    public ProgressiveWorkspaceRouteViewModel(IRosetta rosetta)
    {
        Title       = rosetta.GetText(RbFile.Localizable, "progressive.workspace.title");
        Description = rosetta.GetText(RbFile.Localizable, "progressive.workspace.description");
    }
}
