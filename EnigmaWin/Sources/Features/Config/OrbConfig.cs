// OrbConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Config;

/// <summary>The method used to calculate effective orbs.</summary>
public enum OrbSystems
{
    Procentual    = 0,
    Fixed         = 1,
    HarmonicBased = 2
}

/// <summary>Extension methods for <see cref="OrbSystems"/>.</summary>
public static class OrbSystemsExtensions
{
    public static string LocalizedName(this OrbSystems system) => system switch
    {
        OrbSystems.Procentual    => "enum.orbsystem.procentual",
        OrbSystems.Fixed         => "enum.orbsystem.fixed",
        OrbSystems.HarmonicBased => "enum.orbsystem.harmonicbased",
        _                        => string.Empty
    };
}

/// <summary>Configuration for global orb values used in different calculation types.</summary>
/// <remarks>
/// The effective orb for an aspect is derived by combining the base orb with
/// the orb percentages from FactorConfig and AspectConfig.
/// Each midpoint dial has its own fixed orb value.
/// </remarks>
public readonly record struct OrbConfig(
    OrbSystems OrbSystem,
    double AspectBaseOrb,
    double Midpoint360DialOrb,
    double Midpoint90DialOrb,
    double Midpoint45DialOrb,
    double HarmonicOrb,
    double ParallelOrb,
    double DeclinationMidpointOrb)
{
    public static OrbConfig Default => new(
        OrbSystems.Procentual,
        AspectBaseOrb:           10.0,
        Midpoint360DialOrb:       1.5,
        Midpoint90DialOrb:        1.0,
        Midpoint45DialOrb:        0.5,
        HarmonicOrb:              2.0,
        ParallelOrb:              1.0,
        DeclinationMidpointOrb:   0.75);
}
