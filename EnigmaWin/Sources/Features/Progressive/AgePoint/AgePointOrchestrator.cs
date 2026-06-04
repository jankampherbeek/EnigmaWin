// AgePointOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWin.Sources.Features.Progressive.AgePoint;

/// <summary>
/// Calculates ecliptic positions for the Huber Age Point technique.
/// The Age Point starts at the Ascendant at birth and moves linearly through the
/// 12 houses in 72 years, spending exactly 6 years per house, then cycles back.
/// </summary>
public sealed class AgePointOrchestrator
{
    private const double CycleDuration = 72.0;
    private const double HouseDuration = 6.0;   // CycleDuration / 12 houses

    /// <summary>Returns the ecliptic longitude of the Age Point for a given age.</summary>
    /// <param name="age">Age in years (may exceed 72; the cycle repeats every 72 years).</param>
    /// <param name="cusps">12-element array of house-cusp longitudes (0-based: cusps[0] = house 1 = Ascendant).</param>
    /// <returns>Ecliptic longitude in degrees (0–360).</returns>
    public double AgePointLongitude(double age, double[] cusps)
    {
        if (cusps.Length < 12) return cusps.Length > 0 ? cusps[0] : 0;

        var aNorm = age % CycleDuration;
        if (aNorm < 0) aNorm += CycleDuration;

        var houseIdx = (int)(aNorm / HouseDuration);
        if (houseIdx > 11) houseIdx = 11;

        var fraction  = (aNorm - houseIdx * HouseDuration) / HouseDuration;
        var fromLong  = cusps[houseIdx];
        var toLong    = cusps[(houseIdx + 1) % 12];

        var dist = toLong - fromLong;
        if (dist < 0) dist += 360;

        var position = fromLong + fraction * dist;
        while (position >= 360) position -= 360;
        while (position < 0)   position += 360;
        return position;
    }

    /// <summary>Returns (years, months, totalYears) for the age at which the Age Point reaches the given longitude.</summary>
    /// <param name="longitude">Target ecliptic longitude (0–360).</param>
    /// <param name="cusps">12-element array of house-cusp longitudes (0-based).</param>
    /// <returns>Tuple with whole years, remaining months, and decimal total years.</returns>
    public (int Years, int Months, double TotalYears) AgeFromLongitude(double longitude, double[] cusps)
    {
        if (cusps.Length < 12) return (0, 0, 0);

        for (var houseIdx = 0; houseIdx < 12; houseIdx++)
        {
            var fromLong = cusps[houseIdx];
            var toLong   = cusps[(houseIdx + 1) % 12];
            var dist = toLong - fromLong;
            if (dist < 0) dist += 360;

            var diff = longitude - fromLong;
            if (diff < 0) diff += 360;

            if (dist > 0 && diff >= 0 && diff <= dist)
            {
                var fraction   = diff / dist;
                var totalYears = houseIdx * HouseDuration + fraction * HouseDuration;
                var wholeYears = (int)totalYears;
                var months     = (int)((totalYears - wholeYears) * 12);
                return (wholeYears, months, totalYears);
            }
        }
        return (0, 0, 0);
    }

    /// <summary>
    /// Calculates 12 house-cusp ecliptic longitudes for the given chart using the specified house system.
    /// Returns null if the calculation fails (e.g. no geographic coordinates available).
    /// </summary>
    public double[]? CalculateCusps(FullChart chart, Horoscope horoscope, HouseSystems houseSystem)
    {
        if (horoscope.Latitude is null || horoscope.Longitude is null) return null;
        try
        {
            var seWrapper = new SEWrapper();
            var houseChar = houseSystem.SeId();
            var result    = seWrapper.CalculateHouses(chart.JulianDay, 0,
                horoscope.Latitude.Value, horoscope.Longitude.Value, houseChar);
            var houseCusps = result[0];
            if (houseCusps.Length < 13) return null;
            // Swiss Ephemeris returns cusps[0]=unused, cusps[1..12]=house cusps
            var cusps = new double[12];
            for (var i = 0; i < 12; i++)
                cusps[i] = houseCusps[i + 1];
            return cusps;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Extracts the 12 house-cusp longitudes from an existing FullChart (uses the chart's current house system).</summary>
    public static double[] CuspsFromChart(FullChart chart)
    {
        var cusps = new double[12];
        for (var i = 0; i < Math.Min(12, chart.HousePositions.Cusps.Length); i++)
            cusps[i] = chart.HousePositions.Cusps[i].Longitude;
        return cusps;
    }
}
