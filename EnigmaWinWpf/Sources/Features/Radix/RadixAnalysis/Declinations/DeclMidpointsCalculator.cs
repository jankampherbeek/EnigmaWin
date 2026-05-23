// DeclMidpointsCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

public static class DeclMidpointsCalculator
{
    public static List<DeclBaseMidpoint> BaseMidpoints(
        FullChart chart,
        FactorConfig factorConfig)
    {
        var pairs = ActivePairs(chart, factorConfig);
        if (pairs.Count < 2) return [];

        var result = new List<DeclBaseMidpoint>();
        for (var i = 0; i < pairs.Count; i++)
        {
            var (f1, d1) = pairs[i];
            for (var j = i + 1; j < pairs.Count; j++)
            {
                var (f2, d2) = pairs[j];
                result.Add(new DeclBaseMidpoint(f1, f2, (d1 + d2) / 2.0));
            }
        }
        return result;
    }

    public static List<(Factors Factor, double Declination)> ActivePairs(
        FullChart chart,
        FactorConfig factorConfig)
    {
        var result = new List<(Factors, double)>();
        foreach (var setting in factorConfig.Settings)
        {
            if (!setting.IsUsed) continue;
            var decl = GetDeclination(setting.Factor, chart);
            if (decl.HasValue)
                result.Add((setting.Factor, decl.Value));
        }
        return result;
    }

    public static double? GetDeclination(Factors factor, FullChart chart)
    {
        var calcType = factor.CalculationType();
        if (calcType == CalculationTypes.ZodiacFixed) return null;

        if (calcType == CalculationTypes.Mundane)
        {
            FullCuspPosition? cusp = factor switch
            {
                Factors.Ascendant => chart.HousePositions.Ascendant,
                Factors.Mc        => chart.HousePositions.Midheaven,
                Factors.EastPoint => chart.HousePositions.Eastpoint,
                Factors.Vertex    => chart.HousePositions.Vertex,
                _                 => null
            };
            return cusp?.Declination;
        }

        if (chart.Coordinates.TryGetValue(factor, out var pos)
            && pos.Equatorial.Length > 0)
            return pos.Equatorial[0].Deviation;

        return null;
    }
}
