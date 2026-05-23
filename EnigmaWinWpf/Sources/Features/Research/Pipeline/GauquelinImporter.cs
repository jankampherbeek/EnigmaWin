// GauquelinImporter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EnigmaWin.Sources.Features.Research.Pipeline;

/// <summary>Parses Gauquelin tab-separated data files into ResearchInputRecord list.</summary>
/// <remarks>
/// Tab-separated, first row is a header and is skipped.
/// Blank lines and lines starting with '#' are silently skipped.
/// Seconds are not present in coordinate fields and are treated as zero.
///
/// Column order:
///   NUM  NAT  DAY  MON  YEA  HOU  MIN  SEC  TMZ  LAT  LON  COD  PLA
///
/// LAT: ddNmm or ddSmm   (N positive, S negative; no seconds)
/// LON: ddEmm or ddWmm   (E positive, W negative; no seconds)
/// TMZ: UTC offset in whole hours
/// </remarks>
public sealed class GauquelinImporter : IDataImporter
{
    public List<ResearchInputRecord> Parse(string sourcePath, bool isData, int startId)
    {
        string raw;
        try { raw = File.ReadAllText(sourcePath, Encoding.UTF8); }
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
        var headerSkipped = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var trimmed    = lines[i].Trim();

            if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;

            if (!headerSkipped) { headerSkipped = true; continue; }

            var fields = lines[i].Split('\t');
            if (fields.Length < 13)
                throw DataImportException.ParseError(lineNumber, trimmed);

            var dayStr = fields[2].Trim();
            var monStr = fields[3].Trim();
            var yeaStr = fields[4].Trim();
            var houStr = fields[5].Trim();
            var minStr = fields[6].Trim();
            var secStr = fields[7].Trim();
            var tmzStr = fields[8].Trim();
            var latStr = fields[9].Trim();
            var lonStr = fields[10].Trim();

            if (!int.TryParse(dayStr, out var day)    || day    < 1  || day    > 31) throw DataImportException.OutOfRange(lineNumber, "day",      dayStr);
            if (!int.TryParse(monStr, out var month)  || month  < 1  || month  > 12) throw DataImportException.OutOfRange(lineNumber, "month",    monStr);
            if (!int.TryParse(yeaStr, out var year))                                  throw DataImportException.ParseError(lineNumber, yeaStr);
            if (!int.TryParse(houStr, out var hour)   || hour   < 0  || hour   > 23) throw DataImportException.OutOfRange(lineNumber, "hour",     houStr);
            if (!int.TryParse(minStr, out var minute) || minute < 0  || minute > 59) throw DataImportException.OutOfRange(lineNumber, "minute",   minStr);
            if (!int.TryParse(secStr, out var second) || second < 0  || second > 59) throw DataImportException.OutOfRange(lineNumber, "second",   secStr);

            if (!double.TryParse(tmzStr, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var offset) ||
                offset < -14 || offset > 14)
                throw DataImportException.OutOfRange(lineNumber, "timezone", tmzStr);

            var geoLat = ParseCoordinate(latStr, positiveDir: 'N', negativeDir: 'S', lineNumber);
            var geoLon = ParseCoordinate(lonStr, positiveDir: 'E', negativeDir: 'W', lineNumber);

            if (geoLat < -90  || geoLat > 90)  throw DataImportException.OutOfRange(lineNumber, "latitude",  latStr);
            if (geoLon < -180 || geoLon > 180) throw DataImportException.OutOfRange(lineNumber, "longitude", lonStr);

            records.Add(new ResearchInputRecord(nextId++, isData, geoLat, geoLon,
                                                year, month, day, hour, minute, second, offset));
        }

        if (records.Count == 0) throw DataImportException.NoData();
        return records;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Parses ddNmm / ddSmm (latitude) or ddEmm / ddWmm (longitude). No seconds in Gauquelin format.</summary>
    private static double ParseCoordinate(string raw, char positiveDir, char negativeDir, int line)
    {
        var upper = raw.ToUpperInvariant();
        bool isNegative;
        char splitChar;

        if (upper.Contains(positiveDir))       { isNegative = false; splitChar = positiveDir; }
        else if (upper.Contains(negativeDir))  { isNegative = true;  splitChar = negativeDir; }
        else throw DataImportException.ParseError(line, raw);

        var idx = upper.IndexOf(splitChar);
        var degStr = upper[..idx].Trim();
        var minStr = upper[(idx + 1)..].Trim();

        if (!double.TryParse(degStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var deg) ||
            !double.TryParse(minStr, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var min))
            throw DataImportException.ParseError(line, raw);

        var decimal_ = deg + min / 60.0;
        return isNegative ? -decimal_ : decimal_;
    }
}
