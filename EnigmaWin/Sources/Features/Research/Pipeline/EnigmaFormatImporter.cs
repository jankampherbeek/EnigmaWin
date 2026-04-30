// EnigmaFormatImporter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EnigmaWin.Sources.Features.Research.Pipeline;

/// <summary>Parses the standard Enigma CSV format into ResearchInputRecord list.</summary>
/// <remarks>
/// Comma-separated, first row is a header and is skipped.
/// Blank lines and lines starting with '#' are silently skipped.
///
/// Column order:
///   Id, Name, longitude, latitude, date, cal, time, zone, dst
///
/// longitude / latitude: deg:min:sec:dir  (dir = E/W or N/S)
/// date:  year/month/day
/// cal:   G (Gregorian) or J (Julian) — not used further
/// time:  hour:minute  or  hour:minute:second
/// zone:  UTC offset as decimal hours
/// dst:   daylight-saving offset as decimal hours
/// Effective UTC offset = zone + dst
/// </remarks>
public sealed class EnigmaFormatImporter : IDataImporter
{
    public List<ResearchInputRecord> Parse(string sourcePath, bool isData, int startId)
    {
        string raw;
        try { raw = File.ReadAllText(sourcePath, Encoding.UTF8); }
        catch { throw DataImportException.Unreadable(sourcePath); }
        return ParseString(raw, isData, startId);
    }

    public List<ResearchInputRecord> ParseString(string content, bool isData, int startId)
    {
        var lines   = content.Split('\n');
        var records = new List<ResearchInputRecord>();
        var nextId  = startId;
        var headerSkipped = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var trimmed    = lines[i].Trim().TrimEnd('\r');

            if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;

            if (!headerSkipped) { headerSkipped = true; continue; }

            var fields = trimmed.Split(',');
            if (fields.Length != 9)
                throw DataImportException.ParseError(lineNumber, trimmed);

            for (var f = 0; f < fields.Length; f++) fields[f] = fields[f].Trim();

            // fields[0] = Id (ignored), fields[1] = Name (ignored)
            var geoLon = ParseDmsDirection(fields[2], "longitude", lineNumber);
            var geoLat = ParseDmsDirection(fields[3], "latitude",  lineNumber);
            var (year, month, day) = ParseDate(fields[4], lineNumber);
            // fields[5] = cal (J/G) — not used
            var (hour, minute, second) = ParseTime(fields[6], lineNumber);

            if (!double.TryParse(fields[7], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var zone))
                throw DataImportException.ParseError(lineNumber, fields[7]);
            if (!double.TryParse(fields[8], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var dst))
                throw DataImportException.ParseError(lineNumber, fields[8]);

            var offset = zone + dst;

            if (geoLat < -90  || geoLat > 90)   throw DataImportException.OutOfRange(lineNumber, "latitude",  fields[3]);
            if (geoLon < -180 || geoLon > 180)   throw DataImportException.OutOfRange(lineNumber, "longitude", fields[2]);
            if (offset < -14  || offset > 14)    throw DataImportException.OutOfRange(lineNumber, "zone+dst",  $"{offset}");

            records.Add(new ResearchInputRecord(nextId++, isData, geoLat, geoLon,
                                                year, month, day, hour, minute, second, offset));
        }

        if (records.Count == 0) throw DataImportException.NoData();
        return records;
    }

    // ── Field parsers ─────────────────────────────────────────────────────────

    /// <summary>Parses deg:min:sec:dir. Southern and western directions are negative.</summary>
    private static double ParseDmsDirection(string raw, string fieldName, int line)
    {
        var parts = raw.Split(':');
        if (parts.Length != 4 ||
            !double.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var deg) ||
            !double.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var min) ||
            !double.TryParse(parts[2].Trim(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var sec))
            throw DataImportException.ParseError(line, raw);

        var dir = parts[3].Trim().ToUpperInvariant();
        if (dir != "N" && dir != "S" && dir != "E" && dir != "W")
            throw DataImportException.ParseError(line, raw);

        var decimal_ = deg + min / 60.0 + sec / 3600.0;
        return dir is "S" or "W" ? -decimal_ : decimal_;
    }

    private static (int year, int month, int day) ParseDate(string raw, int line)
    {
        var parts = raw.Split('/');
        if (parts.Length != 3 ||
            !int.TryParse(parts[0].Trim(), out var year) ||
            !int.TryParse(parts[1].Trim(), out var month) ||
            !int.TryParse(parts[2].Trim(), out var day))
            throw DataImportException.ParseError(line, raw);

        if (month < 1 || month > 12) throw DataImportException.OutOfRange(line, "month", parts[1]);
        if (day   < 1 || day   > 31) throw DataImportException.OutOfRange(line, "day",   parts[2]);
        return (year, month, day);
    }

    private static (int hour, int minute, int second) ParseTime(string raw, int line)
    {
        var parts = raw.Split(':');
        if (parts.Length < 2 ||
            !int.TryParse(parts[0].Trim(), out var hour) ||
            !int.TryParse(parts[1].Trim(), out var minute))
            throw DataImportException.ParseError(line, raw);

        var second = parts.Length >= 3 && int.TryParse(parts[2].Trim(), out var s) ? s : 0;

        if (hour   < 0 || hour   > 23) throw DataImportException.OutOfRange(line, "hour",   parts[0]);
        if (minute < 0 || minute > 59) throw DataImportException.OutOfRange(line, "minute", parts[1]);
        if (second < 0 || second > 59) throw DataImportException.OutOfRange(line, "second", $"{second}");
        return (hour, minute, second);
    }
}
