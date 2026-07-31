// MonthlyEphemerisCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Periods.MonthlyEphemeris;

/// <summary>Result row for one day in the ephemeris.</summary>
public sealed class EphemerisRow
{
    public double JulianDay  { get; }
    public int    Year       { get; }
    public int    Month      { get; }
    public int    Day        { get; }

    public IReadOnlyDictionary<Factors, EphemerisPositions> Positions { get; }

    public EphemerisRow(double jd, int year, int month, int day,
        Dictionary<Factors, EphemerisPositions> positions)
    {
        JulianDay = jd;
        Year      = year;
        Month     = month;
        Day       = day;
        Positions = positions;
    }
}

/// <summary>All six coordinates for one factor on one day.</summary>
public sealed class EphemerisPositions
{
    public double Longitude         { get; }
    public double Latitude          { get; }
    public double RightAscension    { get; }
    public double Declination       { get; }
    public double Distance          { get; }
    public double SpeedLongitude    { get; }
    public double SpeedDeclination  { get; }

    public EphemerisPositions(double longitude, double latitude, double rightAscension,
        double declination, double distance, double speedLongitude, double speedDeclination)
    {
        Longitude        = longitude;
        Latitude         = latitude;
        RightAscension   = rightAscension;
        Declination      = declination;
        Distance         = distance;
        SpeedLongitude   = speedLongitude;
        SpeedDeclination = speedDeclination;
    }
}

public static class MonthlyEphemerisCalculator
{
    /// <summary>Calculates daily positions for all given factors for every day of the specified month.</summary>
    public static List<EphemerisRow> Calculate(
        int year, int month, IReadOnlyList<Factors> factors,
        Ayanamshas ayanamsha, ObserverPositions observerPosition)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var rows = new List<EphemerisRow>(daysInMonth);

        var config = new CalculationConfig(
            HouseSystems.NoHouses,
            ayanamsha,
            observerPosition,
            ProjectionTypes.TwoDimensional,
            LunarNodeTypes.MeanNode,
            LotsTypes.Sect);

        if (ayanamsha != Ayanamshas.Tropical)
            SEWrapper.SetAyanamsha(ayanamsha.SeId());

        var seWrapper = new SEWrapper();

        for (var day = 1; day <= daysInMonth; day++)
        {
            var jd = SEWrapper.JulianDay(
                new AstronomicalDate(year, month, day, Gregorian: true),
                new AstronomicalTime(0, 0, 0));

            var posDict = new Dictionary<Factors, EphemerisPositions>();

            foreach (var factor in factors)
            {
                posDict[factor] = CalculateOneDay(factor, jd, config, seWrapper, ayanamsha);
            }

            rows.Add(new EphemerisRow(jd, year, month, day, posDict));
        }

        return rows;
    }

    private static EphemerisPositions CalculateOneDay(
        Factors factor, double jd, CalculationConfig config, SEWrapper seWrapper, Ayanamshas ayanamsha)
    {
        var ayanamshaOffset = 0.0;
        if (ayanamsha != Ayanamshas.Tropical)
            ayanamshaOffset = SEWrapper.GetAyanamshaOffset(jd);

        var calcRequest = new CalcRequest(jd, [factor], (int)HouseSystems.NoHouses.SeId(),
            0, 0.0, 0.0, 0.0, config);

        var eclFlags = SEFlags.DefineFlags(config, CoordinateSystems.Ecliptical);
        var eqFlags  = SEFlags.DefineFlags(config, CoordinateSystems.Equatorial);

        double lon = 0, lat = 0, ra = 0, dec = 0, dist = 0, speedLon = 0, speedDec = 0;

        switch (factor.CalculationType())
        {
            case CalculationTypes.CommonSe:
            {
                var eclPos = SEWrapper.CalculateFactorPosition(jd, factor.SeId(), eclFlags);
                var eqPos  = SEWrapper.CalculateFactorPosition(jd, factor.SeId(), eqFlags);
                if (eclPos is not null)
                {
                    lon      = eclPos.MainPos - ayanamshaOffset;
                    lat      = eclPos.Deviation;
                    dist     = eclPos.Distance;
                    speedLon = eclPos.MainPosSpeed;
                }
                if (eqPos is not null)
                {
                    ra       = eqPos.MainPos;
                    dec      = eqPos.Deviation;
                    speedDec = eqPos.DeviationSpeed;
                }
                break;
            }

            default:
            {
                lon  = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(calcRequest, Coordinates.Longitude).Position;
                lat  = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(calcRequest, Coordinates.Latitude).Position;
                ra   = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(calcRequest, Coordinates.RightAscension).Position;
                dec  = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(calcRequest, Coordinates.Declination).Position;
                dist = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(calcRequest, Coordinates.Distance).Position;

                var nextCalcRequest = new CalcRequest(jd + 1.0, [factor], (int)HouseSystems.NoHouses.SeId(),
                    0, 0.0, 0.0, 0.0, config);
                var lonNext = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(nextCalcRequest, Coordinates.Longitude).Position;
                var decNext = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(nextCalcRequest, Coordinates.Declination).Position;
                speedLon = lonNext - lon;
                speedDec = decNext - dec;
                break;
            }
        }

        if (lon < 0) lon += 360.0;
        if (lon >= 360.0) lon -= 360.0;

        return new EphemerisPositions(lon, lat, ra, dec, dist, speedLon, speedDec);
    }
}
