// FormulaCalc.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;

namespace EnigmaWin.Sources.Features.AstronCalc;

/// <summary>
/// Calculate geocentric ecliptical positions for celestial points that are not supported by the Swiss Ephemeris,
/// using specific formulas.
/// </summary>
public static class FormulaCalc
{
    /// <summary>Julian day for 1900/1/1 0:00:00 UT.</summary>
    private const double Jd1900 = 2415020.5;

    /// <summary>Tropical year in days (365.242190 days).</summary>
    private const double TropicalYearInDays = 365.242190;

    /// <summary>
    /// Calculate the positions for factors that are handled by formulas instead of directly by Swiss Ephemeris.
    /// Latitude is unknown, so latitude, RA, declination, azimuth and altitude are set to zero.
    /// </summary>
    /// <param name="seWrapper">Wrapper for Swiss Ephemeris (used for some formulas).</param>
    /// <param name="calcRequest">Request with Julian day and factors to use.</param>
    /// <param name="ayanamshaOffset">Offset for ayanamsha to obtain sidereal longitude.</param>
    /// <returns>Dictionary with calculated positions per factor.</returns>
    public static Dictionary<Factors, FullFactorPosition> CalculateFormulaFactors(
        SEWrapper seWrapper,
        CalcRequest calcRequest,
        double ayanamshaOffset)
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>();

        var julianDay = calcRequest.JulianDay;
        var julianDayPrevious = julianDay - 0.5;
        var julianDayNext = julianDay + 0.5;
        const double distance = 0.0;   // distance is unknown or irrelevant
        const double latitude = 0.0;   // latitude is unknown

        var zeroEqCoordinate = new MainAstronomicalPosition(
            MainPos: 0.0,
            Deviation: 0.0,
            Distance: 0.0,
            MainPosSpeed: 0.0,
            DeviationSpeed: 0.0,
            DistanceSpeed: 0.0);

        var zeroHorizontalCoordinate = new HorizontalPosition(
            Azimuth: 0.0,
            Altitude: 0.0);

        foreach (var factor in calcRequest.FactorsToUse)
        {
            double longitude;
            double longitudePrevious;
            double longitudeNext;

            switch (factor)
            {
                case Factors.PersephoneCarteret:
                    longitude = CalcCarteretHypPlanet(julianDay, 212.0, 1.0);
                    longitudePrevious = CalcCarteretHypPlanet(julianDayPrevious, 212.0, 1.0);
                    longitudeNext = CalcCarteretHypPlanet(julianDayNext, 212.0, 1.0);
                    break;

                case Factors.VulcanusCarteret:
                    longitude = CalcCarteretHypPlanet(julianDay, 15.7, 0.55);
                    longitudePrevious = CalcCarteretHypPlanet(julianDayPrevious, 15.7, 0.55);
                    longitudeNext = CalcCarteretHypPlanet(julianDayNext, 15.7, 0.55);
                    break;

                case Factors.ApogeeDuval:
                    var apogeeCalc = new ApogeeDuvalCalc();
                    longitude = apogeeCalc.CalcApogeeDuval(julianDay, seWrapper);
                    longitudePrevious = apogeeCalc.CalcApogeeDuval(julianDayPrevious, seWrapper);
                    longitudeNext = apogeeCalc.CalcApogeeDuval(julianDayNext, seWrapper);
                    break;

                default:
                    // Unsupported factor for formula calculation; skip
                    continue;
            }

            var longitudeSpeed = longitudeNext - longitudePrevious;

            var eclipticalPos = new MainAstronomicalPosition(
                MainPos: RangeUtil.ValueToRange(longitude - ayanamshaOffset, 0.0, 360.0),
                Deviation: latitude,
                Distance: distance,
                MainPosSpeed: longitudeSpeed,
                DeviationSpeed: 0.0,
                DistanceSpeed: 0.0
            );

            var fullPosition = new FullFactorPosition(
                Ecliptical: [eclipticalPos],
                Equatorial: [zeroEqCoordinate],
                Horizontal: [zeroHorizontalCoordinate]
            );

            coordinates[factor] = fullPosition;
        }

        return coordinates;
    }


    /// <summary>
    /// Calculate Carteret hypothetical planet position.
    /// </summary>
    /// <param name="julianDay">Julian day for UT.</param>
    /// <param name="startPoint">Starting point in degrees.</param>
    /// <param name="yearlySpeed">Yearly speed in degrees per year.</param>
    /// <returns>Longitude in degrees (0-360).</returns>
    private static double CalcCarteretHypPlanet(double julianDay, double startPoint, double yearlySpeed)
    {
        var positions = startPoint +
                        (julianDay - Jd1900) * (yearlySpeed / TropicalYearInDays);
        return RangeUtil.ValueToRange(positions, 0.0, 360.0);
    }
}

