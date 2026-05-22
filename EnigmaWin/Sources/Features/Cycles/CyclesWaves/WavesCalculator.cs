// WavesCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Cycles.CyclesWaves;

/// <summary>
/// Calculates a "wave value" across a time period based on the total pairwise
/// separation of a set of slow-moving planets for a given coordinate.
///
/// Three cycle types are supported:
/// - Saturn (default): Saturn, Uranus, Neptune, Pluto — 6 pairs.
/// - Jupiter: Jupiter, Saturn, Uranus, Neptune, Pluto — 10 pairs.
/// - Uranus: Uranus, Neptune, Pluto — 3 pairs.
///
/// For longitude and right ascension the pairwise distance is the shortest arc (0°–180°).
/// For all other coordinates (latitude, declination, distance) it is the absolute difference.
/// All pairwise distances are summed to produce a single wave value for each time step.
/// </summary>
public static class WavesCalculator
{
    /// <summary>Calculates the wave value using a WavesRequest.</summary>
    /// <param name="request">The WavesRequest describing the cycle type, interval, time range,
    /// and coordinate to calculate.</param>
    /// <returns>A list of (JulianDay, WaveValue) tuples in chronological order.</returns>
    public static List<(double JulianDay, double WaveValue)> PerformCalculation(
        WavesRequest request)
    {
        return PerformCalculation(
            startJdNr: request.JdStart,
            endJdNr: request.JdEnd,
            interval: request.Interval,
            cycleType: request.CycleType,
            coordinate: request.Coordinate,
            observerPosition: request.ObserverPosition);
    }

    /// <summary>Calculates the wave value at each step between two Julian Day numbers.</summary>
    /// <param name="startJdNr">Julian Day number for the start of the period.</param>
    /// <param name="endJdNr">Julian Day number for the end of the period (inclusive).</param>
    /// <param name="interval">Step size in days between successive calculations.</param>
    /// <param name="cycleType">The planet that defines the cycle. Must be Jupiter, Saturn, or Uranus.
    /// Defaults to Saturn.</param>
    /// <param name="coordinate">The coordinate to calculate for each planet.
    /// Defaults to Longitude.</param>
    /// <param name="observerPosition">The observer position. Defaults to Geocentric.</param>
    /// <returns>A list of (JulianDay, WaveValue) tuples in chronological order.</returns>
    public static List<(double JulianDay, double WaveValue)> PerformCalculation(
        double startJdNr,
        double endJdNr,
        int interval,
        Factors cycleType = Factors.Saturn,
        Coordinates coordinate = Coordinates.Longitude,
        ObserverPositions observerPosition = ObserverPositions.Geocentric)
    {
        var config = new CalculationConfig(
            HouseSystems.NoHouses,
            Ayanamshas.Tropical,
            observerPosition,
            ProjectionTypes.TwoDimensional,
            BlackMoonCorrectionTypes.Duval,
            LunarNodeTypes.MeanNode,
            LotsTypes.Sect);

        var step = (double)interval;
        var outerPlanets = PlanetsForCycleType(cycleType);
        var results = new List<(double JulianDay, double WaveValue)>();
        var jd = startJdNr;

        while (jd <= endJdNr)
        {
            var positions = new Dictionary<Factors, double>();
            foreach (var factor in outerPlanets)
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
                positions[factor] = position;
            }

            var waveValue = 0.0;
            for (var i = 0; i < outerPlanets.Count; i++)
            {
                for (var j = i + 1; j < outerPlanets.Count; j++)
                {
                    var p1 = positions.GetValueOrDefault(outerPlanets[i], 0.0);
                    var p2 = positions.GetValueOrDefault(outerPlanets[j], 0.0);
                    waveValue += ComputeDifference(p1, p2, coordinate);
                }
            }

            results.Add((JulianDay: jd, WaveValue: waveValue));
            jd += step;
        }

        return results;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>Returns the planet list for the given cycle type.
    /// Only Jupiter, Saturn, and Uranus are valid; any other value falls back to Saturn.</summary>
    private static List<Factors> PlanetsForCycleType(Factors cycleType) => cycleType switch
    {
        Factors.Jupiter => [Factors.Jupiter, Factors.Saturn, Factors.Uranus, Factors.Neptune, Factors.Pluto],
        Factors.Uranus  => [Factors.Uranus,  Factors.Neptune, Factors.Pluto],
        _               => [Factors.Saturn,  Factors.Uranus,  Factors.Neptune, Factors.Pluto]
    };

    /// <summary>Shortest arc (0°–180°) for cyclic coordinates; absolute difference for linear ones.</summary>
    private static double ComputeDifference(double p1, double p2, Coordinates coordinate)
    {
        switch (coordinate)
        {
            case Coordinates.Longitude:
            case Coordinates.RightAscension:
                var diff = Math.Abs(p1 - p2) % 360.0;
                return diff > 180.0 ? 360.0 - diff : diff;
            default:
                return Math.Abs(p1 - p2);
        }
    }
}
