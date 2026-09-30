// AafFieldParsers.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Globalization;
using EnigmaWin.Sources.Features.ImportExport.Qck;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWin.Sources.Features.ImportExport.Aaf;

/// <summary>
/// Field-level parsers for the AAF'97 #A93/#B93 chunks. Each parser is independently testable and reports
/// its own warnings and fatal errors.
/// </summary>
public static class AafFieldParsers
{
    public readonly record struct DateResult(int Day, int Month, int Year, bool IsGregorian);

    public readonly record struct TimeResult(int Hour, int Minute, int Second);

    /// <summary>
    /// Parses "day.month.year[g|j]", e.g. "29.1.1953", "24.12.-6g", "7.10.1582j". BCE years already use
    /// astronomical year numbering. Without a calendar suffix, AAF's own fallback rule applies: before the
    /// Gregorian cutover (1582-10-15) defaults to Julian, on/after defaults to Gregorian.
    /// </summary>
    public static DateResult? ParseDate(string raw, int recordNumber, List<ExchangeMessage> messages)
    {
        var parts = raw.Split('.');
        if (parts.Length != 3 || !TryParseInt(parts[0], out var day) || !TryParseInt(parts[1], out var month))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafInvalidDate, recordNumber, "date", raw));
            return null;
        }

        var yearText = parts[2];
        char? suffix = null;
        if (yearText.Length > 0 && char.ToLowerInvariant(yearText[^1]) is 'g' or 'j')
        {
            suffix = char.ToLowerInvariant(yearText[^1]);
            yearText = yearText[..^1];
        }
        if (!TryParseInt(yearText, out var year))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafInvalidYear, recordNumber, "date", parts[2]));
            return null;
        }

        var isGregorian = suffix.HasValue
            ? suffix.Value == 'g'
            : !ExchangeDateTime.IsBeforeGregorianCutover(year, month, day);

        if (!QckLineParser.IsValidDate(year, month, day, isGregorian))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgInvalidDate, recordNumber, "date", raw));
            return null;
        }
        return new DateResult(day, month, year, isGregorian);
    }

    /// <summary>Parses civil time: "16:55", "22:30:12", "19", "22h30:12". No AM/PM.</summary>
    public static TimeResult? ParseTime(string raw, int recordNumber, List<ExchangeMessage> messages)
    {
        var parts = raw.Replace('h', ':').Replace('H', ':').Split(':');
        var minute = 0;
        var second = 0;
        if (parts.Length is < 1 or > 3
            || !TryParseInt(parts[0], out var hour)
            || (parts.Length >= 2 && !TryParseInt(parts[1], out minute))
            || (parts.Length >= 3 && !TryParseInt(parts[2], out second)))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafInvalidTime, recordNumber, "time", raw));
            return null;
        }
        if (hour is < 0 or > 23 || minute is < 0 or > 59 || second is < 0 or > 59)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafTimeRange, recordNumber, "time", raw));
            return null;
        }
        return new TimeResult(hour, minute, second);
    }

    /// <summary>
    /// Parses a latitude "DDnMM[:SS]"/"DDsMM[:SS]" or longitude "DDDeMM[:SS]"/"DDDwMM[:SS]".
    /// <paramref name="positiveChar"/>/<paramref name="negativeChar"/> are lowercase direction letters.
    /// </summary>
    public static double? ParseCoordinate(string raw, char positiveChar, char negativeChar, bool isLongitude,
        int recordNumber, string field, List<ExchangeMessage> messages)
    {
        var lower = raw.ToLowerInvariant();
        var directionIndex = lower.IndexOfAny([positiveChar, negativeChar]);
        if (directionIndex < 0)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafInvalidCoordinate, recordNumber, field, raw));
            return null;
        }
        var degreesText = lower[..directionIndex];
        var isPositive = lower[directionIndex] == positiveChar;
        var rest = lower[(directionIndex + 1)..];

        var secondsText = "0";
        var colonIndex = rest.IndexOf(':');
        if (colonIndex >= 0)
        {
            secondsText = rest[(colonIndex + 1)..];
            rest = rest[..colonIndex];
        }

        if (!TryParseInt(degreesText, out var degrees) || !TryParseInt(rest, out var minutes)
            || !TryParseInt(secondsText, out var seconds))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafInvalidCoordinate, recordNumber, field, raw));
            return null;
        }

        var isValid = isLongitude
            ? GeoCoordinateConversion.IsValidLongitude(degrees, minutes, seconds)
            : GeoCoordinateConversion.IsValidLatitude(degrees, minutes, seconds);
        if (!isValid)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafCoordinateRange, recordNumber, field, raw));
            return null;
        }
        return GeoCoordinateConversion.DecimalDegrees(degrees, minutes, seconds, isPositive);
    }

    /// <summary>
    /// Parses the Greenwich offset field: "1he", "5hw", "5he30", "0he32:06", or "*" (unknown). East is a
    /// positive modern UTC offset, no sign inversion (unlike QCK).
    /// </summary>
    /// <returns><c>Success</c> false on a fatal error; otherwise <c>Seconds</c> is the offset, or null for "*".</returns>
    public static (bool Success, int? Seconds) ParseGreenwichOffset(string raw, int recordNumber, List<ExchangeMessage> messages)
    {
        var trimmed = raw.Trim();
        if (trimmed == "*") return (true, null);

        var lower = trimmed.ToLowerInvariant();
        var hIndex = lower.IndexOf('h');
        if (hIndex < 0)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafInvalidOffset, recordNumber, "greenwichOffset", raw));
            return (false, null);
        }
        var hoursText = lower[..hIndex];
        var afterH = lower[(hIndex + 1)..];
        if (afterH.Length == 0 || afterH[0] is not ('e' or 'w'))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafInvalidOffsetDirection, recordNumber, "greenwichOffset", raw));
            return (false, null);
        }
        var direction = afterH[0];
        var rest = afterH[1..];

        var secondsText = "0";
        var colonIndex = rest.IndexOf(':');
        if (colonIndex >= 0)
        {
            secondsText = rest[(colonIndex + 1)..];
            rest = rest[..colonIndex];
        }
        var minutesText = rest.Length == 0 ? "0" : rest;

        if (!TryParseInt(hoursText, out var hours) || !TryParseInt(minutesText, out var minutes)
            || !TryParseInt(secondsText, out var seconds))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafInvalidOffset, recordNumber, "greenwichOffset", raw));
            return (false, null);
        }
        var magnitude = hours * 3600 + minutes * 60 + seconds;
        return (true, direction == 'e' ? magnitude : -magnitude);
    }

    private static bool TryParseInt(string text, out int value) =>
        int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
}
