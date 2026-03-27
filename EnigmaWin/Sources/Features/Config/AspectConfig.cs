// AspectConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>Settings for a single aspect.</summary>
public readonly record struct AspectSettings(
    Aspects Aspect,
    bool IsUsed,
    bool IsDrawn,
    int OrbPercentage,
    ColorConfig Color)
{
    public static ColorConfig DefaultColor(Aspects aspect) => aspect switch
    {
        Aspects.Conjunction                           => new ColorConfig(0.0, 0.0, 1.0),
        Aspects.Square or Aspects.Opposition          => new ColorConfig(1.0, 0.0, 0.0),
        Aspects.Trine or Aspects.Sextile              => new ColorConfig(0.0, 0.6, 0.0),
        Aspects.Inconjunct                            => new ColorConfig(0.5, 0.0, 0.5),
        _                                             => new ColorConfig(0.5, 0.5, 0.5)
    };
}

/// <summary>Configuration for which aspects are active and their orb percentages.</summary>
public readonly record struct AspectConfig(IReadOnlyList<AspectSettings> Settings)
{
    public static AspectConfig Default => new(DefaultSettings);

    public static IReadOnlyList<AspectSettings> DefaultSettings =>
        Enum.GetValues<Aspects>().Select(a => a switch
        {
            Aspects.Conjunction or Aspects.Opposition =>
                new AspectSettings(a, IsUsed: true,  IsDrawn: true,  OrbPercentage: 100, AspectSettings.DefaultColor(a)),
            Aspects.Trine or Aspects.Square =>
                new AspectSettings(a, IsUsed: true,  IsDrawn: true,  OrbPercentage: 80,  AspectSettings.DefaultColor(a)),
            Aspects.Sextile =>
                new AspectSettings(a, IsUsed: true,  IsDrawn: true,  OrbPercentage: 60,  AspectSettings.DefaultColor(a)),
            Aspects.Inconjunct =>
                new AspectSettings(a, IsUsed: true,  IsDrawn: true,  OrbPercentage: 30,  AspectSettings.DefaultColor(a)),
            _ =>
                new AspectSettings(a, IsUsed: false, IsDrawn: false, OrbPercentage: 15,  AspectSettings.DefaultColor(a))
        }).ToList();
}
