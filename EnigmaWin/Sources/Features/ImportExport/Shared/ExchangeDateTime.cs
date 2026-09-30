// ExchangeDateTime.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWin.Sources.Features.ImportExport.Shared;

/// <summary>
/// Date/time conversions shared by the exchange formats. The formats supply an explicit UTC offset rather
/// than an IANA zone, so the local clock numbers are read as if they were UT to get a Julian Day on the local
/// scale, and then shifted by the offset (expressed in days) to reach true UT.
/// </summary>
public static class ExchangeDateTime
{
    /// <summary>Converts a local calendar date/time plus a known UTC offset into a Julian Day in UT.</summary>
    public static double LocalToUtJulianDay(AstronomicalDate localDate, AstronomicalTime localTime, int offsetSeconds)
    {
        var localJd = SEWrapper.JulianDay(localDate, localTime);
        return localJd - offsetSeconds / 86400.0;
    }

    /// <summary>
    /// Converts a Julian Day (UT) into a Gregorian date with whole hours, minutes and seconds. The Julian Day
    /// is rounded to the nearest second first, so the result never contains a second or minute value of 60.
    /// </summary>
    public static AstronomicalDateTime UtFromJulianDay(double julianDay)
    {
        var roundedJd = Math.Round(julianDay * 86400.0) / 86400.0;
        var dateTime = SEWrapper.DateFromJulianDay(roundedJd, gregorian: true);
        var totalSeconds = (int)Math.Round(dateTime.Time.HourDecimal * 3600.0);
        totalSeconds = Math.Clamp(totalSeconds, 0, 86399);
        var time = new AstronomicalTime(totalSeconds / 3600, totalSeconds % 3600 / 60, totalSeconds % 60);
        return new AstronomicalDateTime(dateTime.Date, time);
    }

    /// <summary>
    /// True when the date lies before the historical Gregorian calendar cutover (1582-10-15). Used by formats
    /// that leave the calendar implicit (QCK) or default to Julian before the cutover (AAF'97).
    /// </summary>
    public static bool IsBeforeGregorianCutover(int year, int month, int day)
    {
        if (year != 1582) return year < 1582;
        if (month != 10) return month < 10;
        return day < 15;
    }
}
