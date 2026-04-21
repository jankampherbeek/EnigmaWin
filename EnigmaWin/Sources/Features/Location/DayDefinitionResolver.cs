// DayDefinitionResolver.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;

namespace EnigmaWin.Sources.Features.Location;

/// <summary>
/// Resolves DST/TZ day-of-month expressions to an actual calendar day.
/// Supported IANA formats: "14" (fixed), "last0" (last Sunday), "Sun>=8", "0>=8", "Sun&lt;=25", "0&lt;=25".
/// Weekday convention: Mon=0 … Sun=6.
/// </summary>
internal static class DayDefinitionResolver
{
    /// <summary>Returns the day-of-month (1-based) for the given IANA expression, or <c>null</c> if unparseable.</summary>
    public static int? ResolveIana(string expression, int year, int month)
    {
        var expr = expression.Trim();

        if (int.TryParse(expr, out var fixed_)) return fixed_;

        if (expr.StartsWith("last", StringComparison.OrdinalIgnoreCase))
        {
            var wdStr = expr[4..];
            if (WeekdayNumber(wdStr) is { } wd)
                return LastWeekday(wd, year, month);
        }

        var gtIdx = expr.IndexOf(">=", StringComparison.Ordinal);
        if (gtIdx >= 0)
        {
            var wdStr  = expr[..gtIdx];
            var dayStr = expr[(gtIdx + 2)..];
            if (int.TryParse(dayStr, out var boundary))
            {
                if (WeekdayNumber(wdStr) is { } wd)
                    return FirstWeekdayOnOrAfter(wd, boundary, year, month);
                return boundary;
            }
        }

        var ltIdx = expr.IndexOf("<=", StringComparison.Ordinal);
        if (ltIdx >= 0)
        {
            var wdStr  = expr[..ltIdx];
            var dayStr = expr[(ltIdx + 2)..];
            if (int.TryParse(dayStr, out var boundary))
            {
                if (WeekdayNumber(wdStr) is { } wd)
                    return LastWeekdayOnOrBefore(wd, boundary, year, month);
                return boundary;
            }
        }

        return null;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>Mon=0 … Sun=6, accepting names ("Mon","Sun") or digits ("0".."6").</summary>
    private static int? WeekdayNumber(string s) => s.ToLowerInvariant() switch
    {
        "mon" or "0" => 0,
        "tue" or "1" => 1,
        "wed" or "2" => 2,
        "thu" or "3" => 3,
        "fri" or "4" => 4,
        "sat" or "5" => 5,
        "sun" or "6" => 6,
        _            => int.TryParse(s, out var n) ? n : null
    };

    /// <summary>Weekday of a given date using Mon=0 … Sun=6 convention.</summary>
    private static int Weekday(int year, int month, int day)
    {
        var dt = new DateTime(year, month, day, new GregorianCalendar());
        // DayOfWeek: Sunday=0, Monday=1 … Saturday=6  →  convert to Mon=0 … Sun=6
        return ((int)dt.DayOfWeek + 6) % 7;
    }

    private static int DaysInMonth(int year, int month) =>
        DateTime.DaysInMonth(year, month);

    private static int LastWeekday(int targetWd, int year, int month)
    {
        for (var d = DaysInMonth(year, month); d >= 1; d--)
            if (Weekday(year, month, d) == targetWd) return d;
        return DaysInMonth(year, month);
    }

    private static int FirstWeekdayOnOrAfter(int targetWd, int boundary, int year, int month)
    {
        var last = DaysInMonth(year, month);
        for (var d = boundary; d <= last; d++)
            if (Weekday(year, month, d) == targetWd) return d;
        return boundary;
    }

    private static int LastWeekdayOnOrBefore(int targetWd, int boundary, int year, int month)
    {
        var cap = Math.Min(boundary, DaysInMonth(year, month));
        for (var d = cap; d >= 1; d--)
            if (Weekday(year, month, d) == targetWd) return d;
        return boundary;
    }
}
