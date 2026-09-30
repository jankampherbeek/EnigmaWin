// QckMapper.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWin.Sources.Features.ImportExport.Qck;

/// <summary>
/// Maps between <see cref="QckRecord"/> (what is physically present in a QCK file) and the data needed to
/// build or read a <see cref="Horoscope"/> with its preferred <see cref="HoroscopeDateTime"/>. No calculation
/// happens here beyond the date/time to Julian Day conversion, and no IANA timezone is guessed from a QCK
/// abbreviation.
/// </summary>
public static class QckMapper
{
    /// <summary>Data needed to create a chart with a preferred date/time from one imported QCK record.</summary>
    public sealed record MappedChart(
        string Name,
        string? PlaceName,
        double Latitude,
        double Longitude,
        double JulianDate,
        string OriginalInput);

    // ── Import ───────────────────────────────────────────────────────────────

    public static (MappedChart Chart, List<ExchangeMessage> Messages) ToMappedChart(QckRecord record, int recordNumber)
    {
        var messages = new List<ExchangeMessage>();

        var localDate = new AstronomicalDate(record.Year, record.Month, record.Day, Gregorian: true);
        var localTime = new AstronomicalTime(record.Hour, record.Minute, record.Second);
        var offsetMinutes = record.EffectiveUtcOffsetMinutes;

        if (record.IsLmt)
        {
            var longitudeDerivedMinutes = record.Longitude / 15.0 * 60.0;
            if (Math.Abs(longitudeDerivedMinutes - offsetMinutes) > 2.0)
                messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgQckLmtMismatch, recordNumber, "lmt",
                    offsetMinutes, longitudeDerivedMinutes.ToString("F1", CultureInfo.CurrentCulture)));
        }
        else if (record.TimeZoneAbbreviation.Length > 0)
        {
            messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgQckTzAbbreviation, recordNumber, "timezone",
                record.TimeZoneAbbreviation));
        }

        var julianDate = ExchangeDateTime.LocalToUtJulianDay(localDate, localTime, offsetMinutes * 60);

        var chart = new MappedChart(
            record.Name,
            record.Place.Length == 0 ? null : record.Place,
            record.Latitude,
            record.Longitude,
            julianDate,
            OriginalInputText(record));
        return (chart, messages);
    }

    private static string OriginalInputText(QckRecord record)
    {
        var monthText = QckLineParser.Months.FirstOrDefault(m => m.Value == record.Month).Key ?? record.Month.ToString();
        var hour12 = record.Hour % 12;
        if (hour12 == 0) hour12 = 12;
        var timeText = $"{hour12:00}:{record.Minute:00}:{record.Second:00} {(record.IsPm ? "PM" : "AM")}";
        var sign = record.QckCorrectionMinutes < 0 ? "-" : "+";
        var magnitude = Math.Abs(record.QckCorrectionMinutes);
        var correctionText = $"QCK {sign}{magnitude / 60:00}:{magnitude % 60:00}";
        var tzPart = record.TimeZoneAbbreviation.Length == 0 ? "" : $" {record.TimeZoneAbbreviation}";
        return $"{record.Day} {monthText} {record.Year}, {timeText}{tzPart} ({correctionText})";
    }

    // ── Export ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the QCK record for one chart. Enigma stores only the resolved Julian Day (UT) plus an IANA
    /// timezone identifier used for display, never a separate standard/DST offset. QCK needs a fixed local
    /// offset, so export always writes the time as UT: the moment in time is preserved exactly, only the local
    /// display/timezone identity is not.
    /// </summary>
    public static (QckRecord Record, List<ExchangeMessage> Messages) ToRecord(
        string name, string? placeName, double latitude, double longitude,
        double julianDate, string timeZoneIdentifier, int recordNumber)
    {
        var messages = new List<ExchangeMessage>();
        if (timeZoneIdentifier != "UTC")
            messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgQckTzNotPreserved, recordNumber, "timezone",
                timeZoneIdentifier));

        var dateTime = ExchangeDateTime.UtFromJulianDay(julianDate);
        var record = new QckRecord
        {
            Name = name,
            Month = dateTime.Date.Month,
            Day = dateTime.Date.Day,
            Year = dateTime.Date.Year,
            Hour = dateTime.Time.Hour,
            Minute = dateTime.Time.Minute,
            Second = dateTime.Time.Second,
            IsPm = dateTime.Time.Hour >= 12,
            TimeZoneAbbreviation = "",
            QckCorrectionMinutes = 0,
            Longitude = longitude,
            Latitude = latitude,
            Place = placeName ?? ""
        };
        return (record, messages);
    }
}
