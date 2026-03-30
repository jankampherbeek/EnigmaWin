// MidpointsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;

/// <summary>
/// Orchestrates midpoint calculations for a chart.
/// Intended to be called from the UI layer with sub-configs from the application configuration.
/// </summary>
public static class MidpointsOrchestrator
{
    /// <summary>
    /// Returns all base midpoints for the active factors in the chart, sorted by position (0–360°).
    /// </summary>
    /// <param name="chart">The full chart with all factor positions.</param>
    /// <param name="factorConfig">Determines which factors are included.</param>
    /// <returns>Sorted list of <see cref="BaseMidpoint"/>, or empty if fewer than two active factors are found.</returns>
    public static List<BaseMidpoint> Calculate(FullChart chart, FactorConfig factorConfig)
    {
        var positions = ActivePositions(chart, factorConfig);
        if (positions.Count < 2) return [];
        return MidpointsCalculator.Calculate(positions);
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
}
