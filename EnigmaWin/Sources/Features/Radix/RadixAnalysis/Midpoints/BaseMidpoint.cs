// BaseMidpoint.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;

/// <summary>A midpoint between two astrological factors, positioned on the ecliptic (0–360°).</summary>
public record BaseMidpoint(
    Factors Factor1,
    Factors Factor2,
    double Position);
