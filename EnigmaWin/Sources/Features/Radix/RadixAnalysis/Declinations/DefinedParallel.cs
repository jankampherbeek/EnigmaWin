// DefinedParallel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

public record DefinedParallel(
    Factors Factor1,
    Factors Factor2,
    bool IsContraParallel,
    double MaxOrb,
    double ActualOrb,
    double Factor1Declination,
    double Factor2Declination);
