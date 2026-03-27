// LotsCalc.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Conversion;

namespace EnigmaWin.Sources.Features.AstronCalc;

/// <summary>
/// Calculate Hellenistic lots (e.g., Pars Fortuna).
/// For now only the calculation of Pars Fortunae (with and without sect).
/// Should augment this to include the standard Greek lots.
/// </summary>
public static class LotsCalc
{
    /// <summary>
    /// Calculate lots factors.
    /// </summary>
    /// <param name="seWrapper">Wrapper for Swiss Ephemeris functions.</param>
    /// <param name="calcRequest">Request containing calculation parameters.</param>
    /// <param name="obliquity">Obliquity of the ecliptic in degrees.</param>
    /// <param name="ascendantLongitude">The ecliptical longitude of the Ascendant in degrees.</param>
    /// <param name="sunLongitude">The ecliptical longitude of the Sun in degrees.</param>
    /// <param name="moonLongitude">The ecliptical longitude of the Moon in degrees.</param>
    /// <param name="isDayChart">True if the Sun is above the horizon (day chart), false if below (night chart).</param>
    /// <param name="ayanamshaOffset">Offset for ayanamsha to obtain sidereal longitude.</param>
    /// <returns>Dictionary of factor positions.</returns>
    public static Dictionary<Factors, FullFactorPosition> CalculateLotsFactors(
        SEWrapper seWrapper,
        CalcRequest calcRequest,
        double obliquity,
        double ascendantLongitude,
        double sunLongitude,
        double moonLongitude,
        bool isDayChart,
        double ayanamshaOffset)
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>();

        foreach (var factor in calcRequest.FactorsToUse)
        {
            switch (factor)
            {
                case Factors.FortunaSect:
                {
                    // Calculate Pars Fortuna with sect
                    // Formula: With sect (only if night chart): Ascendant + Moon - Sun
                    //          Otherwise: Ascendant + Sun - Moon
                    double parsFortunaLongitude;
                    if (calcRequest.ConfigData.LotsType == LotsTypes.Sect && !isDayChart)
                    {
                        // With sect: Ascendant + Moon - Sun
                        parsFortunaLongitude = RangeUtil.ValueToRange(
                            ascendantLongitude + moonLongitude - sunLongitude,
                            0.0,
                            360.0
                        );
                    }
                    else
                    {
                        // Without sect or any day chart: Ascendant + Sun - Moon
                        parsFortunaLongitude = RangeUtil.ValueToRange(
                            ascendantLongitude + sunLongitude - moonLongitude,
                            0.0,
                            360.0
                        );
                    }

                    // Create full position from calculated longitude
                    var fullPosition = CreateFullPositionFromLongitude(
                        seWrapper,
                        parsFortunaLongitude,
                        calcRequest.JulianDay,
                        calcRequest.Latitude,
                        calcRequest.Longitude,
                        obliquity,
                        ayanamshaOffset
                    );

                    coordinates[factor] = fullPosition;
                    break;
                }

                case Factors.FortunaNoSect:
                {
                    // Calculate Pars Fortuna without sect
                    // Formula: Ascendant + Sun - Moon (always, for day and night chart)
                    var parsFortunaLongitude = RangeUtil.ValueToRange(
                        ascendantLongitude + sunLongitude - moonLongitude,
                        0.0,
                        360.0
                    );

                    // Create full position from calculated longitude
                    var fullPosition = CreateFullPositionFromLongitude(
                        seWrapper,
                        parsFortunaLongitude,
                        calcRequest.JulianDay,
                        calcRequest.Latitude,
                        calcRequest.Longitude,
                        obliquity,
                        ayanamshaOffset
                    );

                    coordinates[factor] = fullPosition;
                    break;
                }
            }
        }

        return coordinates;
    }

    /// <summary>
    /// Create full position from ecliptical longitude.
    /// </summary>
    /// <param name="seWrapper">Wrapper for Swiss Ephemeris functions.</param>
    /// <param name="longitude">Ecliptical longitude in degrees.</param>
    /// <param name="julianDay">Julian day for UT.</param>
    /// <param name="latitude">Observer latitude.</param>
    /// <param name="observerLongitude">Observer longitude (for horizontal coordinates).</param>
    /// <param name="obliquity">Obliquity of the ecliptic.</param>
    /// <param name="ayanamshaOffset">Offset for ayanamsha to obtain sidereal longitude.</param>
    /// <returns>FullFactorPosition with all coordinate systems.</returns>
    private static FullFactorPosition CreateFullPositionFromLongitude(
        SEWrapper seWrapper,
        double longitude,
        double julianDay,
        double latitude,
        double observerLongitude,
        double obliquity,
        double ayanamshaOffset)
    {
        // Ecliptical position (latitude is 0 for lots)
        var eclipticalPos = new MainAstronomicalPosition(
            MainPos: RangeUtil.ValueToRange(longitude - ayanamshaOffset, 0.0, 360.0),
            Deviation: 0.0,
            Distance: 0.0,
            MainPosSpeed: 0.0,
            DeviationSpeed: 0.0,
            DistanceSpeed: 0.0
        );

        // Convert to equatorial coordinates
        var (ra, decl) = seWrapper.EclipticToEquatorial(
            [longitude, 0.0],
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
            latitude,
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
