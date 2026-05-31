// CalculatorsWorkspaceRouteViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.ViewModels.Routes;

public sealed class CalculatorsWorkspaceRouteViewModel
{
    public string Title       { get; }
    public string Description { get; }

    public CalculatorsWorkspaceRouteViewModel(IRosetta rosetta)
    {
        Title       = rosetta.GetText(RbFile.Calculators, "view.calculators.title");
        Description = rosetta.GetText(RbFile.Calculators, "view.calculators.select");
    }
}
