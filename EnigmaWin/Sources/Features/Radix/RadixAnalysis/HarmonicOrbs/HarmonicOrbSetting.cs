// HarmonicOrbSetting.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicOrbs;

/// <summary>A single aspect entry for the Harmonic Orbs feature: its harmonic divisor.</summary>
public sealed record HarmonicOrbSetting(Domain.Aspects Aspect, int HarmonicNumber)
{
    /// <summary>The 20 aspects supported by Harmonic Orbs, in display order, each with its harmonic divisor.</summary>
    public static readonly IReadOnlyList<HarmonicOrbSetting> Defaults =
    [
        new(Domain.Aspects.Conjunction, 1),
        new(Domain.Aspects.Opposition, 2),
        new(Domain.Aspects.Trine, 3),
        new(Domain.Aspects.Square, 4),
        new(Domain.Aspects.Quintile, 5),
        new(Domain.Aspects.Biquintile, 5),
        new(Domain.Aspects.Sextile, 6),
        new(Domain.Aspects.Septile, 7),
        new(Domain.Aspects.Biseptile, 7),
        new(Domain.Aspects.Triseptile, 7),
        new(Domain.Aspects.Semisquare, 8),
        new(Domain.Aspects.Sesquiquadrate, 8),
        new(Domain.Aspects.Novile, 9),
        new(Domain.Aspects.Binovile, 9),
        new(Domain.Aspects.Quadranovile, 9),
        new(Domain.Aspects.Semiquintile, 10),
        new(Domain.Aspects.Tridecile, 10),
        new(Domain.Aspects.Undecile, 11),
        new(Domain.Aspects.Semisextile, 12),
        new(Domain.Aspects.Inconjunct, 12),
    ];
}
