// TransitsConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>Configuration for transits.</summary>
public readonly record struct TransitsConfig(IReadOnlyList<Factors> Factors, double Orb)
{
    public static TransitsConfig Default => new(DefaultFactors, Orb: 1.0);

    public static IReadOnlyList<Factors> DefaultFactors =>
    [
        Domain.Factors.Sun, Domain.Factors.Moon, Domain.Factors.Mercury, Domain.Factors.Venus,
        Domain.Factors.Mars, Domain.Factors.Jupiter, Domain.Factors.Saturn,
        Domain.Factors.Uranus, Domain.Factors.Neptune, Domain.Factors.Pluto,
        Domain.Factors.Chiron, Domain.Factors.NorthNodeMean
    ];
}
