// LongEquivalentResult.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

public record LongEquivalentResult(
    Factors Factor,
    double LongitudeEquivalent,
    bool IsOutOfBounds);
