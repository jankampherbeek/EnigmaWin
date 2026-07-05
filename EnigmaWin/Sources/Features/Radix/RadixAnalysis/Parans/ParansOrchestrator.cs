// ParansOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Parans;

public static class ParansOrchestrator
{
    public static ParansResult Calculate(
        FullChart chart,
        double geoLon, double geoLat, double height,
        FactorConfig factorConfig,
        FixStarConfig fixStarConfig,
        CalculationConfig calculationConfig,
        double paranTimeOrbMinutes)
    {
        var jdUt       = chart.JulianDay;
        var searchFrom = jdUt - 0.5;

        if (calculationConfig.ObserverPosition == ObserverPositions.Topocentric)
            SEWrapper.SetTopocentric(geoLon, geoLat, height);

        var activeFactors = ActiveFactorsFromChart(chart, factorConfig);
        var activeStars   = ActiveStarsFromConfig(fixStarConfig);

        var factorTimes = activeFactors
            .Select(f => CalcParanTimes(ParanBodyKind.Factor, f, null, searchFrom, geoLon, geoLat, height))
            .ToList();

        var starTimes = activeStars
            .Select(s => CalcParanTimes(ParanBodyKind.Star, null, s, searchFrom, geoLon, geoLat, height))
            .ToList();

        var allTimes = factorTimes.Concat(starTimes).ToList();
        var orbDays  = paranTimeOrbMinutes / 1440.0;
        var matches  = FindMatches(allTimes, factorTimes, orbDays);

        return new ParansResult(allTimes, matches);
    }

    private static List<Factors> ActiveFactorsFromChart(FullChart chart, FactorConfig factorConfig)
    {
        var usedFactors = factorConfig.Settings
            .Where(s => s.IsUsed && s.Factor.CalculationType() == CalculationTypes.CommonSe)
            .Select(s => s.Factor)
            .ToHashSet();
        return chart.Coordinates.Keys
            .Where(usedFactors.Contains)
            .OrderBy(f => f.SeId())
            .ToList();
    }

    private static List<StarDefinitions> ActiveStarsFromConfig(FixStarConfig fixStarConfig)
    {
        return fixStarConfig.ActiveSelection switch
        {
            FixStarSelections.SelfDefined =>
                StarDefinitions.AllCases
                    .Where(s => fixStarConfig.FixStarSettings.Any(fs => fs.FixStar.Id == s.Id && fs.IsUsed))
                    .ToList(),
            FixStarSelections.Magnitude =>
                StarDefinitions.AllCases
                    .Where(s => s.Magnitude <= fixStarConfig.MagnitudeLimit)
                    .ToList(),
            _ => FixStarsOrchestrator.GetSelection(fixStarConfig.ActiveSelection).ToList()
        };
    }

    private static ParanTimesForBody CalcParanTimes(
        ParanBodyKind kind, Factors? factor, StarDefinitions? star,
        double searchFrom, double geoLon, double geoLat, double height)
    {
        int    ipl      = kind == ParanBodyKind.Factor ? (factor?.SeId() ?? 0) : 0;
        string starName = kind == ParanBodyKind.Star && star != null
            ? "," + star.AstronomicalName
            : string.Empty;

        double? Calc(ParanType t) =>
            SEWrapper.CalculateRiseTrans(searchFrom, ipl, starName, t.Rsmi(), geoLon, geoLat, height);

        return new ParanTimesForBody(kind, factor, star,
            Rising:          Calc(ParanType.Rising),
            Setting:         Calc(ParanType.Setting),
            Culmination:     Calc(ParanType.Culmination),
            AntiCulmination: Calc(ParanType.AntiCulmination));
    }

    private static List<ParanMatch> FindMatches(
        List<ParanTimesForBody> allTimes,
        List<ParanTimesForBody> factorTimes,
        double orbDays)
    {
        var matches = new List<ParanMatch>();
        foreach (var b1 in allTimes)
        {
            foreach (var b2 in factorTimes)
            {
                if (b1 == b2) continue;
                if (b1.Kind == ParanBodyKind.Star && b2.Kind == ParanBodyKind.Star) continue;

                foreach (var t1 in ParanTypeExtensions.All)
                {
                    var time1 = b1.TimeFor(t1);
                    if (time1 is null) continue;
                    foreach (var t2 in ParanTypeExtensions.All)
                    {
                        var time2 = b2.TimeFor(t2);
                        if (time2 is null) continue;
                        var diff = Math.Abs(time1.Value - time2.Value);
                        if (diff <= orbDays)
                            matches.Add(new ParanMatch(b1, t1, time1.Value, b2, t2, time2.Value, diff * 1440.0));
                    }
                }
            }
        }
        return [.. matches.OrderBy(m => m.OrbMinutes)];
    }
}
