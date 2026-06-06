// ZodiacDivisionsInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions.UI;

public sealed class ZodiacDivisionsInputViewModel(ZodiacDivisionsViewModel inner)
{
    public ZodiacDivisionsViewModel Inner { get; } = inner;
}
