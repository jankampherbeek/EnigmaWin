// TransitAspectsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;

namespace EnigmaWin.Sources.Features.Progressive;

/// <summary>
/// Calculates aspects between a set of transit/progressive positions and a radix chart.
/// Factor1 in each FoundAspect is the transit factor; Factor2 is the radix factor.
/// </summary>
public static class TransitAspectsOrchestrator
{
    public static List<FoundAspect> Calculate(
        Dictionary<Factors, ProgressivePosition> transitPositions,
        FullChart radixChart,
        FactorConfig factorConfig,
        AspectConfig aspectConfig,
        double baseOrb)
    {
        if (transitPositions.Count == 0) return [];

        var radixPairs = ActiveRadixPositions(radixChart, factorConfig);
        if (radixPairs.Count == 0) return [];

        var usedAspects = aspectConfig.Settings.Where(s => s.IsUsed).ToList();
        if (usedAspects.Count == 0) return [];

        var found = new List<FoundAspect>();

        foreach (var (tFactor, tPos) in transitPositions)
        {
            foreach (var (rFactor, rLong) in radixPairs)
            {
                var distance = ShortestDistance(tPos.Longitude, rLong);
                foreach (var aspectSetting in usedAspects)
                {
                    var deviation = Math.Abs(distance - aspectSetting.Aspect.Angle());
                    if (deviation <= baseOrb)
                    {
                        found.Add(new FoundAspect(tFactor, rFactor, aspectSetting.Aspect, deviation, baseOrb));
                    }
                }
            }
        }

        return [.. found.OrderBy(a => a.Orb)];
    }

    private static List<(Factors Factor, double Longitude)> ActiveRadixPositions(
        FullChart chart,
        FactorConfig factorConfig)
    {
        var usedFactors = new HashSet<Factors>(
            factorConfig.Settings.Where(s => s.IsUsed).Select(s => s.Factor));

        var result = new List<(Factors, double)>();
        foreach (var (factor, position) in chart.Coordinates)
        {
            if (usedFactors.Contains(factor) && position.Ecliptical.Length > 0)
                result.Add((factor, position.Ecliptical[0].MainPos));
        }
        return result;
    }

    private static double ShortestDistance(double long1, double long2)
    {
        var diff = Math.Abs(long1 - long2) % 360.0;
        return diff > 180.0 ? 360.0 - diff : diff;
    }
}
