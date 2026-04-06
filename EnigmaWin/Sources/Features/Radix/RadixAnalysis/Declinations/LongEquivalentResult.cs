// LongEquivalentResult.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

/// <summary>The longitude equivalent for a single chart factor.</summary>
/// <param name="Factor">The factor this result belongs to.</param>
/// <param name="LongitudeEquivalent">Computed longitude equivalent in degrees (0–360°).</param>
/// <param name="IsOutOfBounds"><c>true</c> when the factor's declination exceeds the obliquity (out of bounds).</param>
public record LongEquivalentResult(
    Factors Factor,
    double LongitudeEquivalent,
    bool IsOutOfBounds);
