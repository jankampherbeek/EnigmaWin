// SynastryAspectsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;

namespace EnigmaWin.Sources.Features.Synastry;

/// <summary>
/// Calculates aspects between the factors of two different charts (synastry).
/// Factor1 in each FoundAspect is chart A's factor; Factor2 is chart B's factor.
/// </summary>
public static class SynastryAspectsOrchestrator
{
    public static List<FoundAspect> Calculate(
        FullChart chartA,
        FullChart chartB,
        FactorConfig factorConfig,
        AspectConfig aspectConfig,
        OrbConfig orbConfig)
    {
        var positionsA = ActivePositions(chartA, factorConfig);
        var positionsB = ActivePositions(chartB, factorConfig);
        if (positionsA.Count == 0 || positionsB.Count == 0) return [];

        var usedAspects = aspectConfig.Settings.Where(s => s.IsUsed).ToList();
        if (usedAspects.Count == 0) return [];

        var factorOrbPct = factorConfig.Settings
            .ToDictionary(s => s.Factor, s => s.OrbPercentage);

        var found = new List<FoundAspect>();

        foreach (var (fA, longA) in positionsA)
        {
            foreach (var (fB, longB) in positionsB)
            {
                var distance = ShortestDistance(longA, longB);

                var orbFractionA = (factorOrbPct.TryGetValue(fA, out var pctA) ? pctA : 100) / 100.0;
                var orbFractionB = (factorOrbPct.TryGetValue(fB, out var pctB) ? pctB : 100) / 100.0;
                var maxFactorFraction = Math.Max(orbFractionA, orbFractionB);

                foreach (var aspectSetting in usedAspects)
                {
                    var maxOrb = maxFactorFraction
                                 * (aspectSetting.OrbPercentage / 100.0)
                                 * orbConfig.AspectBaseOrb;
                    var deviation = Math.Abs(distance - aspectSetting.Aspect.Angle());
                    if (deviation <= maxOrb)
                    {
                        found.Add(new FoundAspect(fA, fB, aspectSetting.Aspect, deviation, maxOrb));
                    }
                }
            }
        }

        return [.. found.OrderBy(a => a.Orb)];
    }

    private static List<(Factors Factor, double Longitude)> ActivePositions(
        FullChart chart,
        FactorConfig factorConfig)
    {
        var result = new List<(Factors, double)>();

        foreach (var setting in factorConfig.Settings)
        {
            if (!setting.IsUsed) continue;

            double longitude;

            if (setting.Factor.CalculationType() == CalculationTypes.Mundane)
            {
                longitude = MundaneLongitude(setting.Factor, chart.HousePositions);
            }
            else if (chart.Coordinates.TryGetValue(setting.Factor, out var pos)
                     && pos.Ecliptical.Length > 0)
            {
                longitude = pos.Ecliptical[0].MainPos;
            }
            else
            {
                continue;
            }

            if (longitude >= 0.0)
                result.Add((setting.Factor, longitude));
        }

        return result;
    }

    private static double MundaneLongitude(Factors factor, HousePositions hp) => factor switch
    {
        Factors.Ascendant => hp.Ascendant.Longitude,
        Factors.Mc        => hp.Midheaven.Longitude,
        Factors.EastPoint => hp.Eastpoint.Longitude,
        Factors.Vertex    => hp.Vertex.Longitude,
        _                 => -1.0
    };

    private static double ShortestDistance(double long1, double long2)
    {
        var diff = Math.Abs(long1 - long2) % 360.0;
        return diff > 180.0 ? 360.0 - diff : diff;
    }
}
