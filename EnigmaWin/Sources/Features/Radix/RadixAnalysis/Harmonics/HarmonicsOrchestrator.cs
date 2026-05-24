// HarmonicsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics;

/// <summary>Orchestrates harmonic calculations for a chart.</summary>
public static class HarmonicsOrchestrator
{
    public static List<(Factors Factor, double Longitude)> Calculate(
        FullChart chart,
        UserConfiguration config,
        double harmonic)
    {
        var positions = ActivePositions(chart, config.FactorConfig);
        if (positions.Count == 0) return [];
        return HarmonicsCalculator.Calculate(positions, harmonic);
    }

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

    private static double MundaneLongitude(Factors factor, HousePositions hp) => factor switch
    {
        Factors.Ascendant => hp.Ascendant.Longitude,
        Factors.Mc        => hp.Midheaven.Longitude,
        Factors.EastPoint => hp.Eastpoint.Longitude,
        Factors.Vertex    => hp.Vertex.Longitude,
        _                 => -1.0
    };
}
