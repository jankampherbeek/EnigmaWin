// GeoCoordinateConversion.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Features.ImportExport.Shared;

/// <summary>
/// Shared degrees/minutes/seconds to decimal-degrees conversion for the exchange-format parsers (QCK, AAF'97).
/// Each format has its own textual coordinate syntax, but they all resolve to this same arithmetic and to the
/// ranges used by the Enigma domain model: decimal degrees, West and South negative.
/// </summary>
public static class GeoCoordinateConversion
{
    /// <summary>Converts degrees/minutes/seconds into decimal degrees.</summary>
    /// <param name="degrees">Degrees.</param>
    /// <param name="minutes">Minutes.</param>
    /// <param name="seconds">Seconds.</param>
    /// <param name="isPositiveDirection">True for North/East, false for South/West.</param>
    public static double DecimalDegrees(int degrees, int minutes, int seconds, bool isPositiveDirection)
    {
        var magnitude = degrees + minutes / 60.0 + seconds / 3600.0;
        return isPositiveDirection ? magnitude : -magnitude;
    }

    public static bool IsValidLongitude(int degrees, int minutes, int seconds) =>
        degrees is >= 0 and <= 180 && minutes is >= 0 and <= 59 && seconds is >= 0 and <= 59;

    public static bool IsValidLatitude(int degrees, int minutes, int seconds) =>
        degrees is >= 0 and <= 90 && minutes is >= 0 and <= 59 && seconds is >= 0 and <= 59;

    /// <summary>
    /// Splits a decimal-degree value into non-negative degrees/minutes/seconds plus a direction flag,
    /// for writers that emit DMS text. Seconds are rounded, with carry into minutes and degrees.
    /// </summary>
    public static (int Degrees, int Minutes, int Seconds, bool IsPositiveDirection) Components(double value)
    {
        var isPositiveDirection = value >= 0;
        var magnitude = Math.Abs(value);
        var degrees = (int)magnitude;
        var minutesDecimal = (magnitude - degrees) * 60.0;
        var minutes = (int)minutesDecimal;
        var seconds = (int)Math.Round((minutesDecimal - minutes) * 60.0, MidpointRounding.AwayFromZero);
        if (seconds >= 60) { seconds -= 60; minutes += 1; }
        if (minutes >= 60) { minutes -= 60; degrees += 1; }
        return (degrees, minutes, seconds, isPositiveDirection);
    }
}
