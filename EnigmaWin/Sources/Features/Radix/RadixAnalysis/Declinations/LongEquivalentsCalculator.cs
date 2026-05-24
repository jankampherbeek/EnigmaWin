// LongEquivalentsCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

public static class LongEquivalentsCalculator
{
    public static List<LongEquivalentResult> Calculate(
        FullChart chart,
        FactorConfig factorConfig)
    {
        var obliquity = chart.Obliquity;
        var sinObl    = Math.Sin(obliquity * Math.PI / 180.0);
        if (sinObl == 0.0) return [];

        var results = new List<LongEquivalentResult>();

        foreach (var setting in factorConfig.Settings)
        {
            if (!setting.IsUsed) continue;

            var lonAndDecl = GetLongitudeAndDeclination(setting.Factor, chart);
            if (lonAndDecl is null) continue;

            var (longitude, rawDecl) = lonAndDecl.Value;

            var declination = rawDecl;
            var isOob       = false;
            if (Math.Abs(rawDecl) > obliquity)
            {
                var oobPart = Math.Abs(rawDecl) - obliquity;
                declination = rawDecl > 0.0
                    ? obliquity - oobPart
                    : oobPart - obliquity;
                isOob = true;
            }

            var sinDecl    = Math.Sin(declination * Math.PI / 180.0);
            var candidate1 = Math.Asin(sinDecl / sinObl) * 180.0 / Math.PI;
            if (candidate1 < 0.0) candidate1 += 360.0;

            var candidate2 = longitude < 180.0
                ? 90.0  + (90.0  - candidate1)
                : 270.0 + (270.0 - candidate1);

            var equivalent = Math.Abs(candidate1 - longitude) <= Math.Abs(candidate2 - longitude)
                ? candidate1
                : candidate2;

            results.Add(new LongEquivalentResult(setting.Factor, equivalent, isOob));
        }

        return results;
    }

    private static (double Longitude, double Declination)? GetLongitudeAndDeclination(
        Factors factor, FullChart chart)
    {
        FullCuspPosition? cusp = factor switch
        {
            Factors.Ascendant => chart.HousePositions.Ascendant,
            Factors.Mc        => chart.HousePositions.Midheaven,
            Factors.EastPoint => chart.HousePositions.Eastpoint,
            Factors.Vertex    => chart.HousePositions.Vertex,
            _                 => null
        };

        if (cusp is not null) return (cusp.Longitude, cusp.Declination);

        if (chart.Coordinates.TryGetValue(factor, out var pos)
            && pos.Ecliptical.Length > 0
            && pos.Equatorial.Length > 0)
            return (pos.Ecliptical[0].MainPos, pos.Equatorial[0].Deviation);

        return null;
    }
}
