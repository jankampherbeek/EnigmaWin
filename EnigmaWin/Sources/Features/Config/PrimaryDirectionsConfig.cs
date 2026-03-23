// PrimaryDirectionsConfig.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Config;

public enum PrimaryMethods
{
    Placidus     = 0,
    Regiomontanus = 1
}

public enum PrimaryApproaches
{
    Mundane    = 0,
    Ecliptical = 1
}

public enum PrimaryTimeKeys
{
    Placidus  = 0,
    Naibod    = 1,
    Brahe     = 2,
    Ptolemy   = 3,
    VanDam    = 4
}

public static class PrimaryMethodsExtensions
{
    public static string LocalizedName(this PrimaryMethods m) => m switch
    {
        PrimaryMethods.Placidus      => "enum.primarymethod.placidus",
        PrimaryMethods.Regiomontanus => "enum.primarymethod.regiomontanus",
        _                            => string.Empty
    };
}

public static class PrimaryApproachesExtensions
{
    public static string LocalizedName(this PrimaryApproaches a) => a switch
    {
        PrimaryApproaches.Mundane    => "enum.primaryapproach.mundane",
        PrimaryApproaches.Ecliptical => "enum.primaryapproach.ecliptical",
        _                            => string.Empty
    };
}

public static class PrimaryTimeKeysExtensions
{
    public static string LocalizedName(this PrimaryTimeKeys k) => k switch
    {
        PrimaryTimeKeys.Placidus => "enum.primarytimekey.placidus",
        PrimaryTimeKeys.Naibod   => "enum.primarytimekey.naibod",
        PrimaryTimeKeys.Brahe    => "enum.primarytimekey.brahe",
        PrimaryTimeKeys.Ptolemy  => "enum.primarytimekey.ptolemy",
        PrimaryTimeKeys.VanDam   => "enum.primarytimekey.vandam",
        _                        => string.Empty
    };
}

/// <summary>Configuration for primary directions.</summary>
public readonly record struct PrimaryDirectionsConfig(
    IReadOnlyList<Factors> Promissors,
    IReadOnlyList<Factors> Significators,
    double Orb,
    PrimaryMethods Method,
    PrimaryApproaches Approach,
    PrimaryTimeKeys TimeKey)
{
    public static PrimaryDirectionsConfig Default => new(
        DefaultPromissors,
        DefaultSignificators,
        Orb: 1.0,
        PrimaryMethods.Placidus,
        PrimaryApproaches.Mundane,
        PrimaryTimeKeys.Naibod);

    public static IReadOnlyList<Factors> DefaultPromissors =>
    [
        Factors.Sun, Factors.Moon, Factors.Mercury, Factors.Venus,
        Factors.Mars, Factors.Jupiter, Factors.Saturn
    ];

    public static IReadOnlyList<Factors> DefaultSignificators =>
    [
        Factors.Sun, Factors.Moon, Factors.Mercury, Factors.Venus,
        Factors.Mars, Factors.Jupiter, Factors.Saturn, Factors.Mc, Factors.Ascendant
    ];
}
