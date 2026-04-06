// DefinedParallel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

/// <summary>A found parallel or contra-parallel between two chart factors.</summary>
/// <param name="Factor1">First factor in the parallel pair (lower index in calculation order).</param>
/// <param name="Factor2">Second factor in the parallel pair.</param>
/// <param name="IsContraParallel">
/// <c>true</c> when the factors are on opposite sides of the equator (one north, one south);
/// <c>false</c> for a regular parallel (same side).
/// </param>
/// <param name="MaxOrb">The configured maximum orb that was used.</param>
/// <param name="ActualOrb">The actual angular distance between the absolute declination values.</param>
public record DefinedParallel(
    Factors Factor1,
    Factors Factor2,
    bool IsContraParallel,
    double MaxOrb,
    double ActualOrb);
