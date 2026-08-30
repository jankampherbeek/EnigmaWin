// HarmonicOrbsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicOrbs;

/// <summary>Calculates all aspects present in a chart using a flat per-aspect orb (the maximum orb
/// divided by the aspect's harmonic number), considering only the selected aspect settings. Unlike
/// <see cref="Aspects.AspectsOrchestrator"/>, mundane points (Ascendant/MC) do not participate —
/// matches the Apple app's narrower behaviour for this feature.</summary>
public static class HarmonicOrbsOrchestrator
{
    public static List<FoundAspect> Calculate(
        FullChart chart,
        FactorConfig factorConfig,
        IReadOnlyList<(Domain.Aspects Aspect, int HarmonicNumber, bool IsSelected)> settings,
        double maxOrbDegrees)
    {
        var positions = ActivePositions(chart, factorConfig);
        if (positions.Count < 2) return [];

        var selected = settings.Where(s => s.IsSelected).ToList();
        if (selected.Count == 0) return [];

        var found = new List<FoundAspect>();

        for (var i = 0; i < positions.Count; i++)
        {
            for (var j = i + 1; j < positions.Count; j++)
            {
                var (f1, long1) = positions[i];
                var (f2, long2) = positions[j];
                var distance = ShortestDistance(long1, long2);

                foreach (var setting in selected)
                {
                    var maxOrb = maxOrbDegrees / setting.HarmonicNumber;
                    var deviation = Math.Abs(distance - setting.Aspect.Angle());
                    if (deviation <= maxOrb)
                    {
                        found.Add(new FoundAspect(f1, f2, setting.Aspect, deviation, maxOrb));
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
        var usedFactors = factorConfig.Settings.Where(s => s.IsUsed).Select(s => s.Factor).ToHashSet();
        var result = new List<(Factors, double)>();

        foreach (var (factor, position) in chart.Coordinates)
        {
            if (!usedFactors.Contains(factor)) continue;
            if (position.Ecliptical.Length == 0) continue;
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
