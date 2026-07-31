// SynastryMidpointsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;

namespace EnigmaWin.Sources.Features.Synastry;

/// <summary>
/// Calculates cross-chart midpoints for synastry: one chart's own factor-pair midpoints,
/// tested against the other chart's factors as occupants. Called in both directions (A's
/// midpoints occupied by B, and vice versa) so both charts get their own midpoint tree/table.
/// </summary>
public static class SynastryMidpointsOrchestrator
{
    /// <summary>Returns midpoints formed by <paramref name="midpointChart"/> that are occupied
    /// by a factor from <paramref name="occupantChart"/>.</summary>
    public static List<MidpointMatch> Midpoints(
        FullChart midpointChart, FullChart occupantChart,
        FactorConfig factorConfig, OrbConfig orbConfig, MidpointDialType dialType)
    {
        var midpointPositions = ActivePositions(midpointChart, factorConfig);
        if (midpointPositions.Count < 2) return [];
        var mids = MidpointsCalculator.Calculate(midpointPositions);

        var occupantPositions = ActivePositions(occupantChart, factorConfig);
        if (occupantPositions.Count == 0) return [];

        var orb = dialType switch
        {
            MidpointDialType.Dial90 => orbConfig.Midpoint90DialOrb,
            MidpointDialType.Dial45 => orbConfig.Midpoint45DialOrb,
            _                       => orbConfig.Midpoint360DialOrb
        };

        return MidpointMatchFinder.Find(mids, occupantPositions, dialType, orb);
    }

    private static List<(Factors Factor, double Longitude)> ActivePositions(
        FullChart chart, FactorConfig factorConfig)
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

    private static double MundaneLongitude(Factors factor, HousePositions hp) => factor switch
    {
        Factors.Ascendant => hp.Ascendant.Longitude,
        Factors.Mc        => hp.Midheaven.Longitude,
        Factors.EastPoint => hp.Eastpoint.Longitude,
        Factors.Vertex    => hp.Vertex.Longitude,
        _                 => -1.0
    };
}
