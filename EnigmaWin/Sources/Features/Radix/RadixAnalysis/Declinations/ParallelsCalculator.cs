// ParallelsCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

/// <summary>Calculates parallels and contra-parallels from a chart.</summary>
/// <remarks>
/// A <b>parallel</b> occurs when two factors have (approximately) the same absolute
/// declination value and are on the <b>same</b> side of the celestial equator.
/// A <b>contra-parallel</b> occurs when they have the same absolute declination but
/// are on <b>opposite</b> sides.
/// The orb used is <see cref="OrbConfig.ParallelOrb"/>.
/// </remarks>
public static class ParallelsCalculator
{
    /// <summary>
    /// Find all parallels and contra-parallels for the active factors in a chart.
    /// </summary>
    /// <param name="chart">The calculated chart containing equatorial positions.</param>
    /// <param name="factorConfig">Determines which factors are active (<c>IsUsed == true</c>).</param>
    /// <param name="orbConfig">Supplies <see cref="OrbConfig.ParallelOrb"/>.</param>
    /// <returns>All <see cref="DefinedParallel"/> instances whose actual orb ≤ <c>ParallelOrb</c>.</returns>
    public static List<DefinedParallel> Calculate(
        FullChart chart,
        FactorConfig factorConfig,
        OrbConfig orbConfig)
    {
        var maxOrb = orbConfig.ParallelOrb;

        // Collect (factor, declination) pairs for every active factor that has a position.
        var pairs = new List<(Factors Factor, double Declination)>();
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

    /// <summary>Returns the declination for a factor from the chart, or <c>null</c> if not present.</summary>
    private static double? GetDeclination(Factors factor, FullChart chart)
    {
        // Mundane factors (Asc, MC, Eastpoint, Vertex) are stored in HousePositions with
        // accurate declinations. chart.Coordinates contains placeholder zero-values for them,
        // so check HousePositions first for these factors.
        FullCuspPosition? cusp = factor switch
        {
            Factors.Ascendant => chart.HousePositions.Ascendant,
            Factors.Mc        => chart.HousePositions.Midheaven,
            Factors.EastPoint => chart.HousePositions.Eastpoint,
            Factors.Vertex    => chart.HousePositions.Vertex,
            _                 => null
        };

        if (cusp is not null)
            return cusp.Declination;

        if (chart.Coordinates.TryGetValue(factor, out var pos)
            && pos.Equatorial.Length > 0)
            return pos.Equatorial[0].Deviation;

        return null;
    }
}
