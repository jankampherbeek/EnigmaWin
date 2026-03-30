// BaseMidpoint.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;

/// <summary>A midpoint between two astrological factors, positioned on the ecliptic (0–360°).</summary>
/// <param name="Factor1">First factor of the midpoint pair.</param>
/// <param name="Factor2">Second factor of the midpoint pair.</param>
/// <param name="Position">Ecliptic longitude of the midpoint (0–360°).</param>
public record BaseMidpoint(
    Factors Factor1,
    Factors Factor2,
    double Position);
