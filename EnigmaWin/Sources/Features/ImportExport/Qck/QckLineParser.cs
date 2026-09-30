// QckLineParser.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ImportExport.Shared;
using EnigmaWin.Sources.Features.Shared.Validation;

namespace EnigmaWin.Sources.Features.ImportExport.Qck;

/// <summary>
/// Parses a single fixed-width Quick*Chart / QCK record (100 or 101 characters) into a <see cref="QckRecord"/>.
/// Offsets are zero-based. Only CR/LF is stripped before the length check; the line is never fully trimmed
/// first, since that would shift every fixed field.
/// </summary>
public static class QckLineParser
{
    public static readonly IReadOnlyDictionary<string, int> Months = new Dictionary<string, int>
    {
        ["JAN"] = 1, ["FEB"] = 2, ["MAR"] = 3, ["APR"] = 4, ["MAY"] = 5, ["JUN"] = 6,
        ["JUL"] = 7, ["AUG"] = 8, ["SEP"] = 9, ["OCT"] = 10, ["NOV"] = 11, ["DEC"] = 12
    };

    /// <summary>
    /// Parses one record. Returns a null record together with at least one fatal message when the record
    /// cannot be represented; otherwise returns the record together with zero or more warnings.
    /// </summary>
    public static (QckRecord? Record, List<ExchangeMessage> Messages) Parse(string line, int recordNumber)
    {
        var messages = new List<ExchangeMessage>();
        var text = line.TrimEnd('\r', '\n');

        if (!(text.Length == 100 || (text.Length == 101 && text[100] == ' ')))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckInvalidLength, recordNumber, null, text.Length));
            return (null, messages);
        }

        var name = text[..23].TrimEnd(' ');

        var month = ParseMonth(text[23..26], recordNumber, messages);
        if (month is null) return (null, messages);
        var day = ParseInt(text[26..29], "day", recordNumber, messages);
        if (day is null) return (null, messages);
        var year = ParseYear(text[30..35], recordNumber, messages);
        if (year is null) return (null, messages);

        if (!IsValidDate(year.Value, month.Value, day.Value, gregorian: true))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgInvalidDate, recordNumber, "date",
                $"{day}-{month}-{year}"));
            return (null, messages);
        }
        if (ExchangeDateTime.IsBeforeGregorianCutover(year.Value, month.Value, day.Value))
            messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgQckBeforeCutover, recordNumber, "date"));

        var time = ParseTime(text[35..47], recordNumber, messages);
        if (time is null) return (null, messages);

        var tzAbbreviation = text[47..50].Trim().ToUpperInvariant();

        var correction = ParseCorrection(text[50..56], recordNumber, messages);
        if (correction is null) return (null, messages);

        var longitude = ParseLongitude(text, 56, recordNumber, messages);
        if (longitude is null) return (null, messages);
        var latitude = ParseLatitude(text, 66, recordNumber, messages);
        if (latitude is null) return (null, messages);

        var place = text[75..100].TrimEnd(' ');

        var record = new QckRecord
        {
            Name = name,
            Month = month.Value,
            Day = day.Value,
            Year = year.Value,
            Hour = time.Value.Hour,
            Minute = time.Value.Minute,
            Second = time.Value.Second,
            IsPm = time.Value.IsPm,
            TimeZoneAbbreviation = tzAbbreviation,
            QckCorrectionMinutes = correction.Value,
            Longitude = longitude.Value,
            Latitude = latitude.Value,
            Place = place
        };
        return (record, messages);
    }

    // ── Field parsers ────────────────────────────────────────────────────────

    private static int? ParseMonth(string raw, int recordNumber, List<ExchangeMessage> messages)
    {
        if (Months.TryGetValue(raw.Trim().ToUpperInvariant(), out var month)) return month;
        messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckInvalidMonth, recordNumber, "month", raw));
        return null;
    }

    private static int? ParseInt(string raw, string field, int recordNumber, List<ExchangeMessage> messages)
    {
        if (int.TryParse(raw.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value))
            return value;
        messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckInvalidNumber, recordNumber, field, field, raw));
        return null;
    }

    private static int? ParseYear(string raw, int recordNumber, List<ExchangeMessage> messages)
    {
        var year = ParseInt(raw, "year", recordNumber, messages);
        if (year is null) return null;
        if (year.Value is >= -9999 and <= 99999) return year;
        messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckYearRange, recordNumber, "year", year.Value));
        return null;
    }

    /// <summary>Parses "hh:mm:ss AM"/"hh:mm:ss PM", tolerant of the historical "00:00:00 PM" bug.</summary>
    private static (int Hour, int Minute, int Second, bool IsPm)? ParseTime(
        string raw, int recordNumber, List<ExchangeMessage> messages)
    {
        var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckInvalidTime, recordNumber, "time", raw));
            return null;
        }
        var amPm = parts[1].ToUpperInvariant();
        if (amPm is not ("AM" or "PM"))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckInvalidAmPm, recordNumber, "time", parts[1]));
            return null;
        }
        var clock = parts[0].Split(':');
        if (clock.Length != 3
            || !int.TryParse(clock[0], out var hour12Raw)
            || !int.TryParse(clock[1], out var minute)
            || !int.TryParse(clock[2], out var second))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckInvalidTime, recordNumber, "time", raw));
            return null;
        }
        if (hour12Raw is < 0 or > 12 || minute is < 0 or > 59 || second is < 0 or > 59)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckTimeRange, recordNumber, "time", raw));
            return null;
        }

        var isPm = amPm == "PM";
        // Tolerate the historical "00:mm:ss" bug by treating hour 0 like hour 12.
        var hour12 = hour12Raw == 0 ? 12 : hour12Raw;
        var hour24 = hour12 == 12
            ? (isPm ? 12 : 0)
            : (isPm ? hour12 + 12 : hour12);
        if (hour12Raw == 0)
            messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgQckNoonBug, recordNumber, "time"));
        return (hour24, minute, second, isPm);
    }

    /// <summary>Parses the QCK correction "±hh:mm" (QCK's own sign convention, UT = LocalTime + correction).</summary>
    private static int? ParseCorrection(string raw, int recordNumber, List<ExchangeMessage> messages)
    {
        var rest = raw.Trim();
        if (rest.Length == 0) return 0;
        var sign = 1;
        if (rest.StartsWith('-')) { sign = -1; rest = rest[1..]; }
        else if (rest.StartsWith('+')) rest = rest[1..];

        var parts = rest.Split(':');
        if (parts.Length == 2
            && int.TryParse(parts[0], out var hours)
            && int.TryParse(parts[1], out var minutes)
            && minutes is >= 0 and <= 59)
            return sign * (hours * 60 + minutes);

        messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckInvalidCorrection, recordNumber, "correction", raw));
        return null;
    }

    /// <summary>Longitude "DDD[E|W]MM'SS" at fixed sub-offsets relative to <paramref name="start"/>.</summary>
    private static double? ParseLongitude(string text, int start, int recordNumber, List<ExchangeMessage> messages)
    {
        var raw = text.Substring(start, 9);
        if (!int.TryParse(raw[..3], out var degrees)
            || !int.TryParse(raw[4..6], out var minutes)
            || !int.TryParse(raw[7..9], out var seconds))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckInvalidLongitude, recordNumber, "longitude", raw));
            return null;
        }
        var direction = char.ToUpperInvariant(raw[3]);
        if (direction is not ('E' or 'W'))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckLongitudeDirection, recordNumber, "longitude", raw[3]));
            return null;
        }
        if (!GeoCoordinateConversion.IsValidLongitude(degrees, minutes, seconds))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckLongitudeRange, recordNumber, "longitude", raw));
            return null;
        }
        return GeoCoordinateConversion.DecimalDegrees(degrees, minutes, seconds, direction == 'E');
    }

    /// <summary>Latitude "DD[N|S]MM'SS" at fixed sub-offsets relative to <paramref name="start"/>.</summary>
    private static double? ParseLatitude(string text, int start, int recordNumber, List<ExchangeMessage> messages)
    {
        var raw = text.Substring(start, 8);
        if (!int.TryParse(raw[..2], out var degrees)
            || !int.TryParse(raw[3..5], out var minutes)
            || !int.TryParse(raw[6..8], out var seconds))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckInvalidLatitude, recordNumber, "latitude", raw));
            return null;
        }
        var direction = char.ToUpperInvariant(raw[2]);
        if (direction is not ('N' or 'S'))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckLatitudeDirection, recordNumber, "latitude", raw[2]));
            return null;
        }
        if (!GeoCoordinateConversion.IsValidLatitude(degrees, minutes, seconds))
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckLatitudeRange, recordNumber, "latitude", raw));
            return null;
        }
        return GeoCoordinateConversion.DecimalDegrees(degrees, minutes, seconds, direction == 'N');
    }

    internal static bool IsValidDate(int year, int month, int day, bool gregorian) =>
        month is >= 1 and <= 12 && day is >= 1 and <= 31
        && AstronomicalDateValidation.ValidateDate(new AstronomicalDate(year, month, day, gregorian));
}

/// <summary>
/// Splits a QCK file (as decoded text) into records and parses each one. A fatal error in one record does not
/// abort the file: the remaining records are still parsed. Blank lines are ignored.
/// </summary>
public static class QckFileParser
{
    public static FormatParseResult<QckRecord> Parse(string content)
    {
        var lines = content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var result = new FormatParseResult<QckRecord>();
        var recordNumber = 0;

        foreach (var line in lines.Where(l => l.Trim().Length > 0))
        {
            recordNumber++;
            var (record, messages) = QckLineParser.Parse(line, recordNumber);
            result.Messages.AddRange(messages);
            if (record is not null) result.Records.Add(record);
        }
        return result;
    }
}
