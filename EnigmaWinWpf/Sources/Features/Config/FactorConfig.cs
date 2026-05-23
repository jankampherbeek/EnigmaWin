// FactorConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>Settings for a single celestial factor.</summary>
public readonly record struct FactorSettings(
    Factors Factor,
    bool IsUsed,
    bool IsDrawn,
    int OrbPercentage = 100);

/// <summary>Configuration for which celestial factors are active and their orb percentages.</summary>
public readonly record struct FactorConfig(IReadOnlyList<FactorSettings> Settings)
{
    public static FactorConfig Default => new(DefaultSettings);

    /// <summary>Default western tropical configuration: classic planets active, all others inactive.</summary>
    public static IReadOnlyList<FactorSettings> DefaultSettings =>
        Enum.GetValues<Factors>().Select(f => f switch
        {
            Factors.Sun or Factors.Moon or Factors.Ascendant or Factors.Mc =>
                new FactorSettings(f, IsUsed: true,  IsDrawn: true,  OrbPercentage: 100),
            Factors.Mercury or Factors.Venus or Factors.Mars or Factors.Jupiter =>
                new FactorSettings(f, IsUsed: true,  IsDrawn: true,  OrbPercentage: 85),
            Factors.Saturn or Factors.Uranus or Factors.Neptune or Factors.Pluto
                or Factors.NorthNodeMean or Factors.Chiron =>
                new FactorSettings(f, IsUsed: true,  IsDrawn: true,  OrbPercentage: 75),
            _ =>
                new FactorSettings(f, IsUsed: false, IsDrawn: false, OrbPercentage: 50)
        }).ToList();
}
