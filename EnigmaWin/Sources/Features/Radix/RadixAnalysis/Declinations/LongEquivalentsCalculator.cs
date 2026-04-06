// LongEquivalentsCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

/// <summary>Calculates the longitude equivalent for every active factor in a chart.</summary>
/// <remarks>
/// The algorithm:
/// <list type="number">
///   <item>For each active factor collect its ecliptic longitude and equatorial declination.</item>
///   <item>If <c>|declination| &gt; obliquity</c> the factor is <b>out of bounds (OOB)</b>:
///     reflect its declination back inside the ecliptic band:
///     <c>reflected = sign(decl) × (obliquity − (|decl| − obliquity))</c></item>
///   <item>Convert the (possibly reflected) declination to a longitude candidate:
///     <c>candidate1 = asin(sin(decl) / sin(obliquity))</c> (degrees, normalised to 0–360).</item>
///   <item>Compute a second candidate on the opposite arc:
///     <c>candidate2 = longitude &lt; 180 ? 90 + (90 − c1) : 270 + (270 − c1)</c></item>
///   <item>Return the candidate whose distance to the actual longitude is smaller.</item>
/// </list>
/// </remarks>
public static class LongEquivalentsCalculator
{
    /// <summary>
    /// Compute longitude equivalents for all active factors in a chart.
    /// </summary>
    /// <param name="chart">The calculated chart; supplies ecliptic longitude, equatorial
    /// declination, and <see cref="FullChart.Obliquity"/>.</param>
    /// <param name="factorConfig">Determines which factors are active (<c>IsUsed == true</c>).</param>
    /// <returns>One <see cref="LongEquivalentResult"/> per active factor that has both
    /// ecliptic and equatorial data in the chart.</returns>
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

            // OOB check and reflection
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

            // Convert declination → longitude candidate (asin, degrees)
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

    /// <summary>
    /// Returns (longitude, declination) for a factor from the chart, or <c>null</c> if not present.
    /// </summary>
    private static (double Longitude, double Declination)? GetLongitudeAndDeclination(
        Factors factor, FullChart chart)
    {
        if (chart.Coordinates.TryGetValue(factor, out var pos)
            && pos.Ecliptical.Length > 0
            && pos.Equatorial.Length > 0)
            return (pos.Ecliptical[0].MainPos, pos.Equatorial[0].Deviation);

        FullCuspPosition? cusp = factor switch
        {
            Factors.Ascendant => chart.HousePositions.Ascendant,
            Factors.Mc        => chart.HousePositions.Midheaven,
            Factors.EastPoint => chart.HousePositions.Eastpoint,
            Factors.Vertex    => chart.HousePositions.Vertex,
            _                 => null
        };

        if (cusp is null) return null;
        return (cusp.Longitude, cusp.Declination);
    }
}
