// OrbConfig.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

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
/// </remarks>
public readonly record struct OrbConfig(
    OrbSystems OrbSystem,
    double AspectBaseOrb,
    double MidpointOrb,
    double HarmonicOrb,
    double ParallelOrb)
{
    public static OrbConfig Default => new(
        OrbSystems.Procentual,
        AspectBaseOrb: 10.0,
        MidpointOrb:   1.6,
        HarmonicOrb:   2.0,
        ParallelOrb:   1.0);
}
