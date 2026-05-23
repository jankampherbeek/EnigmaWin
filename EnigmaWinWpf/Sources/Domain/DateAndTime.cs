// DateAndTime.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>Date for astronomical calculations.</summary>
/// <remarks>
/// Assumes the astronomical calendar, so there is a year zero and years BCE should subtract 1 to get the astronomical date.
/// Gregorian indicates the calendar: true (default) is Gregorian calendar, otherwise the Julian calendar is used.
/// </remarks>
/// <param name="Year">The year</param>
/// <param name="Month">The month (1-12)</param>
/// <param name="Day">The day</param>
/// <param name="Gregorian">True for Gregorian calendar (default), false for Julian calendar</param>
public record AstronomicalDate(
    int Year,
    int Month,
    int Day,
    bool Gregorian = true);

/// <summary>Time for all purposes, including astronomical. No timezone is supplied, for astronomical calculations always use UT.</summary>
public record AstronomicalTime
{
    public int Hour { get; init; }
    public int Minute { get; init; }
    public int Second { get; init; }
    public double HourDecimal { get; init; }

    /// <summary>Create AstronomicalTime from hour, minute, and second.</summary>
    /// <param name="hour">The hour</param>
    /// <param name="minute">The minute</param>
    /// <param name="second">The second</param>
    public AstronomicalTime(int hour, int minute, int second)
    {
        Hour = hour;
        Minute = minute;
        Second = second;
        HourDecimal = hour + minute / 60.0 + second / 3600.0;
    }

    /// <summary>Create AstronomicalTime from decimal hours.</summary>
    /// <param name="hourDecimal">The time as decimal hours</param>
    public AstronomicalTime(double hourDecimal)
    {
        HourDecimal = hourDecimal;
        var hour = (int)hourDecimal;
        var minute = (int)((hourDecimal - hour) * 60.0);
        var second = (int)Math.Round(((hourDecimal - hour) * 60.0 - minute) * 60.0);
        Hour = hour;
        Minute = minute;
        Second = second;
    }
}

/// <summary>Astronomical date and time.</summary>
/// <param name="Date">The astronomical date</param>
/// <param name="Time">The astronomical time</param>
public record AstronomicalDateTime(
    AstronomicalDate Date,
    AstronomicalTime Time);

