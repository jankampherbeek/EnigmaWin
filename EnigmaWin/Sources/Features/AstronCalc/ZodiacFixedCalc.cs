// ZodiacFixedCalc.cs
// EnigmaWin
// Created by porting from ZodiacFixedCalc.swift on 27-01-2026

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.AstronCalc;

/// <summary>
/// Calculate the coordinates for predefined points on the ecliptic (zodiac-fixed points).
/// Currently supports Zero Aries.
/// </summary>
public static class ZodiacFixedCalc
{
    /// <summary>
    /// Calculate the coordinates for predefined points on the ecliptic.
    /// </summary>
    /// <param name="calcRequest">Request containing calculation parameters.</param>
    /// <param name="obliquity">Obliquity of the ecliptic in degrees.</param>
    /// <param name="seWrapper">Wrapper for Swiss Ephemeris functions.</param>
    /// <returns>Dictionary of factor positions.</returns>
    public static Dictionary<Factors, FullFactorPosition> ZodiacFixedFactors(
        CalcRequest calcRequest,
        double obliquity,
        SEWrapper seWrapper)
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        var julianDay = calcRequest.JulianDay;
        var observerLat = calcRequest.Latitude;
        var observerLong = calcRequest.Longitude;

        foreach (var factor in calcRequest.FactorsToUse)
        {
            double longitude;
            switch (factor)
            {
                case Factors.ZeroAries:
                    longitude = 0.0;
                    break;

                default:
                    // Unsupported factor in this calculator – skip
                    continue;
            }

            var fullPosition = CreateFullPositionFromLongitude(
                seWrapper,
                longitude,
                julianDay,
                observerLat,
                observerLong,
                obliquity
            );

            coordinates[factor] = fullPosition;
        }

        return coordinates;
    }

    /// <summary>
    /// Create full position from ecliptical longitude.
    /// This mirrors the Swift <c>FullPositionFromLongitude</c> helper.
    /// </summary>
    private static FullFactorPosition CreateFullPositionFromLongitude(
        SEWrapper seWrapper,
        double longitude,
        double julianDay,
        double observerLatitude,
        double observerLongitude,
        double obliquity,
        double eclipticalLatitude = 0.0)
    {
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

