// DeclMidpointsCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

/// <summary>Computes all base midpoints in declination for the active factors in a chart.</summary>
/// <remarks>
/// Declination midpoints are arithmetic means: <c>midpoint = (decl1 + decl2) / 2</c>.
/// There is no circular wrap-around — declination is a linear scale from roughly −90° to +90°.
/// </remarks>
public static class DeclMidpointsCalculator
{
    /// <summary>
    /// Returns all pair midpoints in declination for the active factors present in the chart.
    /// </summary>
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

    /// <summary>
    /// Returns (factor, declination) for every active factor present in the chart.
    /// Uses <see cref="Factors.CalculationType"/> to route mundane factors to
    /// <see cref="HousePositions"/> and celestial factors to <see cref="FullChart.Coordinates"/>.
    /// Factors with <see cref="CalculationTypes.ZodiacFixed"/> (e.g. ZeroAries) are excluded.
    /// </summary>
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

    /// <summary>
    /// Returns the declination for a factor from the chart, or <c>null</c> if not present.
    /// Mundane factors (ASC, MC, EP, Vertex) are read from <see cref="HousePositions"/>;
    /// all other celestial factors from <see cref="FullChart.Coordinates"/>.
    /// Fixed zodiac points (ZeroAries) are excluded (returns <c>null</c>).
    /// </summary>
    public static double? GetDeclination(Factors factor, FullChart chart)
    {
        var calcType = factor.CalculationType();

        // Exclude fixed zodiac points — they have no meaningful declination for midpoints
        if (calcType == CalculationTypes.ZodiacFixed) return null;

        // Mundane factors: read from HousePositions
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

        // Celestial factors: read from Coordinates
        if (chart.Coordinates.TryGetValue(factor, out var pos)
            && pos.Equatorial.Length > 0)
            return pos.Equatorial[0].Deviation;

        return null;
    }
}
