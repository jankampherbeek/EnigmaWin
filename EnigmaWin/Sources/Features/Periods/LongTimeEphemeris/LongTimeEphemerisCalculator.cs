// LongTimeEphemerisCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Periods.LongTimeEphemeris;

public static class LongTimeEphemerisCalculator
{
    private static readonly HashSet<Factors> HelioExcluded =
    [
        Factors.Sun, Factors.Moon, Factors.NorthNodeMean, Factors.ApogeeMean,
        Factors.ApogeeKoch, Factors.ApogeeDuval, Factors.ApogeeInterpolated,
        Factors.Priapus, Factors.PriapusKoch,
        Factors.PriapusDuval, Factors.PriapusInterpolated, Factors.Dragon,
        Factors.Beast, Factors.SouthNodeMean, Factors.BlackSun, Factors.Diamond
    ];

    private static readonly HashSet<Factors> AlwaysExcluded = [Factors.LogTimeScale, Factors.AgePoint];

    /// <summary>Factors that can meaningfully be shown for the given observer position, excluding non-positional factors.</summary>
    public static IReadOnlyList<Factors> AvailableFactors(ObserverPositions observerPosition)
    {
        var positionExcluded = observerPosition == ObserverPositions.Heliocentric
            ? HelioExcluded
            : [Factors.Earth];

        return Enum.GetValues<Factors>().Where(f =>
        {
            if (AlwaysExcluded.Contains(f)) return false;
            if (positionExcluded.Contains(f)) return false;
            var ct = f.CalculationType();
            return ct is not (CalculationTypes.Mundane or CalculationTypes.Lots
                or CalculationTypes.ZodiacFixed or CalculationTypes.Unknown);
        }).ToList();
    }

    /// <summary>Calculates one row (all selected factors, one coordinate) for a single Julian Day.</summary>
    public static Dictionary<Factors, double> CalculateRow(
        double jd, IReadOnlyList<Factors> factors, LongTimeEphemerisCoordinate coordinate,
        CalculationConfig config, SEWrapper seWrapper, Ayanamshas ayanamsha)
    {
        var ayanamshaOffset = 0.0;
        if (ayanamsha != Ayanamshas.Tropical)
            ayanamshaOffset = SEWrapper.GetAyanamshaOffset(jd);

        var eclFlags = SEFlags.DefineFlags(config, CoordinateSystems.Ecliptical);
        var eqFlags  = SEFlags.DefineFlags(config, CoordinateSystems.Equatorial);

        var values = new Dictionary<Factors, double>();

        foreach (var factor in factors)
        {
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
                    var calcRequest = new CalcRequest(jd, [factor], (int)HouseSystems.NoHouses.SeId(),
                        0, 0.0, 0.0, 0.0, config);
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

            values[factor] = coordinate switch
            {
                LongTimeEphemerisCoordinate.Longitude        => lon,
                LongTimeEphemerisCoordinate.Latitude          => lat,
                LongTimeEphemerisCoordinate.RightAscension    => ra,
                LongTimeEphemerisCoordinate.Declination        => dec,
                LongTimeEphemerisCoordinate.Distance           => dist,
                LongTimeEphemerisCoordinate.SpeedLongitude     => speedLon,
                LongTimeEphemerisCoordinate.SpeedDeclination   => speedDec,
                _                                                => 0.0
            };
        }

        return values;
    }
}
