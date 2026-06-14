// SolarReturnOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using Serilog;

namespace EnigmaWin.Sources.Features.Progressive.Solar;

/// <summary>
/// Calculates a solar return: the Julian Day when the Sun returns to its natal longitude,
/// then computes all factor positions at that moment using AstronCalcOrchestrator.
/// </summary>
public static class SolarReturnOrchestrator
{
    private const double TropicalYearInDays = 365.242199074;
    private const int    SeSun             = 0;
    private const int    SeFlagsTropical   = 2 + 256;           // SEFLG_SWIEPH | SEFLG_SPEED
    private const int    SeFlagsSidereal   = 2 + 256 + 65536;   // + SEFLG_SIDEREAL
    private const int    FaganAyanamshaId  = 0;
    private const int    MaxIterations     = 1000;
    private const double TargetPrecision   = 1e-8;
    private const double MinInterval       = 1e-12;

    public sealed record Result(
        double                                   SolarJd,
        string                                   DateText,
        Dictionary<Factors, ProgressivePosition> Positions,
        FullChart                                SolarFullChart,
        double                                   RadixSunLongitude,
        double                                   SolarSunLongitude);

    /// <summary>
    /// Calculates the solar return for the given birth JD, age, and location.
    /// </summary>
    public static Result Calculate(
        double            birthJd,
        int               age,
        bool              useSidereal,
        double            geoLon,
        double            geoLat,
        UserConfiguration userConfig)
    {
        if (useSidereal)
            SEWrapper.SetAyanamsha(FaganAyanamshaId);

        var flags        = useSidereal ? SeFlagsSidereal : SeFlagsTropical;
        var radixSunLong = SEWrapper.CalculateFactorPosition(birthJd, SeSun, flags)?.MainPos ?? 0.0;
        var estimatedJd  = birthJd + age * TropicalYearInDays;
        var solarJd      = FindSolarReturnJd(estimatedJd, radixSunLong, flags);
        var solarSunLong = SEWrapper.CalculateFactorPosition(solarJd, SeSun, flags)?.MainPos ?? 0.0;

        var (positions, solarFullChart) = CalculatePositions(solarJd, geoLon, geoLat, userConfig, useSidereal);

        var dt       = SEWrapper.DateFromJulianDay(solarJd, true);
        var dateText = $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2} " +
                       $"{dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{dt.Time.Second:D2} UT";

        return new Result(solarJd, dateText, positions, solarFullChart, radixSunLong, solarSunLong);
    }

    // MARK: - Binary search

    private static double FindSolarReturnJd(
        double estimated,
        double targetLongitude,
        int    flags)
    {
        var startLong   = SEWrapper.CalculateFactorPosition(estimated, SeSun, flags)?.MainPos ?? 0.0;
        var angularDiff = targetLongitude - startLong;
        if (angularDiff >  180) angularDiff -= 360;
        if (angularDiff < -180) angularDiff += 360;

        double jdLow, jdHigh;
        if (angularDiff < 0) { jdHigh = estimated; jdLow = estimated - Math.Abs(angularDiff); }
        else                 { jdLow  = estimated; jdHigh = estimated + Math.Abs(angularDiff); }

        var counter = 0;
        var delta   = double.MaxValue;
        var tempJd  = estimated;

        do
        {
            if (++counter > MaxIterations) { Log.Warning("SolarReturnOrchestrator: max iterations reached"); break; }
            if (jdHigh - jdLow < MinInterval) break;

            tempJd = (jdLow + jdHigh) / 2.0;
            var trialLong = SEWrapper.CalculateFactorPosition(tempJd, SeSun, flags)?.MainPos ?? 0.0;
            var diff = targetLongitude - trialLong;
            if (diff >  180) diff -= 360;
            if (diff < -180) diff += 360;
            delta = Math.Abs(diff);

            if (diff < 0) jdHigh = tempJd;
            else          jdLow  = tempJd;
        }
        while (delta > TargetPrecision);

        return tempJd;
    }

    // MARK: - Full chart positions at the solar return

    private static (Dictionary<Factors, ProgressivePosition> Positions, FullChart FullChart) CalculatePositions(
        double            solarJd,
        double            geoLon,
        double            geoLat,
        UserConfiguration userConfig,
        bool              useSidereal)
    {
        var calcConfig = useSidereal
            ? userConfig.CalculationConfig with { Ayanamsha = Ayanamshas.Fagan }
            : userConfig.CalculationConfig;

        var factors = userConfig.FactorConfig.Settings
            .Where(f => f.IsUsed)
            .Select(f => f.Factor)
            .ToList();

        if (factors.Count == 0)
            factors = [Factors.Sun, Factors.Moon, Factors.Mercury, Factors.Venus, Factors.Mars,
                       Factors.Jupiter, Factors.Saturn, Factors.Uranus, Factors.Neptune, Factors.Pluto];

        var houseSystem = (int)userConfig.CalculationConfig.HouseSystem.SeId();
        var request = new CalcRequest(
            julianDay:    solarJd,
            factorsToUse: factors,
            houseSystem:  houseSystem,
            seFlags:      0,
            latitude:     geoLat,
            longitude:    geoLon,
            height:       0.0,
            configData:   calcConfig);

        var fullChart = AstronCalcOrchestrator.PerformCalculation(request);

        var results = new Dictionary<Factors, ProgressivePosition>();
        foreach (var (factor, position) in fullChart.Coordinates)
        {
            var longitude   = position.Ecliptical.Length > 0 ? position.Ecliptical[0].MainPos   : 0.0;
            var declination = position.Equatorial.Length > 0 ? position.Equatorial[0].Deviation : 0.0;
            results[factor] = new ProgressivePosition(longitude, declination);
        }
        return (results, fullChart);
    }
}
