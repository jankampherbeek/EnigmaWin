// HarmonicsMatch.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics;

/// <summary>A radix factor whose position coincides (within orb) with a harmonic position.</summary>
/// <param name="RadixFactor">The factor whose radix longitude is being matched against.</param>
/// <param name="HarmonicFactor">The factor whose harmonic position falls near the radix factor.</param>
/// <param name="ActualOrb">Actual angular separation between the harmonic and the radix position (0 = exact).</param>
/// <param name="MaxOrb">Maximum allowed orb for this match.</param>
public record HarmonicsMatch(
    Factors RadixFactor,
    Factors HarmonicFactor,
    double ActualOrb,
    double MaxOrb);
