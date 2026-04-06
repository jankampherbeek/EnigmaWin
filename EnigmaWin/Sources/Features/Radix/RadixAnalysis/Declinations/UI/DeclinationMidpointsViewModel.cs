// DeclinationMidpointsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public sealed class DeclinationMidpointsViewModel
{
    private readonly IRosetta _rosetta;

    public DeclinationMidpointsViewModel(IRosetta rosetta)
    {
        _rosetta = rosetta;
    }

    public string LabelTitle => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.title");
}
