// HarmonicsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics;

/// <summary>
/// Orchestrates harmonic calculations for a chart.
/// Intended to be called from the UI layer via <see cref="IConfigContext.ActiveConfig"/>.
/// </summary>
public static class HarmonicsOrchestrator
{
    /// <summary>
    /// Returns harmonic positions for all active factors in the chart.
    /// </summary>
    /// <param name="chart">The full chart with all calculated positions.</param>
    /// <param name="config">The active user configuration (factors are read from <see cref="UserConfiguration.FactorConfig"/>).</param>
    /// <param name="harmonic">The harmonic number to apply.</param>
    /// <returns>List of (factor, harmonic longitude) pairs, or empty if no active factors are found.</returns>
    public static List<(Factors Factor, double Longitude)> Calculate(
        FullChart chart,
        UserConfiguration config,
        double harmonic)
    {
        var positions = ActivePositions(chart, config.FactorConfig);
        if (positions.Count == 0) return [];
        return HarmonicsCalculator.Calculate(positions, harmonic);
    }

    /// <summary>
    /// Returns all harmonic matches for the active factors in the chart, sorted by orb (most exact first).
    /// </summary>
    /// <param name="chart">The full chart with all calculated positions.</param>
    /// <param name="config">The active user configuration (factors from <see cref="UserConfiguration.FactorConfig"/>, orb from <see cref="UserConfiguration.OrbConfig"/>).</param>
    /// <param name="harmonic">The harmonic number to apply.</param>
    /// <returns>Sorted list of <see cref="HarmonicsMatch"/>, or empty if fewer than two active factors are found.</returns>
    public static List<HarmonicsMatch> Matches(
        FullChart chart,
        UserConfiguration config,
        double harmonic)
    {
        var positions = ActivePositions(chart, config.FactorConfig);
        if (positions.Count < 2) return [];
        var harmonicPositions = HarmonicsCalculator.Calculate(positions, harmonic);
        return HarmonicsMatchFinder.Find(harmonicPositions, positions, config.OrbConfig.HarmonicOrb);
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
