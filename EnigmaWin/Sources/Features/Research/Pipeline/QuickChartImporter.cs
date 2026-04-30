// QuickChartImporter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EnigmaWin.Sources.Features.Research.Pipeline;

/// <summary>Parses QuickChart SDF (Structure Defined Format) files into ResearchInputRecord list.</summary>
/// <remarks>
/// Fixed-width columns, Windows-1252 encoding, CR/LF line endings.
/// Blank lines are silently skipped. Lines shorter than 74 characters throw a parse error.
/// Content at column 101+ (calculated positions) is ignored.
///
/// Column layout (1-based):
///   1–23   Name                    (ignored)
///  24–26   Month abbreviation      JAN … DEC
///  28–29   Day                     two digits
///  32–35   Year                    four digits
///  36–46   Time                    hh:mm:ss AM  or  hh:mm:ss PM
///  48–50   Timezone indicator      LMT  or  spaces
///  51–56   UTC offset              ±hh:mm
///  57–65   Geographic longitude    dddDmm'ss  (D = E or W)
///  67–74   Geographic latitude     ddDmm'ss   (D = N or S)
///
/// If the timezone indicator is LMT, the UTC offset is derived from longitude / 15.
/// </remarks>
public sealed class QuickChartImporter : IDataImporter
{
    public List<ResearchInputRecord> Parse(string sourcePath, bool isData, int startId)
    {
        string raw;
        // Windows-1252 is code page 1252; fall back to Latin-1
        var cp1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252) ?? Encoding.Latin1;
        try { raw = File.ReadAllText(sourcePath, cp1252); }
        catch
        {
            try { raw = File.ReadAllText(sourcePath, Encoding.Latin1); }
            catch { throw DataImportException.Unreadable(sourcePath); }
        }
        return ParseString(raw, isData, startId);
    }

    public List<ResearchInputRecord> ParseString(string content, bool isData, int startId)
    {
        var normalised = content.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines      = normalised.Split('\n');
        var records    = new List<ResearchInputRecord>();
        var nextId     = startId;

        for (var i = 0; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var line       = lines[i].TrimEnd('\r');

            if (line.Trim().Length == 0) continue;

            if (line.Length < 74)
                throw DataImportException.ParseError(lineNumber, line);

            // Extract fixed-width fields (0-based indices)
            var monthStr  = line[23..26];           // pos 24–26
            var dayStr    = line[27..29];            // pos 28–29
            var yearStr   = line[31..35];            // pos 32–35
            var timeStr   = line[35..46];            // pos 36–46
            var tzStr     = line[47..50];            // pos 48–50
            var offsetStr = line[50..56];            // pos 51–56
            var lonStr    = line[56..65];            // pos 57–65
            var latStr    = line[66..74];            // pos 67–74

            var month  = ParseMonth(monthStr,  lineNumber);
            var day    = ParseDay(dayStr,      lineNumber);
            var year   = ParseYear(yearStr,    lineNumber);
            var (hour, minute, second) = ParseTime(timeStr, lineNumber);
            var geoLon = ParseLongitude(lonStr, lineNumber);
            var geoLat = ParseLatitude(latStr,  lineNumber);

            var isLmt  = tzStr.Trim().Equals("LMT", StringComparison.OrdinalIgnoreCase);
            var offset = isLmt ? geoLon / 15.0 : ParseOffset(offsetStr, lineNumber);

            if (geoLat < -90  || geoLat > 90)  throw DataImportException.OutOfRange(lineNumber, "latitude",  latStr);
            if (geoLon < -180 || geoLon > 180) throw DataImportException.OutOfRange(lineNumber, "longitude", lonStr);
            if (offset < -14  || offset > 14)  throw DataImportException.OutOfRange(lineNumber, "offset",    offsetStr);

            records.Add(new ResearchInputRecord(nextId++, isData, geoLat, geoLon,
                                                year, month, day, hour, minute, second, offset));
        }

        if (records.Count == 0) throw DataImportException.NoData();
        return records;
    }

    // ── Field parsers ─────────────────────────────────────────────────────────

    private static int ParseMonth(string raw, int line) => raw.Trim().ToUpperInvariant() switch
    {
        "JAN" => 1,  "FEB" => 2,  "MAR" => 3,  "APR" => 4,
        "MAY" => 5,  "JUN" => 6,  "JUL" => 7,  "AUG" => 8,
        "SEP" => 9,  "OCT" => 10, "NOV" => 11, "DEC" => 12,
        _ => throw DataImportException.OutOfRange(line, "month", raw.Trim())
    };

    private static int ParseDay(string raw, int line)
    {
        if (!int.TryParse(raw.Trim(), out var d) || d < 1 || d > 31)
            throw DataImportException.OutOfRange(line, "day", raw.Trim());
        return d;
    }

    private static int ParseYear(string raw, int line)
    {
        if (!int.TryParse(raw.Trim(), out var y))
            throw DataImportException.ParseError(line, raw);
        return y;
    }

    /// <summary>Parses "hh:mm:ss AM" or "hh:mm:ss PM" (11 chars). Converts 12-hour to 24-hour clock.</summary>
    private static (int hour, int minute, int second) ParseTime(string raw, int line)
    {
        var parts = raw.Split(':');
        if (parts.Length < 3 ||
            !int.TryParse(parts[0].Trim(), out var hourRaw) ||
            !int.TryParse(parts[1].Trim(), out var minute))
            throw DataImportException.ParseError(line, raw);

        var secTokens = parts[2].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (!int.TryParse(secTokens[0], out var second))
            throw DataImportException.ParseError(line, raw);

        var amPm = secTokens.Length >= 2 ? secTokens[1].ToUpperInvariant() : "AM";
        var hour = hourRaw;
        if      (amPm == "PM" && hour != 12) hour += 12;
        else if (amPm == "AM" && hour == 12) hour  =  0;

        if (hour   < 0 || hour   > 23) throw DataImportException.OutOfRange(line, "hour",   $"{hourRaw}");
        if (minute < 0 || minute > 59) throw DataImportException.OutOfRange(line, "minute", parts[1]);
        if (second < 0 || second > 59) throw DataImportException.OutOfRange(line, "second", secTokens[0]);
        return (hour, minute, second);
    }

    /// <summary>Parses dddDmm'ss (D = E or W), e.g. "002E21'00". Returns decimal degrees; West is negative.</summary>
    private static double ParseLongitude(string raw, int line)
    {
        if (raw.Length < 9 ||
            !int.TryParse(raw[0..3], out var deg) ||
            !int.TryParse(raw[4..6], out var min) ||
            !int.TryParse(raw[7..9], out var sec))
            throw DataImportException.ParseError(line, raw);

        var dir = raw[3..4].ToUpperInvariant();
        if (dir != "E" && dir != "W") throw DataImportException.ParseError(line, raw);
        var d = deg + min / 60.0 + sec / 3600.0;
        return dir == "W" ? -d : d;
    }

    /// <summary>Parses ddDmm'ss (D = N or S), e.g. "48N51'00". Returns decimal degrees; South is negative.</summary>
    private static double ParseLatitude(string raw, int line)
    {
        if (raw.Length < 8 ||
            !int.TryParse(raw[0..2], out var deg) ||
            !int.TryParse(raw[3..5], out var min) ||
            !int.TryParse(raw[6..8], out var sec))
            throw DataImportException.ParseError(line, raw);

        var dir = raw[2..3].ToUpperInvariant();
        if (dir != "N" && dir != "S") throw DataImportException.ParseError(line, raw);
        var d = deg + min / 60.0 + sec / 3600.0;
        return dir == "S" ? -d : d;
    }

    /// <summary>Parses ±hh:mm (6 chars), e.g. "-01:00". Returns decimal hours.</summary>
    private static double ParseOffset(string raw, int line)
    {
        var s = raw.Trim();
        if (s.Length == 0) return 0.0;

        var sign = 1.0;
        if      (s[0] == '-') { sign = -1.0; s = s[1..]; }
        else if (s[0] == '+') { s = s[1..]; }

        var parts = s.Split(':');
        if (parts.Length != 2 ||
            !double.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var hours) ||
            !double.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var minutes))
            throw DataImportException.ParseError(line, raw);

        return sign * (hours + minutes / 60.0);
    }
}
