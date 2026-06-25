// PeriodCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Cycles.CyclesAstronomical;

/// <summary>Calculates a single coordinate for one factor across a time period at regular intervals.</summary>
public static class PeriodCalculator
{
    /// <summary>Calculates positions for a single factor at evenly spaced Julian Day steps.</summary>
    /// <param name="request">The PeriodCalculatorRequest describing the factor, interval, time range,
    /// and coordinate to calculate.</param>
    /// <returns>A list of (JulianDay, Position) tuples in chronological order, one entry per step
    /// from JdStart up to and including JdEnd.</returns>
    public static List<(double JulianDay, double Position)> PerformCalculation(
        PeriodCalculatorRequest request)
    {
        var config = new CalculationConfig(
            HouseSystems.NoHouses,
            request.Ayanamsha,
            request.ObserverPosition,
            ProjectionTypes.TwoDimensional,
            LunarNodeTypes.MeanNode,
            LotsTypes.Sect);

        var results = new List<(double JulianDay, double Position)>();
        var jd = request.JdStart;

        while (jd <= request.JdEnd)
        {
            var calcRequest = new CalcRequest(
                jd,
                [request.Factor],
                (int)HouseSystems.NoHouses.SeId(),
                0,
                0.0,
                0.0,
                0.0,
                config);

            var result = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
                calcRequest,
                request.Coordinate);

            results.Add(result);
            jd += request.Interval;
        }

        return results;
    }
}
