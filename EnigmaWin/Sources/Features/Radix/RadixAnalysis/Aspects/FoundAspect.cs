// FoundAspect.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;

/// <summary>The result of a single found aspect between two celestial factors.</summary>
/// <param name="Factor1">First factor in the aspect pair (lower index in calculation order).</param>
/// <param name="Factor2">Second factor in the aspect pair.</param>
/// <param name="Aspect">The aspect type that was found.</param>
/// <param name="Orb">Actual angular deviation from the exact aspect angle (0 = exact).</param>
/// <param name="MaxOrb">Maximum allowed orb for this factor/aspect combination.</param>
public record FoundAspect(
    Factors Factor1,
    Factors Factor2,
    Domain.Aspects Aspect,
    double Orb,
    double MaxOrb);
