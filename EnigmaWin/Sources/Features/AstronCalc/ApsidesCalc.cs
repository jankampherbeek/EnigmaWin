// ApsidesCalc.cs
// EnigmaWin
// Created by porting from ApsidesCalc.swift on 27-01-2026

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.AstronCalc;

/// <summary>
/// Calculate apsides (perihelion/aphelion for planets, perigee/apogee for Moon) using Swiss Ephemeris.
/// Currently supports Black Sun and Diamond.
/// </summary>
public static class ApsidesCalc
{
    /// <summary>
    /// Calculate apsides factors.
    /// </summary>
    /// <param name="calcRequest">The CalcRequest containing calculation parameters.</param>
    /// <param name="obliquity">Obliquity of the ecliptic in degrees.</param>
    /// <param name="ayanamshaOffset">Ayanamsha offset in degrees.</param>
    /// <param name="flags">SE flags for ecliptical calculations.</param>
    /// <param name="seWrapper">SEWrapper instance for calculations.</param>
    /// <returns>Dictionary of factor positions.</returns>
    public static Dictionary<Factors, FullFactorPosition> CalculateApsidesFactors(
        CalcRequest calcRequest,
        double obliquity,
        double ayanamshaOffset,
        int flags,
        SEWrapper seWrapper)
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        var julianDay = calcRequest.JulianDay;

        const int method = 1; // SE_NODBIT_MEAN for mean nodes/apsides

        // Planet id for which apsides are calculated: Sun (Earth's orbit)
        var planetId = Factors.Sun.SeId();

        foreach (var factor in calcRequest.FactorsToUse)
        {
            // Calculate apsides for the given planet
            var apsidesResult = seWrapper.CalculateApsides(
                julianDay,
                planetId,
                flags,
                method
            );

            double longitude;
            double latitude;

            switch (factor)
            {
                case Factors.BlackSun:
                    // Black Sun = Earth's aphelion (farthest from Sun)
                    longitude = apsidesResult.Aphelion[0];
                    latitude = apsidesResult.Aphelion[1];
                    break;

                case Factors.Diamond:
                    // Diamond = Earth's perihelion (closest to Sun)
                    longitude = apsidesResult.Perihelion[0];
                    latitude = apsidesResult.Perihelion[1];
                    break;

                default:
                    // Unsupported factor for this calculator
                    continue;
            }

            var fullPosition = CreateFullPositionFromLongitude(
                seWrapper,
                longitude - ayanamshaOffset,
                julianDay,
                calcRequest.Latitude,
                calcRequest.Longitude,
                obliquity,
                latitude
            );

            coordinates[factor] = fullPosition;
        }

        return coordinates;
    }

    /// <summary>
    /// Create full position from ecliptical longitude and latitude.
    /// Shared helper mirroring Swift FullPositionFromLongitude.
    /// </summary>
    private static FullFactorPosition CreateFullPositionFromLongitude(
        SEWrapper seWrapper,
        double longitude,
        double julianDay,
        double observerLatitude,
        double observerLongitude,
        double obliquity,
        double eclipticalLatitude)
    {
        // Ecliptical position
        var eclipticalPos = new MainAstronomicalPosition(
            MainPos: longitude,
            Deviation: eclipticalLatitude,
            Distance: 0.0,
            MainPosSpeed: 0.0,
            DeviationSpeed: 0.0,
            DistanceSpeed: 0.0
        );

        // Convert to equatorial coordinates
        var (ra, decl) = seWrapper.EclipticToEquatorial(
            [longitude, eclipticalLatitude],
            obliquity
        );

        var equatorialPos = new MainAstronomicalPosition(
            MainPos: ra,
            Deviation: decl,
            Distance: 0.0,
            MainPosSpeed: 0.0,
            DeviationSpeed: 0.0,
            DistanceSpeed: 0.0
        );

        // Calculate horizontal position using equatorial coordinates
        var horiz = seWrapper.AzimuthAndAltitude(
            julianDay,
            ra,
            decl,
            observerLatitude,
            observerLongitude,
            0.0
        );

        var horizontalPos = new HorizontalPosition(
            Azimuth: horiz[0],
            Altitude: horiz[1]
        );

        return new FullFactorPosition(
            Ecliptical: [eclipticalPos],
            Equatorial: [equatorialPos],
            Horizontal: [horizontalPos]
        );
    }
}

