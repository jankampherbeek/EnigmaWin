// PeriodDifference.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Periods.CyclesAstronomical;

/// <summary>
/// Calculates the difference in a single coordinate between two celestial factors
/// across a time period at regular intervals.
///
/// For longitude and right ascension the result is the shortest angular distance (0°–180°).
/// For latitude, declination, and distance the result is the absolute difference.
/// </summary>
public static class PeriodDifference
{
    /// <summary>Calculates the positional difference between two factors at each step.</summary>
    /// <param name="request">The PeriodDifferenceRequest describing the two factors, interval,
    /// time range, and coordinate to compare.</param>
    /// <returns>A list of (JulianDay, Difference) tuples in chronological order.</returns>
    public static List<(double JulianDay, double Difference)> PerformCalculation(
        PeriodDifferenceRequest request)
    {
        var config = new CalculationConfig(
            HouseSystems.NoHouses,
            request.Ayanamsha,
            request.ObserverPosition,
            ProjectionTypes.TwoDimensional,
            LunarNodeTypes.MeanNode,
            LotsTypes.Sect);

        var results = new List<(double JulianDay, double Difference)>();
        var jd = request.JdStart;

        while (jd <= request.JdEnd)
        {
            var pos1 = CalculatePosition(request.Factor1, jd, request.Coordinate, config);
            var pos2 = CalculatePosition(request.Factor2, jd, request.Coordinate, config);
            results.Add((JulianDay: jd, Difference: ComputeDifference(pos1, pos2, request.Coordinate)));
            jd += request.Interval;
        }

        return results;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static double CalculatePosition(
        Factors factor, double jd, Coordinates coordinate, CalculationConfig config)
    {
        var calcRequest = new CalcRequest(
            jd,
            [factor],
            (int)HouseSystems.NoHouses.SeId(),
            0,
            0.0,
            0.0,
            0.0,
            config);

        var (_, position) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            calcRequest, coordinate);
        return position;
    }

    /// <summary>Shortest arc (0°–180°) for longitude and right ascension; absolute difference for all others.</summary>
    private static double ComputeDifference(double pos1, double pos2, Coordinates coordinate)
    {
        switch (coordinate)
        {
            case Coordinates.Longitude:
            case Coordinates.RightAscension:
                var diff = Math.Abs(pos1 - pos2) % 360.0;
                return diff > 180.0 ? 360.0 - diff : diff;
            default:
                return Math.Abs(pos1 - pos2);
        }
    }
}
