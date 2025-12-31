using System;
using System.Globalization;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Shared.Conversion;

/// <summary>Conversion utilities for position values in degrees.</summary>
public static class PositionInDegreesConversion
{
    /// <summary>Converts a double to a formatted string with degrees, minutes, and seconds.</summary>
    /// <remarks>Format: "degrees°minutes'seconds\"" with zero-padding for minutes/seconds &lt; 10.</remarks>
    /// <param name="value">The double value in degrees</param>
    /// <returns>Formatted string (e.g., "45°30'15\"")</returns>
    public static string DoubleToDms(double value)
    {
        var totalSeconds = (int)(Math.Abs(value) * 3600);
        var degrees = totalSeconds / 3600;
        var minutes = totalSeconds % 3600 / 60;
        var seconds = totalSeconds % 60;
        
        var sign = value < 0 ? "-" : "";
        var minutesStr = minutes.ToString("00");
        var secondsStr = seconds.ToString("00");
        
        return $"{sign}{degrees}°{minutesStr}'{secondsStr}\"";
    }
    
    /// <summary>Converts a double to a formatted string with degrees, minutes, seconds, and zodiac sign.</summary>
    /// <remarks>Format: "degrees°minutes'seconds\"" where degrees are within the sign (0-29).</remarks>
    /// <param name="value">The double value in degrees (must be >= 0.0 and &lt; 360.0)</param>
    /// <returns>A tuple containing the DMS string, the Signs enum, and a boolean indicating success (false if value &lt; 0.0 or >= 360.0)</returns>
    public static (string DmsString, Signs? Sign, bool Success) DoubleToDmsSign(double value)
    {
        if (value is < 0.0 or >= 360.0)
        {
            return ("", null, false);
        }
        
        // Determine the sign
        var signIndex = (int)(value / 30.0);
        var sign = signIndex switch
        {
            0 => Signs.Aries,
            1 => Signs.Taurus,
            2 => Signs.Gemini,
            3 => Signs.Cancer,
            4 => Signs.Leo,
            5 => Signs.Virgo,
            6 => Signs.Libra,
            7 => Signs.Scorpio,
            8 => Signs.Sagittarius,
            9 => Signs.Capricorn,
            10 => Signs.Aquarius,
            11 => Signs.Pisces,
            _ => Signs.Pisces // Should not happen, but handle edge case
        };
        
        // Calculate degrees within the sign (0-29.999...)
        var degreesInSign = value % 30.0;
        
        // Convert to DMS format
        var totalSeconds = (int)(Math.Abs(degreesInSign) * 3600);
        var degrees = totalSeconds / 3600;
        var minutes = totalSeconds % 3600 / 60;
        var seconds = totalSeconds % 60;
        
        var minutesStr = minutes.ToString("00");
        var secondsStr = seconds.ToString("00");
        
        var dmsString = $"{degrees}°{minutesStr}'{secondsStr}\"";
        return (dmsString, sign, true);
    }
    
    /// <summary>Converts a double to a formatted string with decimal degrees.</summary>
    /// <remarks>Format: "degrees.decimal°" with variable precision. Uses InvariantCulture to ensure consistent formatting across locales.</remarks>
    /// <param name="value">The double value in degrees</param>
    /// <param name="decimalPlaces">Number of digits after the decimal point (default: 2)</param>
    /// <returns>Formatted string (e.g., "45.50°")</returns>
    public static string DoubleToDegrees(double value, int decimalPlaces = 2)
    {
        var formattedValue = Math.Abs(value).ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
        var sign = value < 0 ? "-" : "";
        return $"{sign}{formattedValue}°";
    }
}
