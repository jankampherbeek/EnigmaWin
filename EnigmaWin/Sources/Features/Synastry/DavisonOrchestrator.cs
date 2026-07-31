// DavisonOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Synastry;

/// <summary>One chart's Julian Day and birth location, as needed to build a Davison chart.</summary>
public readonly record struct DavisonChartInput(double JulianDay, double Latitude, double Longitude, double Obliquity, double MidheavenLongitude);

/// <summary>How the Davison (Combine) chart's moment/location is derived.</summary>
public abstract record DavisonLocationMethod
{
    /// <summary>Arithmetic mean of the latitudes and the longitudes (flat-earth mean).</summary>
    public sealed record Simplified : DavisonLocationMethod;

    /// <summary>Same flat-mean location as Simplified, but the moment in time is solved so that
    /// the resulting chart's MC equals the circular mean of the natal MCs.</summary>
    public sealed record Original : DavisonLocationMethod;

    /// <summary>A user-chosen location; only the time is the midpoint.</summary>
    public sealed record ReferenceLocation(double Latitude, double Longitude) : DavisonLocationMethod;

    /// <summary>True geographic midpoint along the shortest (great-circle) arc between the locations.</summary>
    public sealed record SphericalMidpoint : DavisonLocationMethod;
}

/// <summary>
/// Builds a Davison (Combine) chart: a real chart calculated for the midpoint moment in time (UT)
/// and a midpoint location. Unlike the Composite chart, this is a genuine Swiss Ephemeris
/// calculation for one real date/time/place, not a per-factor blend of two existing charts.
/// </summary>
public static class DavisonOrchestrator
{
    /// <summary>The calculated chart together with the Julian Day/latitude/longitude used to build it.</summary>
    public sealed record Result(FullChart Chart, double JulianDay, double Latitude, double Longitude);

    public static Result Calculate(
        IReadOnlyList<DavisonChartInput> inputs, List<Factors> factorsToUse,
        int houseSystem, ConfigData configData, DavisonLocationMethod method)
    {
        if (inputs.Count < 2)
            throw new ArgumentException("Davison chart needs at least two charts.", nameof(inputs));

        var count = inputs.Count;
        var midJulianDay  = inputs.Average(i => i.JulianDay);
        var flatLatitude  = inputs.Average(i => i.Latitude);
        var flatLongitude = inputs.Average(i => i.Longitude);

        double julianDay;
        double latitude;
        double longitude;

        switch (method)
        {
            case DavisonLocationMethod.Simplified:
                julianDay = midJulianDay;
                latitude  = flatLatitude;
                longitude = flatLongitude;
                break;

            case DavisonLocationMethod.ReferenceLocation refLoc:
                julianDay = midJulianDay;
                latitude  = refLoc.Latitude;
                longitude = refLoc.Longitude;
                break;

            case DavisonLocationMethod.SphericalMidpoint:
                julianDay = midJulianDay;
                (latitude, longitude) = SphericalMidpoint(inputs.Select(i => (i.Latitude, i.Longitude)));
                break;

            case DavisonLocationMethod.Original:
            {
                var obliquity = inputs.Average(i => i.Obliquity);
                var targetMcLongitude = CircularMeanCalculator.Calculate(inputs.Select(i => i.MidheavenLongitude));
                julianDay = SolveJulianDay(targetMcLongitude, obliquity, flatLongitude, midJulianDay);
                latitude  = flatLatitude;
                longitude = flatLongitude;
                break;
            }

            default:
                julianDay = midJulianDay;
                latitude  = flatLatitude;
                longitude = flatLongitude;
                break;
        }

        var request = new CalcRequest(julianDay, factorsToUse, houseSystem, 0, latitude, longitude, 0.0, configData);
        var chart = AstronCalcOrchestrator.PerformCalculation(request);
        return new Result(chart, julianDay, latitude, longitude);
    }

    // ── Spherical midpoint ────────────────────────────────────────────────────

    /// <summary>True midpoint of two or more geographic locations, via unit-vector averaging.</summary>
    private static (double Latitude, double Longitude) SphericalMidpoint(IEnumerable<(double Latitude, double Longitude)> coordinates)
    {
        const double toRad = Math.PI / 180.0;
        const double toDeg = 180.0 / Math.PI;

        var xs = 0.0; var ys = 0.0; var zs = 0.0; var count = 0;
        foreach (var (lat, lon) in coordinates)
        {
            var latR = lat * toRad;
            var lonR = lon * toRad;
            xs += Math.Cos(latR) * Math.Cos(lonR);
            ys += Math.Cos(latR) * Math.Sin(lonR);
            zs += Math.Sin(latR);
            count++;
        }

        var xm = xs / count;
        var ym = ys / count;
        var zm = zs / count;

        var longitudeMid = Math.Atan2(ym, xm) * toDeg;
        var hypotenuse = Math.Sqrt(xm * xm + ym * ym);
        var latitudeMid = Math.Atan2(zm, hypotenuse) * toDeg;
        return (latitudeMid, longitudeMid);
    }

    // ── Original: solve the JD so the chart's MC matches the midpoint MC ────────

    /// <summary>Finds the Julian Day (UT) closest to nominalJulianDay at which the local sidereal
    /// time at the given longitude equals the ARMC for targetMcLongitude. Converges in a few
    /// iterations since sidereal time is almost perfectly linear in JD over a fraction of a day.</summary>
    private static double SolveJulianDay(double targetMcLongitude, double obliquity, double longitude, double nominalJulianDay)
    {
        var seWrapper = new SEWrapper();
        var (armcTarget, _) = seWrapper.EclipticToEquatorial([targetMcLongitude, 0.0], obliquity);

        const double siderealRatePerDay = 360.9856473;

        var jd = nominalJulianDay;
        for (var i = 0; i < 3; i++)
        {
            var armcAtJd = NormalizeDegrees(SEWrapper.SiderealTime(jd) + longitude);
            var diff = NormalizeSignedDegrees(armcTarget - armcAtJd);
            jd += diff / siderealRatePerDay;
        }
        return jd;
    }

    private static double NormalizeDegrees(double value)
    {
        var v = value % 360.0;
        if (v < 0) v += 360.0;
        return v;
    }

    private static double NormalizeSignedDegrees(double value)
    {
        var v = NormalizeDegrees(value);
        if (v > 180.0) v -= 360.0;
        return v;
    }
}
