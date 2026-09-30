// AafWriter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWin.Sources.Features.ImportExport.Aaf;

/// <summary>
/// Writes one <see cref="AafRecord"/> as an "#A93"/"#B93" text block plus optional metadata chunks.
/// Field-based chunks are comma-separated with no quoting mechanism, so a comma in a field-based value is
/// replaced with a space and reported; free-text chunks (#SRC/#VIA/#COM) are written verbatim.
/// </summary>
public static class AafWriter
{
    public static (string Text, List<ExchangeMessage> Messages) Write(AafRecord record, int recordNumber)
    {
        var messages = new List<ExchangeMessage>();

        var lastName = SanitizeField(record.LastName, "lastName", recordNumber, messages);
        var firstName = SanitizeField(record.FirstName, "firstName", recordNumber, messages);
        var place = SanitizeField(record.Place, "place", recordNumber, messages);
        var country = SanitizeField(record.Country, "country", recordNumber, messages);

        var calendarSuffix = record.IsGregorian ? "g" : "j";
        var dateText = $"{record.Day}.{record.Month}.{record.Year}{calendarSuffix}";
        var timeText = $"{record.Hour:00}:{record.Minute:00}:{record.Second:00}";
        var a93 = $"#A93:{lastName},{firstName},{record.Type},{dateText},{timeText},{place},{country}";

        var latText = CoordinateText(record.Latitude, 'n', 's');
        var lonText = CoordinateText(record.Longitude, 'e', 'w');
        var greenwichText = GreenwichOffsetText(record.GreenwichOffsetSeconds);
        var b93 = $"#B93:{record.JulianDayRaw},{latText},{lonText},{greenwichText},{record.TimeType}";

        var lines = new List<string> { a93, b93 };
        if (record.EnigmaId is { } id) lines.Add($"#ENID:{id.ToString().ToUpperInvariant()}");
        if (!string.IsNullOrEmpty(record.ZoneName)) lines.Add($"#ZNAM:{record.ZoneName}");
        if (!string.IsNullOrEmpty(record.Source)) lines.Add($"#SRC:{OneLine(record.Source)}");
        if (!string.IsNullOrEmpty(record.Via)) lines.Add($"#VIA:{OneLine(record.Via)}");
        if (!string.IsNullOrEmpty(record.Comment)) lines.Add($"#COM:{OneLine(record.Comment)}");

        return (string.Join("\n", lines), messages);
    }

    private static string SanitizeField(string value, string field, int recordNumber, List<ExchangeMessage> messages)
    {
        if (!value.Contains(',')) return value;
        messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgAafFieldComma, recordNumber, field, field));
        return value.Replace(',', ' ');
    }

    /// <summary>A chunk is a single line, so line breaks in free text (e.g. multi-line notes) become spaces.</summary>
    private static string OneLine(string text) => text.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');

    private static string CoordinateText(double decimalDegrees, char positiveChar, char negativeChar)
    {
        var c = GeoCoordinateConversion.Components(decimalDegrees);
        var dir = c.IsPositiveDirection ? positiveChar : negativeChar;
        return c.Seconds == 0
            ? $"{c.Degrees}{dir}{c.Minutes:00}"
            : $"{c.Degrees}{dir}{c.Minutes:00}:{c.Seconds:00}";
    }

    private static string GreenwichOffsetText(int? seconds)
    {
        if (seconds is not { } value) return "*";
        var dir = value < 0 ? 'w' : 'e';
        var magnitude = Math.Abs(value);
        var hours = magnitude / 3600;
        var minutes = magnitude % 3600 / 60;
        var secs = magnitude % 60;
        if (minutes == 0 && secs == 0) return $"{hours}h{dir}";
        if (secs == 0) return $"{hours}h{dir}{minutes:00}";
        return $"{hours}h{dir}{minutes:00}:{secs:00}";
    }
}
