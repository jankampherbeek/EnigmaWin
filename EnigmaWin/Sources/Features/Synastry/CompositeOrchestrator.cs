// CompositeOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWin.Sources.Features.Synastry;

/// <summary>How the composite chart's houses (cusps and angles) are derived.</summary>
public abstract record CompositeHouseMethod
{
    /// <summary>Every cusp/angle is the circular mean of the natal cusps/angles.</summary>
    public sealed record MidpointsOnly : CompositeHouseMethod;

    /// <summary>The composite MC is the circular mean of the natal MCs; ARMC is derived from it
    /// and the houses are (re)computed at the given geographic latitude/longitude.</summary>
    public sealed record ReferenceLocation(double Latitude, double Longitude) : CompositeHouseMethod;
}

/// <summary>
/// Builds a composite chart: every factor sits on the (circular) mean of its natal positions
/// across two or more charts. Two conventions for the houses are supported, see
/// <see cref="CompositeHouseMethod"/>.
/// </summary>
public static class CompositeOrchestrator
{
    public static FullChart Calculate(IReadOnlyList<FullChart> charts, int houseSystem, CompositeHouseMethod method)
    {
        if (charts.Count < 2)
            throw new ArgumentException("Composite chart needs at least two charts.", nameof(charts));

        var count = charts.Count;
        var obliquity = charts.Average(c => c.Obliquity);
        var julianDay = charts.Average(c => c.JulianDay);

        var (coordinates, omittedFactors) = CompositeCoordinates(charts, obliquity, count);
        var housePositions = CompositeHousePositions(charts, obliquity, julianDay, houseSystem, method);

        return new FullChart(
            Coordinates: coordinates,
            HousePositions: housePositions,
            SiderealTime: housePositions.Midheaven.RightAscension / 15.0,
            JulianDay: julianDay,
            Obliquity: obliquity,
            OmittedFactors: omittedFactors);
    }

    // ── Coordinates (planets, nodes, lots, asteroids, …) ────────────────────────

    private static (Dictionary<Factors, FullFactorPosition> Coordinates, List<Factors> Omitted) CompositeCoordinates(
        IReadOnlyList<FullChart> charts, double obliquity, int count)
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        var omitted = new List<Factors>();
        var seWrapper = new SEWrapper();

        var commonFactors = charts
            .Skip(1)
            .Aggregate(new HashSet<Factors>(charts[0].Coordinates.Keys), (acc, c) => { acc.IntersectWith(c.Coordinates.Keys); return acc; });

        foreach (var factor in commonFactors)
        {
            if (factor.CalculationType() == CalculationTypes.Mundane) continue;
            if (charts.Any(c => c.OmittedFactors?.Contains(factor) == true))
            {
                omitted.Add(factor);
                continue;
            }

            var positions = charts
                .Select(c => c.Coordinates.TryGetValue(factor, out var p) && p.Ecliptical.Length > 0 ? p.Ecliptical[0] : null)
                .Where(p => p is not null)
                .Select(p => p!)
                .ToList();
            if (positions.Count != count) continue;

            var longitude = CircularMeanCalculator.Calculate(positions.Select(p => p.MainPos));
            var latitude  = positions.Average(p => p.Deviation);
            var distance  = positions.Average(p => p.Distance);
            var mainSpeed = positions.Average(p => p.MainPosSpeed);
            var devSpeed  = positions.Average(p => p.DeviationSpeed);
            var distSpeed = positions.Average(p => p.DistanceSpeed);

            var ecliptical = new MainAstronomicalPosition(longitude, latitude, distance, mainSpeed, devSpeed, distSpeed);
            var (ra, dec) = seWrapper.EclipticToEquatorial([longitude, latitude], obliquity);
            var equatorial = new MainAstronomicalPosition(ra, dec, distance);

            coordinates[factor] = new FullFactorPosition(
                Ecliptical: [ecliptical],
                Equatorial: [equatorial],
                Horizontal: [new HorizontalPosition(0.0, 0.0)]);
        }

        return (coordinates, omitted);
    }

    // ── House positions ──────────────────────────────────────────────────────

    private static HousePositions CompositeHousePositions(
        IReadOnlyList<FullChart> charts, double obliquity, double julianDay, int houseSystem, CompositeHouseMethod method)
    {
        var seWrapper = new SEWrapper();

        FullCuspPosition CuspPosition(double longitude, (double Latitude, double Longitude)? geo)
        {
            var (ra, dec) = seWrapper.EclipticToEquatorial([longitude, 0.0], obliquity);
            if (geo is null)
                return new FullCuspPosition(longitude, ra, dec, new HorizontalPosition(0.0, 0.0));

            var horizontal = seWrapper.AzimuthAndAltitude(julianDay, ra, dec, geo.Value.Latitude, geo.Value.Longitude, 0.0);
            return new FullCuspPosition(longitude, ra, dec, new HorizontalPosition(horizontal[0], horizontal[1]));
        }

        FullCuspPosition MeanCusp(IEnumerable<double> longitudes) =>
            CuspPosition(CircularMeanCalculator.Calculate(longitudes), null);

        HousePositions MidpointsOnlyHousePositions()
        {
            var cuspCount = charts[0].HousePositions.Cusps.Length;
            var cusps = Enumerable.Range(0, cuspCount)
                .Select(i => MeanCusp(charts.Select(c => c.HousePositions.Cusps[i].Longitude)))
                .ToArray();
            return new HousePositions(
                Cusps:      cusps,
                Ascendant:  MeanCusp(charts.Select(c => c.HousePositions.Ascendant.Longitude)),
                Midheaven:  MeanCusp(charts.Select(c => c.HousePositions.Midheaven.Longitude)),
                Eastpoint:  MeanCusp(charts.Select(c => c.HousePositions.Eastpoint.Longitude)),
                Vertex:     MeanCusp(charts.Select(c => c.HousePositions.Vertex.Longitude)));
        }

        switch (method)
        {
            case CompositeHouseMethod.MidpointsOnly:
                return MidpointsOnlyHousePositions();

            case CompositeHouseMethod.ReferenceLocation refLoc:
            {
                var mcLongitude = CircularMeanCalculator.Calculate(charts.Select(c => c.HousePositions.Midheaven.Longitude));
                var (armc, _) = seWrapper.EclipticToEquatorial([mcLongitude, 0.0], obliquity);
                var geo = (refLoc.Latitude, refLoc.Longitude);

                try
                {
                    var houseResult = seWrapper.CalculateHousesArmc(armc, refLoc.Latitude, obliquity, (char)houseSystem);
                    var rawCusps = houseResult[0];
                    var ascmc    = houseResult[1];
                    if (rawCusps.Length < 13 || ascmc.Length < 5)
                        return MidpointsOnlyHousePositions();

                    var cusps = Enumerable.Range(1, 12).Select(i => CuspPosition(rawCusps[i], geo)).ToArray();
                    return new HousePositions(
                        Cusps:      cusps,
                        Ascendant:  CuspPosition(ascmc[0], geo),
                        Midheaven:  CuspPosition(ascmc[1], geo),
                        Eastpoint:  CuspPosition(ascmc[4], geo),
                        Vertex:     CuspPosition(ascmc[3], geo));
                }
                catch (Exception)
                {
                    return MidpointsOnlyHousePositions();
                }
            }

            default:
                return MidpointsOnlyHousePositions();
        }
    }
}
