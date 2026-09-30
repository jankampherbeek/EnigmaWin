// AafMapper.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWin.Sources.Features.ImportExport.Aaf;

/// <summary>
/// Maps between <see cref="AafRecord"/> and the data needed to build or read a <see cref="Horoscope"/> with
/// its preferred <see cref="HoroscopeDateTime"/>.
/// A <see cref="ChartEvent"/> is deliberately never created from AAF: an Enigma event must be linked to one
/// or more existing charts, and a bare AAF record carries no such link. AAF's "type" field ('e' for event,
/// 'l' for country, 'o' for organisation) is instead kept in <see cref="Horoscope.Category"/>.
/// </summary>
public static class AafMapper
{
    public sealed record MappedChart(
        string Name,
        string Category,
        string? Source,
        string? Notes,
        string? PlaceName,
        double Latitude,
        double Longitude,
        double JulianDate,
        string OriginalInput,
        Guid? Id);

    // ── Import ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Maps one record. When the returned messages contain a fatal error, the chart is null and the record
    /// must be skipped.
    /// </summary>
    public static (MappedChart? Chart, List<ExchangeMessage> Messages) ToMappedChart(AafRecord record, int recordNumber)
    {
        var messages = new List<ExchangeMessage>();

        var offsetSeconds = ResolveOffsetSeconds(record, recordNumber, messages);
        if (offsetSeconds is null) return (null, messages);

        var localDate = new AstronomicalDate(record.Year, record.Month, record.Day, record.IsGregorian);
        var localTime = new AstronomicalTime(record.Hour, record.Minute, record.Second);
        var julianDate = ExchangeDateTime.LocalToUtJulianDay(localDate, localTime, offsetSeconds.Value);

        if (record.JulianDayRaw != "*"
            && double.TryParse(record.JulianDayRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out var providedJd)
            && Math.Abs(providedJd - julianDate) > 0.001)
        {
            messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgAafJdMismatch, recordNumber, "julianDay",
                providedJd, julianDate));
        }

        if (record.Type is "m" or "f" or "w")
            messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgAafGender, recordNumber, "type", record.Type));

        var notes = record.Comment;
        if (!string.IsNullOrEmpty(record.Via))
        {
            var viaLine = $"Via: {record.Via}";
            notes = notes is null ? viaLine : $"{notes}\n{viaLine}";
        }

        string? placeName;
        if (record.Place.Length == 0) placeName = null;
        else if (record.Country.Length == 0 || record.Country == "*") placeName = record.Place;
        else placeName = $"{record.Place},{record.Country}";

        var chart = new MappedChart(
            CombinedName(record.LastName, record.FirstName),
            CategoryFor(record.Type),
            string.IsNullOrEmpty(record.Source) ? null : record.Source,
            notes,
            placeName,
            record.Latitude,
            record.Longitude,
            julianDate,
            OriginalInputText(record),
            record.EnigmaId);
        return (chart, messages);
    }

    private static string CombinedName(string lastName, string firstName)
    {
        var last = lastName == "*" ? "" : lastName;
        var first = firstName == "*" ? "" : firstName;
        if (first.Length == 0) return last;
        if (last.Length == 0) return first;
        return $"{first} {last}";
    }

    private static string CategoryFor(string type) => type switch
    {
        "e" => "event",
        "l" => "country",
        "o" => "organisation",
        _   => ""
    };

    private static int? ResolveOffsetSeconds(AafRecord record, int recordNumber, List<ExchangeMessage> messages)
    {
        var lmtSeconds = (int)Math.Round(record.Longitude / 15.0 * 3600.0, MidpointRounding.AwayFromZero);
        switch (record.TimeType)
        {
            case "l":
                return lmtSeconds;
            case "m":
                return record.GreenwichOffsetSeconds ?? lmtSeconds;
        }

        if (record.GreenwichOffsetSeconds is not { } baseOffset)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafUnknownOffset, recordNumber, "greenwichOffset",
                record.TimeType));
            return null;
        }

        int dstSeconds;
        switch (record.TimeType)
        {
            case "1" or "w": dstSeconds = 3600; break;
            case "2":        dstSeconds = 7200; break;
            case "h":        dstSeconds = 1800; break;
            case "0" or "":  dstSeconds = 0;    break;
            default:
                messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgAafUnknownTimeType, recordNumber, "timeType",
                    record.TimeType));
                dstSeconds = 0;
                break;
        }
        return baseOffset + dstSeconds;
    }

    private static string OriginalInputText(AafRecord record)
    {
        var calendar = record.IsGregorian ? "g" : "j";
        return $"AAF {record.Day}.{record.Month}.{record.Year}{calendar} " +
               $"{record.Hour:00}:{record.Minute:00}:{record.Second:00} (type {record.Type}, timeType {record.TimeType})";
    }

    // ── Export ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the AAF record for one chart. Enigma stores only the resolved Julian Day (UT) and an IANA
    /// timezone identifier used for display, never a separate standard/DST offset. AAF export therefore always
    /// writes a zero Greenwich offset with time type "0": the moment in time is preserved exactly, only the
    /// local display/timezone identity is not.
    /// </summary>
    public static (AafRecord Record, List<ExchangeMessage> Messages) ToRecord(
        Guid id, string name, string category, string? source, string? notes, string? placeName,
        double latitude, double longitude, double julianDate, string timeZoneIdentifier, int recordNumber)
    {
        var messages = new List<ExchangeMessage>();
        if (timeZoneIdentifier != "UTC")
            messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgAafTzNotPreserved, recordNumber, "timezone",
                timeZoneIdentifier));

        var (lastName, firstName) = SplitName(name);

        // Horoscope has no separate country field, so import merges "place,country" into one PlaceName. AAF's
        // country field cannot be reconstructed from that reliably (a comma may be part of the place name), so
        // export always writes country "*" and rewrites a comma in the place, per the format's own example
        // ("Staten Island, New York" -> "Staten Island (New York)").
        var rawPlace = placeName ?? "";
        var place = SanitizePlace(rawPlace);
        if (rawPlace.Contains(','))
            messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgAafPlaceComma, recordNumber, "place", rawPlace, place));

        var dateTime = ExchangeDateTime.UtFromJulianDay(julianDate);

        var record = new AafRecord
        {
            LastName = lastName,
            FirstName = firstName,
            Type = TypeFor(category),
            Day = dateTime.Date.Day,
            Month = dateTime.Date.Month,
            Year = dateTime.Date.Year,
            IsGregorian = true,
            Hour = dateTime.Time.Hour,
            Minute = dateTime.Time.Minute,
            Second = dateTime.Time.Second,
            Place = place,
            Country = "*",
            JulianDayRaw = "*",
            Latitude = latitude,
            Longitude = longitude,
            GreenwichOffsetSeconds = 0,
            TimeType = "0",
            ZoneName = "UTC",
            Source = string.IsNullOrEmpty(source) ? null : source,
            Comment = string.IsNullOrEmpty(notes) ? null : notes,
            EnigmaId = id
        };
        return (record, messages);
    }

    /// <summary>
    /// Best-effort inverse of <see cref="CombinedName"/>: "Last, First" or "First Last" to (last, first).
    /// Enigma stores a single free-text name, so this split is necessarily a heuristic.
    /// </summary>
    private static (string LastName, string FirstName) SplitName(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.Length == 0) return ("*", "*");

        var commaIndex = trimmed.IndexOf(',');
        if (commaIndex >= 0)
        {
            var last = trimmed[..commaIndex].Trim();
            var first = trimmed[(commaIndex + 1)..].Trim();
            return (last.Length == 0 ? "*" : last, first.Length == 0 ? "*" : first);
        }

        var spaceIndex = trimmed.LastIndexOf(' ');
        if (spaceIndex >= 0)
        {
            var first = trimmed[..spaceIndex].Trim();
            var last = trimmed[(spaceIndex + 1)..].Trim();
            return (last.Length == 0 ? "*" : last, first.Length == 0 ? "*" : first);
        }
        return (trimmed, "*");
    }

    private static string TypeFor(string category) => category.ToLowerInvariant() switch
    {
        "event"        => "e",
        "country"      => "l",
        "organisation" => "o",
        _              => "*"
    };

    /// <summary>
    /// AAF has no CSV-style quoting, so a comma inside a free field must be rewritten;
    /// "Staten Island, New York" becomes "Staten Island (New York)".
    /// </summary>
    private static string SanitizePlace(string place)
    {
        if (!place.Contains(',')) return place;
        var parts = place.Split(',', 2);
        var first = parts[0].Trim();
        var second = parts[1].Trim();
        if (second.Length == 0 || second.Contains(','))
            return place.Replace(',', ' ');
        return $"{first} ({second})";
    }
}
