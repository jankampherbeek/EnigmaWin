// DeclBaseMidpoint.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

/// <summary>A midpoint between two factors expressed in declination.</summary>
/// <param name="Factor1">First factor of the pair.</param>
/// <param name="Factor2">Second factor of the pair.</param>
/// <param name="Position">
/// Declination of the midpoint in degrees (negative = south, positive = north).
/// Computed as the arithmetic mean: <c>(decl1 + decl2) / 2</c>.
/// </param>
public record DeclBaseMidpoint(
    Factors Factor1,
    Factors Factor2,
    double Position);
