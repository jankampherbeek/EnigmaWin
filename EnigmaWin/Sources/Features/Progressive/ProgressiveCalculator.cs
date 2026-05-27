// ProgressiveCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Progressive;

/// <summary>
/// Calculates geocentric longitude and declination for a set of factors at a given Julian Day.
/// Shared astronomical core for transits and secondary directions.
/// Always geocentric — topocentric override is silently replaced with geocentric.
/// Oblique-longitude projection is also disabled (not applicable to progressives).
/// </summary>
public static class ProgressiveCalculator
{
    public static Dictionary<Factors, ProgressivePosition> PerformCalculation(
        ProgressiveCalcRequest request,
        CalculationConfig calculationConfig,
        IReadOnlyList<Factors> factors)
    {
        var effectiveObserverPosition =
            calculationConfig.ObserverPosition == ObserverPositions.Topocentric
                ? ObserverPositions.Geocentric
                : calculationConfig.ObserverPosition;

        var effectiveConfig = calculationConfig with
        {
            HouseSystem      = HouseSystems.NoHouses,
            ObserverPosition = effectiveObserverPosition,
            ProjectionType   = ProjectionTypes.TwoDimensional
        };

        var calcRequest = new CalcRequest(
            julianDay:    request.JulianDay,
            factorsToUse: new List<Factors>(factors),
            houseSystem:  (int)HouseSystems.NoHouses.SeId(),
            seFlags:      0,
            latitude:     0.0,
            longitude:    0.0,
            height:       0.0,
            configData:   effectiveConfig);

        var fullChart = AstronCalcOrchestrator.PerformCalculation(calcRequest);

        var results = new Dictionary<Factors, ProgressivePosition>();
        foreach (var (factor, position) in fullChart.Coordinates)
        {
            var longitude   = position.Ecliptical.Length > 0 ? position.Ecliptical[0].MainPos   : 0.0;
            var declination = position.Equatorial.Length > 0 ? position.Equatorial[0].Deviation : 0.0;
            results[factor] = new ProgressivePosition(longitude, declination);
        }
        return results;
    }
}
