// AspectsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;

/// <summary>Calculates all aspects present in a chart.</summary>
public static class AspectsOrchestrator
{
    /// <summary>
    /// Returns all aspects found in the chart, sorted by orb (most exact first).
    /// </summary>
    /// <param name="chart">The full chart with all calculated positions.</param>
    /// <param name="factorConfig">Configuration for which factors are active and their orb percentages.</param>
    /// <param name="aspectConfig">Configuration for which aspects are active and their orb percentages.</param>
    /// <param name="orbConfig">Global orb configuration containing the base orb.</param>
    /// <returns>List of found aspects, sorted ascending by orb (most exact first).</returns>
    public static List<FoundAspect> Calculate(
        FullChart chart,
        FactorConfig factorConfig,
        AspectConfig aspectConfig,
        OrbConfig orbConfig)
    {
        var positions = ActivePositions(chart, factorConfig);
        if (positions.Count < 2) return [];

        var usedAspects = aspectConfig.Settings.Where(s => s.IsUsed).ToList();
        if (usedAspects.Count == 0) return [];

        var factorOrbPct = factorConfig.Settings
            .ToDictionary(s => s.Factor, s => s.OrbPercentage);

        var found = new List<FoundAspect>();

        for (var i = 0; i < positions.Count; i++)
        {
            for (var j = i + 1; j < positions.Count; j++)
            {
                var (f1, long1) = positions[i];
                var (f2, long2) = positions[j];
                var distance = ShortestDistance(long1, long2);

                var orbFraction1 = (factorOrbPct.TryGetValue(f1, out var pct1) ? pct1 : 100) / 100.0;
                var orbFraction2 = (factorOrbPct.TryGetValue(f2, out var pct2) ? pct2 : 100) / 100.0;
                var maxFactorFraction = Math.Max(orbFraction1, orbFraction2);

                foreach (var aspectSetting in usedAspects)
                {
                    var maxOrb = maxFactorFraction
                                 * (aspectSetting.OrbPercentage / 100.0)
                                 * orbConfig.AspectBaseOrb;
                    var deviation = Math.Abs(distance - aspectSetting.Aspect.Angle());
                    if (deviation <= maxOrb)
                    {
                        found.Add(new FoundAspect(f1, f2, aspectSetting.Aspect, deviation, maxOrb));
                    }
                }
            }
        }

        return [.. found.OrderBy(a => a.Orb)];
    }

    /// <summary>
    /// Returns (factor, ecliptic longitude) for all used factors, in enum order.
    /// Iterates over the config (single source of truth) rather than the chart dictionary
    /// to guarantee correct order and avoid duplicates or omissions.
    /// </summary>
    private static List<(Factors Factor, double Longitude)> ActivePositions(
        FullChart chart,
        FactorConfig factorConfig)
    {
        var result = new List<(Factors, double)>();

        foreach (var setting in factorConfig.Settings)
        {
            if (!setting.IsUsed) continue;

            double longitude;

            // Mundane factors (Ascendant, MC, EastPoint, Vertex): real positions are in
            // HousePositions; Coordinates only has a placeholder longitude of 0.0.
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

    /// <summary>Returns the ecliptic longitude for a mundane factor from HousePositions.</summary>
    private static double MundaneLongitude(Factors factor, HousePositions hp) => factor switch
    {
        Factors.Ascendant => hp.Ascendant.Longitude,
        Factors.Mc        => hp.Midheaven.Longitude,
        Factors.EastPoint => hp.Eastpoint.Longitude,
        Factors.Vertex    => hp.Vertex.Longitude,
        _                 => -1.0
    };

    /// <summary>Shortest angular distance between two ecliptic longitudes, in the range 0–180°.</summary>
    private static double ShortestDistance(double long1, double long2)
    {
        var diff = Math.Abs(long1 - long2) % 360.0;
        return diff > 180.0 ? 360.0 - diff : diff;
    }
}
