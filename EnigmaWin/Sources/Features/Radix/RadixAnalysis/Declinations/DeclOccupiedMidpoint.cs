// DeclOccupiedMidpoint.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

public record DeclOccupiedMidpoint(
    DeclBaseMidpoint Midpoint,
    Factors OccupyingFactor,
    double OccupyingPosition,
    double ActualOrb,
    double Exactness);
