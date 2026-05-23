// DateTimeConversion.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Shared.Conversion;

/// <summary>Conversion utilities for AstronomicalDate, AstronomicalTime, and AstronomicalDateTime.</summary>
public static class DateTimeConversion
{
    /// <summary>Converts an AstronomicalDate to a formatted string.</summary>
    /// <remarks>Format: "yyyy/mm/dd G" or "yyyy/mm/dd J" with leading zeros for day/month &lt; 10.</remarks>
    /// <param name="date">The AstronomicalDate to convert</param>
    /// <returns>Formatted string (e.g., "2025/01/15 G" or "2025/01/15 J")</returns>
    public static string DateToText(AstronomicalDate date)
    {
        var year = date.Year.ToString();
        var month = date.Month.ToString("00");
        var day = date.Day.ToString("00");
        var calendar = date.Gregorian ? "G" : "J";
        
        return $"{year}/{month}/{day} {calendar}";
    }
    
    /// <summary>Converts an AstronomicalTime to a formatted string.</summary>
    /// <remarks>Format: "hh:mm:ss" with leading zeros for minutes/seconds &lt; 10.</remarks>
    /// <param name="time">The AstronomicalTime to convert</param>
    /// <returns>Formatted string (e.g., "14:30:45")</returns>
    public static string TimeToText(AstronomicalTime time)
    {
        var hour = time.Hour.ToString("00");
        var minute = time.Minute.ToString("00");
        var second = time.Second.ToString("00");
        
        return $"{hour}:{minute}:{second}";
    }
    
    /// <summary>Converts an AstronomicalDateTime to a formatted string.</summary>
    /// <remarks>Combines DateToText and TimeToText separated by a space. Format: "yyyy/mm/dd G hh:mm:ss" or "yyyy/mm/dd J hh:mm:ss"</remarks>
    /// <param name="dateTime">The AstronomicalDateTime to convert</param>
    /// <returns>Formatted string (e.g., "2025/01/15 G 14:30:45")</returns>
    public static string DateTimeToText(AstronomicalDateTime dateTime)
    {
        var dateText = DateToText(dateTime.Date);
        var timeText = TimeToText(dateTime.Time);
        
        return $"{dateText} {timeText}";
    }
}
