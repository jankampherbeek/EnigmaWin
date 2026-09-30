// QckWriter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWin.Sources.Features.ImportExport.Qck;

/// <summary>
/// Writes one <see cref="QckRecord"/> as a 100-character fixed-width QCK line. Truncation of name or place is
/// never silent: it is always reported as a warning. The writer always uses a normal 12-hour time
/// (e.g. "12:00:00 AM"), never the historical "00:00:00 PM" form the parser tolerates on import.
/// </summary>
public static class QckWriter
{
    private static readonly string[] MonthAbbreviations =
        ["JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"];

    /// <summary>
    /// Returns the 100-character line, or null with a fatal message when the record cannot be represented in
    /// QCK at all (e.g. year out of range).
    /// </summary>
    public static (string? Line, List<ExchangeMessage> Messages) Write(QckRecord record, int recordNumber)
    {
        var messages = new List<ExchangeMessage>();

        if (record.Year is < -9999 or > 99999)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckWriteYearRange, recordNumber, "year", record.Year));
            return (null, messages);
        }
        if (record.Month is < 1 or > 12)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgQckWriteMonth, recordNumber, "month", record.Month));
            return (null, messages);
        }

        var name = FixedWidth(record.Name, 23, "name", recordNumber, messages);
        var monthText = MonthAbbreviations[record.Month - 1];
        var dayText = record.Day.ToString().PadLeft(3);
        var yearText = record.Year.ToString().PadLeft(5);
        var timeText = FormattedTime(record.Hour, record.Minute, record.Second) + " ";
        var tzText = (record.TimeZoneAbbreviation.Length > 3
            ? record.TimeZoneAbbreviation[..3]
            : record.TimeZoneAbbreviation).PadRight(3);

        var correctionSign = record.QckCorrectionMinutes < 0 ? "-" : "+";
        var correctionMagnitude = Math.Abs(record.QckCorrectionMinutes);
        var correctionText = $"{correctionSign}{correctionMagnitude / 60:00}:{correctionMagnitude % 60:00}";

        var lon = GeoCoordinateConversion.Components(record.Longitude);
        var longitudeText = $"{lon.Degrees:000}{(lon.IsPositiveDirection ? "E" : "W")}{lon.Minutes:00}'{lon.Seconds:00} ";

        var lat = GeoCoordinateConversion.Components(record.Latitude);
        var latitudeText = $"{lat.Degrees:00}{(lat.IsPositiveDirection ? "N" : "S")}{lat.Minutes:00}'{lat.Seconds:00} ";

        var place = FixedWidth(record.Place, 25, "place", recordNumber, messages);

        var line = name + monthText + dayText + "," + yearText + timeText + tzText + correctionText
                   + longitudeText + latitudeText + place;
        Debug.Assert(line.Length == 100);
        return (line, messages);
    }

    private static string FormattedTime(int hour, int minute, int second)
    {
        var hour12 = hour % 12;
        if (hour12 == 0) hour12 = 12;
        return $"{hour12:00}:{minute:00}:{second:00} {(hour >= 12 ? "PM" : "AM")}";
    }

    /// <summary>Left-aligns and pads <paramref name="value"/> to <paramref name="width"/>, truncating with a warning.</summary>
    private static string FixedWidth(string value, int width, string field, int recordNumber, List<ExchangeMessage> messages)
    {
        if (value.Length <= width) return value.PadRight(width);
        var truncated = value[..width];
        messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgQckTruncated, recordNumber, field,
            field, value.Length, width, value, truncated));
        return truncated;
    }
}
