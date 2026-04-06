// DeclinationsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

/// <summary>
/// Orchestrates declination calculations for a chart.
/// Intended to be called directly from the UI layer.
/// </summary>
public static class DeclinationsOrchestrator
{
    /// <summary>
    /// Returns all parallels and contra-parallels for the active factors in the chart.
    /// </summary>
    /// <param name="chart">The full chart with all factor positions.</param>
    /// <param name="factorConfig">Determines which factors are included (<c>IsUsed == true</c>).</param>
    /// <param name="orbConfig">Supplies <see cref="OrbConfig.ParallelOrb"/>.</param>
    /// <returns>All <see cref="DefinedParallel"/> instances whose actual orb ≤ <c>ParallelOrb</c>.</returns>
    public static List<DefinedParallel> Parallels(
        FullChart chart,
        FactorConfig factorConfig,
        OrbConfig orbConfig)
        => ParallelsCalculator.Calculate(chart, factorConfig, orbConfig);

    /// <summary>
    /// Returns all occupied midpoints in declination for the active factors in the chart,
    /// sorted by <see cref="DeclOccupiedMidpoint.ActualOrb"/> ascending (most exact first).
    /// </summary>
    /// <param name="chart">The full chart with all factor positions.</param>
    /// <param name="factorConfig">Determines which factors are included (<c>IsUsed == true</c>).</param>
    /// <param name="orbConfig">Supplies <see cref="OrbConfig.DeclinationMidpointOrb"/>.</param>
    /// <returns>All <see cref="DeclOccupiedMidpoint"/> instances whose actual orb ≤ <c>DeclinationMidpointOrb</c>.</returns>
    public static List<DeclOccupiedMidpoint> OccupiedMidpoints(
        FullChart chart,
        FactorConfig factorConfig,
        OrbConfig orbConfig)
    {
        var baseMidpoints = DeclMidpointsCalculator.BaseMidpoints(chart, factorConfig);
        var positions     = DeclMidpointsCalculator.ActivePairs(chart, factorConfig);
        return DeclinationMidpointsMatchFinder.Find(baseMidpoints, positions, orbConfig.DeclinationMidpointOrb);
    }

    /// <summary>
    /// Returns longitude equivalents for all active factors in the chart.
    /// </summary>
    /// <param name="chart">The full chart with all factor positions; <see cref="FullChart.Obliquity"/> is read directly.</param>
    /// <param name="factorConfig">Determines which factors are included (<c>IsUsed == true</c>).</param>
    /// <returns>One <see cref="LongEquivalentResult"/> per active factor present in the chart.</returns>
    public static List<LongEquivalentResult> LongitudeEquivalents(
        FullChart chart,
        FactorConfig factorConfig)
        => LongEquivalentsCalculator.Calculate(chart, factorConfig);
}
