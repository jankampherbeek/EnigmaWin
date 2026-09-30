// AltZodiacStartInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.AltZodiacStart.UI;

/// <summary>Route ViewModel for the input screen; wraps the shared <see cref="AltZodiacStartViewModel"/>.</summary>
public sealed class AltZodiacStartInputViewModel(AltZodiacStartViewModel inner)
{
    public AltZodiacStartViewModel Inner { get; } = inner;
}
