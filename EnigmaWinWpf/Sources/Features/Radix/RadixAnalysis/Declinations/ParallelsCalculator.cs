// ParallelsCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

public static class ParallelsCalculator
{
    public static List<DefinedParallel> Calculate(
        FullChart chart,
        FactorConfig factorConfig,
        OrbConfig orbConfig)
    {
        var maxOrb = orbConfig.ParallelOrb;
        var pairs  = new List<(Factors Factor, double Declination)>();

        foreach (var setting in factorConfig.Settings)
        {
            if (!setting.IsUsed) continue;
            var decl = GetDeclination(setting.Factor, chart);
            if (decl.HasValue)
                pairs.Add((setting.Factor, decl.Value));
        }

        var results = new List<DefinedParallel>();
        for (var i = 0; i < pairs.Count; i++)
        {
            var (factor1, decl1) = pairs[i];
            for (var j = i + 1; j < pairs.Count; j++)
            {
                var (factor2, decl2) = pairs[j];
                var actualOrb = Math.Abs(Math.Abs(decl1) - Math.Abs(decl2));
                if (actualOrb > maxOrb) continue;
                var isContra = decl1 >= 0.0 != decl2 >= 0.0;
                results.Add(new DefinedParallel(factor1, factor2, isContra, maxOrb, actualOrb, decl1, decl2));
            }
        }

        return results;
    }

    private static double? GetDeclination(Factors factor, FullChart chart)
    {
        FullCuspPosition? cusp = factor switch
        {
            Factors.Ascendant => chart.HousePositions.Ascendant,
            Factors.Mc        => chart.HousePositions.Midheaven,
            Factors.EastPoint => chart.HousePositions.Eastpoint,
            Factors.Vertex    => chart.HousePositions.Vertex,
            _                 => null
        };

        if (cusp is not null) return cusp.Declination;

        if (chart.Coordinates.TryGetValue(factor, out var pos)
            && pos.Equatorial.Length > 0)
            return pos.Equatorial[0].Deviation;

        return null;
    }
}
