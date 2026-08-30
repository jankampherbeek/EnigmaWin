// CountingsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Countings;

/// <summary>Element/cross counts (by zodiac sign) for the factors currently active in the configuration.
/// Unlike the BLA schema (fixed point set), this always follows the app's regular FactorConfig.</summary>
public static class CountingsOrchestrator
{
    public static (List<CountingsLine> Elements, List<CountingsLine> Crosses) ElementsAndCrosses(
        FullChart chart, FactorConfig factorConfig)
    {
        var usedFactors = factorConfig.Settings.Where(s => s.IsUsed).Select(s => s.Factor).ToHashSet();

        var signCounts = new Dictionary<int, int>();
        for (var s = 1; s <= 12; s++) signCounts[s] = 0;

        foreach (var (factor, position) in chart.Coordinates)
        {
            if (!usedFactors.Contains(factor) || position.Ecliptical.Length == 0) continue;
            var sign = SignForLongitude(position.Ecliptical[0].MainPos);
            signCounts[sign]++;
        }

        var elements = GroupedLines([CountingsGroup.Fire, CountingsGroup.Earth, CountingsGroup.Air, CountingsGroup.Water],
            signCounts, CountingsDomain.ElementFor);
        var crosses = GroupedLines([CountingsGroup.Cardinal, CountingsGroup.Fixed, CountingsGroup.Mutable],
            signCounts, CountingsDomain.CrossFor);
        return (elements, crosses);
    }

    private static int SignForLongitude(double longitude)
    {
        var lon = longitude % 360.0;
        if (lon < 0) lon += 360.0;
        return (int)(lon / 30.0) + 1;
    }

    private static List<CountingsLine> GroupedLines(
        IReadOnlyList<CountingsGroup> kinds, Dictionary<int, int> signCounts, Func<int, CountingsGroup?> groupFor)
    {
        var counts = new Dictionary<CountingsGroup, int>();
        foreach (var (sign, count) in signCounts)
        {
            var group = groupFor(sign);
            if (group is null) continue;
            counts[group.Value] = counts.GetValueOrDefault(group.Value) + count;
        }
        return kinds.Select(k => new CountingsLine(k, counts.GetValueOrDefault(k))).ToList();
    }
}
