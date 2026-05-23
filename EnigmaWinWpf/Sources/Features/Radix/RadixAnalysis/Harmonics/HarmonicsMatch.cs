// HarmonicsMatch.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics;

/// <summary>A radix factor whose position coincides (within orb) with a harmonic position.</summary>
public record HarmonicsMatch(
    Factors RadixFactor,
    Factors HarmonicFactor,
    double ActualOrb,
    double MaxOrb);
