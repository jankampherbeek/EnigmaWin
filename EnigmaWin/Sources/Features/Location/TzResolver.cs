// TzResolver.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWin.Sources.Features.Location;

/// <summary>
/// Resolves the full timezone + DST information for a given local date/time and IANA zone name.
/// </summary>
internal sealed class TzResolver(LocationDb db)
{
    public ZoneInfo Resolve(AstronomicalDateTime dateTime, string zoneName, double? longitude = null)
    {
        var tzRows = db.TzDataRows(zoneName);
        if (tzRows.Count == 0)
            return new ZoneInfo(0, zoneName, false, false, false);

        var requestJd = JulianDay(dateTime);
        var tzRow = ApplicableTzRow(tzRows, requestJd, dateTime);

        if ((tzRow.Format.Contains("LMT", StringComparison.OrdinalIgnoreCase) ||
             tzRow.Rule == "LMT") && longitude.HasValue)
        {
            var offsetSec = (int)(longitude.Value / 15.0 * 3600.0);
            return new ZoneInfo(offsetSec, "LMT", false, false, false);
        }

        var stdOffsetSec = tzRow.StandardOffsetSeconds;
        var ruleName = tzRow.Rule;

        if (ruleName is "-" or "" or null)
        {
            var name = ResolvedTzName(tzRow.Format, "", stdOffsetSec);
            return new ZoneInfo(stdOffsetSec, name, false, false, false);
        }

        var dst = ResolveDst(ruleName, dateTime, stdOffsetSec, requestJd);
        var totalOffset = stdOffsetSec + dst.SaveSeconds;
        return new ZoneInfo(
            OffsetSeconds: totalOffset,
            TzName:        ResolvedTzName(tzRow.Format, dst.Letter, totalOffset),
            DstUsed:       dst.SaveSeconds != 0,
            IsInvalid:     dst.IsInvalid,
            IsAmbiguous:   dst.IsAmbiguous);
    }

    private static TzDataRow ApplicableTzRow(
        IReadOnlyList<TzDataRow> rows, double requestJd, AstronomicalDateTime dateTime)
    {
        foreach (var row in rows)
        {
            if (row.UntilYear == 0) return row;

            var untilDay = DayDefinitionResolver.ResolveIana(row.UntilDay, row.UntilYear, row.UntilMonth) ?? 1;
            var untilDate = new AstronomicalDate(row.UntilYear, row.UntilMonth, untilDay, true);
            var untilTime = new AstronomicalTime(row.AtH, row.AtM, row.AtS);
            var untilJd = SEWrapper.JulianDay(untilDate, untilTime);

            if (requestJd < untilJd) return row;
        }
        return rows[^1];
    }

    private sealed record DstResult(int SaveSeconds, string Letter, bool IsInvalid, bool IsAmbiguous);

    private DstResult ResolveDst(string ruleName, AstronomicalDateTime dateTime, int stdOffsetSec, double requestJd)
    {
        var rows = db.DstDataRows(ruleName);
        if (rows.Count == 0)
            return new DstResult(0, "", false, false);

        var events = ExpandedDstEvents(rows, dateTime.Date.Year, stdOffsetSec);
        events.Sort((a, b) => a.JdLocal.CompareTo(b.JdLocal));

        var currentSave = 0;
        var currentLetter = "";
        var isInvalid = false;
        var isAmbiguous = false;

        foreach (var ev in events)
        {
            if (ev.JdLocal > requestJd) break;

            var delta = ev.SaveSeconds - currentSave;
            if (delta > 0)
            {
                var gapEnd = ev.JdLocal + delta / 86400.0;
                if (requestJd >= ev.JdLocal && requestJd < gapEnd)
                    isInvalid = true;
            }
            else if (delta < 0)
            {
                var overlapEnd = ev.JdLocal + (-delta) / 86400.0;
                if (requestJd >= ev.JdLocal && requestJd < overlapEnd)
                    isAmbiguous = true;
            }

            currentSave   = ev.SaveSeconds;
            currentLetter = ev.Letter;
        }

        return new DstResult(currentSave, currentLetter, isInvalid, isAmbiguous);
    }

    private sealed record DstEvent(double JdLocal, int SaveSeconds, string Letter);

    private List<DstEvent> ExpandedDstEvents(
        IReadOnlyList<DstDataRow> rows, int aroundYear, int stdOffsetSec)
    {
        var startYear = aroundYear - 1;
        var endYear   = aroundYear + 1;
        var events    = new List<DstEvent>();

        foreach (var row in rows)
        {
            var toYearResolved = row.ToYear.ToLowerInvariant() switch
            {
                "only" => row.FromYear,
                "max"  => endYear,
                _      => int.TryParse(row.ToYear, out var n) ? n : row.FromYear
            };

            var lo = Math.Max(row.FromYear, startYear);
            var hi = Math.Min(toYearResolved, endYear);
            if (lo > hi) continue;

            for (var y = lo; y <= hi; y++)
            {
                var day = DayDefinitionResolver.ResolveIana(row.Day, y, row.Month);
                if (day is null) continue;

                var date = new AstronomicalDate(y, row.Month, day.Value, true);
                var time = new AstronomicalTime(row.AtH, row.AtM, row.AtS);
                var jdUt = SEWrapper.JulianDay(date, time);

                if (!row.UseUt.Equals("u", StringComparison.OrdinalIgnoreCase))
                    jdUt -= stdOffsetSec / 86400.0;

                var jdLocal = jdUt + stdOffsetSec / 86400.0;
                events.Add(new DstEvent(jdLocal, row.SaveSeconds, row.Letter));
            }
        }
        return events;
    }

    private static double JulianDay(AstronomicalDateTime dt) =>
        SEWrapper.JulianDay(dt.Date, dt.Time);

    private static string ResolvedTzName(string format, string letter, int offsetSec)
    {
        var effectiveLetter = letter == "-" ? "" : letter;
        var result = format;

        if (result.Contains("%s", StringComparison.Ordinal))
            result = result.Replace("%s", effectiveLetter, StringComparison.Ordinal);

        if (result.Contains("%z", StringComparison.Ordinal) ||
            result.Contains("%Z", StringComparison.Ordinal))
        {
            var sign  = offsetSec >= 0 ? "+" : "-";
            var total = Math.Abs(offsetSec);
            var h     = total / 3600;
            var m     = (total % 3600) / 60;
            var off   = m == 0 ? $"{sign}{h}" : $"{sign}{h}:{m:D2}";
            result = result
                .Replace("%z", off, StringComparison.Ordinal)
                .Replace("%Z", off, StringComparison.Ordinal);
        }
        return result;
    }
}
