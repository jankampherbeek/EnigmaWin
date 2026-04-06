// DeclOccupiedMidpoint.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

/// <summary>A factor whose declination coincides with a declination midpoint within the allowed orb.</summary>
/// <param name="Midpoint">The base midpoint that is occupied.</param>
/// <param name="OccupyingFactor">The factor that occupies the midpoint position.</param>
/// <param name="OccupyingPosition">Declination of the occupying factor in degrees.</param>
/// <param name="ActualOrb">Actual deviation from the exact midpoint position (always ≥ 0).</param>
/// <param name="Exactness">Exactness as a percentage: 100 = exact, 0 = at the orb boundary.</param>
public record DeclOccupiedMidpoint(
    DeclBaseMidpoint Midpoint,
    Factors OccupyingFactor,
    double OccupyingPosition,
    double ActualOrb,
    double Exactness);
