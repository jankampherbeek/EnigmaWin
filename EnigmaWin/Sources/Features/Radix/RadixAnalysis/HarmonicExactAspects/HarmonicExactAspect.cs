// HarmonicExactAspect.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicExactAspects;

/// <summary>The aspect with the smallest orb within a single harmonic chart.</summary>
/// <param name="Harmonic">The harmonic number.</param>
/// <param name="Factor1">First factor in the aspect pair.</param>
/// <param name="Factor2">Second factor in the aspect pair.</param>
/// <param name="Aspect">The aspect between both harmonic positions.</param>
/// <param name="Orb">Actual angular deviation from the exact aspect angle, in the harmonic chart.</param>
public record HarmonicExactAspect(
    int Harmonic,
    Factors Factor1,
    Factors Factor2,
    Domain.Aspects Aspect,
    double Orb);
