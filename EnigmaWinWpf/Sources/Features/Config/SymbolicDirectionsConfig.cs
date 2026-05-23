// SymbolicDirectionsConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Config;

public enum SymbolicTimeKeys
{
    OneDegree = 0,
    TrueSun   = 1,
    MeanSun   = 2
}

public static class SymbolicTimeKeysExtensions
{
    public static string LocalizedName(this SymbolicTimeKeys k) => k switch
    {
        SymbolicTimeKeys.OneDegree => "enum.symbolictimekey.onedegree",
        SymbolicTimeKeys.TrueSun   => "enum.symbolictimekey.truesun",
        SymbolicTimeKeys.MeanSun   => "enum.symbolictimekey.meansun",
        _                          => string.Empty
    };
}

/// <summary>Configuration for symbolic directions.</summary>
public readonly record struct SymbolicDirectionsConfig(
    IReadOnlyList<Factors> Factors,
    double Orb,
    SymbolicTimeKeys TimeKey)
{
    public static SymbolicDirectionsConfig Default => new(DefaultFactors, Orb: 1.0, SymbolicTimeKeys.OneDegree);

    public static IReadOnlyList<Factors> DefaultFactors =>
    [
        Domain.Factors.Sun, Domain.Factors.Moon, Domain.Factors.Mercury, Domain.Factors.Venus,
        Domain.Factors.Mars, Domain.Factors.Jupiter, Domain.Factors.Saturn,
        Domain.Factors.Uranus, Domain.Factors.Neptune, Domain.Factors.Pluto,
        Domain.Factors.Chiron, Domain.Factors.NorthNodeMean, Domain.Factors.Mc, Domain.Factors.Ascendant
    ];
}
