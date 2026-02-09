using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWin.Sources.Features.Shared.Validation;

/// <summary>Validate astronomical dates and times.</summary>
public static class AstronomicalDateValidation
{
    /// <summary>Validates an AstronomicalDate by checking if it can be converted to a Julian Day and back.</summary>
    /// <remarks>Uses the SEWrapper to perform the conversion and verify consistency.</remarks>
    /// <param name="date">The AstronomicalDate to validate</param>
    /// <returns>True if the date is valid (can be converted to Julian Day and back without change), false otherwise</returns>
    public static bool ValidateDate(AstronomicalDate date)
    {
        // Create a time at midnight (0:00:00) for date-only validation
        var midnightTime = new AstronomicalTime(0, 0, 0);
        
        // Convert the date to Julian Day
        var julianDay = SEWrapper.JulianDay(date, midnightTime);
        
        // Convert back from Julian Day to date
        var convertedDateTime = SEWrapper.DateFromJulianDay(julianDay, date.Gregorian);
        
        // Check if the converted date matches the original
        return convertedDateTime.Date.Year == date.Year &&
               convertedDateTime.Date.Month == date.Month &&
               convertedDateTime.Date.Day == date.Day &&
               convertedDateTime.Date.Gregorian == date.Gregorian;
    }
    
    /// <summary>Validates an AstronomicalTime using a 24-hour clock.</summary>
    /// <param name="time">The AstronomicalTime to validate</param>
    /// <returns>True if the time is valid (hour 0-23, minute 0-59, second 0-59), false otherwise</returns>
    public static bool ValidateTime(AstronomicalTime time)
    {
        return time.Hour is >= 0 and <= 23 &&
               time.Minute is >= 0 and <= 59 &&
               time.Second is >= 0 and <= 59;
    }
}

