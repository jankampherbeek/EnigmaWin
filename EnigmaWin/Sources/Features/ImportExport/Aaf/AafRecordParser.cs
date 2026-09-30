// AafRecordParser.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWin.Sources.Features.ImportExport.Aaf;

/// <summary>
/// Parses an AAF'97 file into <see cref="AafRecord"/>s. A new "#A93:" chunk starts a new record and
/// implicitly ends the previous one. Unknown chunks and "#:" comment lines never abort parsing; a fatal error
/// in one record does not prevent the remaining records from being parsed.
/// </summary>
public static class AafRecordParser
{
    private sealed class Builder(string a93Raw)
    {
        public string A93Raw { get; } = a93Raw;
        public string? B93Raw { get; set; }
        public string? ZoneName { get; set; }
        public string? Source { get; set; }
        public string? Via { get; set; }
        public string? Comment { get; set; }
        public string? EnigmaId { get; set; }
        public List<string> UnknownChunkNames { get; } = [];
    }

    public static FormatParseResult<AafRecord> Parse(string content)
    {
        var result = new FormatParseResult<AafRecord>();
        var lines = content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        Builder? builder = null;
        var recordNumber = 0;

        void FinalizeIfNeeded()
        {
            if (builder is null) return;
            var (record, messages) = Finalize(builder, recordNumber);
            result.Messages.AddRange(messages);
            if (record is not null) result.Records.Add(record);
            builder = null;
        }

        foreach (var line in lines)
        {
            if (!line.StartsWith('#')) continue;
            var colonIndex = line.IndexOf(':', 1);
            if (colonIndex < 0) continue;
            var tag = line[1..colonIndex].ToUpperInvariant();
            var rest = line[(colonIndex + 1)..];

            if (tag.Length == 0) continue; // "#: comment"

            if (tag == "A93")
            {
                FinalizeIfNeeded();
                recordNumber++;
                builder = new Builder(rest);
                continue;
            }

            if (builder is null)
            {
                result.Messages.Add(IsKnownChunk(tag)
                    ? ExchangeMessage.Warning(ImportExportKeys.MsgAafChunkBeforeA93, null, null, tag)
                    : ExchangeMessage.Warning(ImportExportKeys.MsgAafUnknownChunk, null, null, tag));
                continue;
            }

            switch (tag)
            {
                case "B93":  builder.B93Raw   = rest; break;
                case "ZNAM": builder.ZoneName = rest; break;
                case "SRC":  builder.Source   = rest; break;
                case "VIA":  builder.Via      = rest; break;
                case "COM":  builder.Comment  = rest; break;
                case "ENID": builder.EnigmaId = rest; break;
                default:
                    builder.UnknownChunkNames.Add(tag);
                    result.Messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgAafUnknownChunk, recordNumber, null, tag));
                    break;
            }
        }
        FinalizeIfNeeded();
        return result;
    }

    private static bool IsKnownChunk(string tag) => tag is "B93" or "ZNAM" or "SRC" or "VIA" or "COM" or "ENID";

    private static (AafRecord? Record, List<ExchangeMessage> Messages) Finalize(Builder builder, int recordNumber)
    {
        var messages = new List<ExchangeMessage>();

        var a93 = builder.A93Raw.Split(',').Select(f => f.Trim()).ToArray();
        if (a93.Length != 7)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafA93FieldCount, recordNumber, null, a93.Length));
            return (null, messages);
        }

        var date = AafFieldParsers.ParseDate(a93[3], recordNumber, messages);
        if (date is null) return (null, messages);
        var time = AafFieldParsers.ParseTime(a93[4], recordNumber, messages);
        if (time is null) return (null, messages);

        if (builder.B93Raw is null)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafNoB93, recordNumber, null));
            return (null, messages);
        }
        var b93 = builder.B93Raw.Split(',').Select(f => f.Trim()).ToArray();
        if (b93.Length != 5)
        {
            messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgAafB93FieldCount, recordNumber, null, b93.Length));
            return (null, messages);
        }

        var latitude = AafFieldParsers.ParseCoordinate(b93[1], 'n', 's', isLongitude: false, recordNumber, "latitude", messages);
        if (latitude is null) return (null, messages);
        var longitude = AafFieldParsers.ParseCoordinate(b93[2], 'e', 'w', isLongitude: true, recordNumber, "longitude", messages);
        if (longitude is null) return (null, messages);
        var (offsetOk, greenwichOffsetSeconds) = AafFieldParsers.ParseGreenwichOffset(b93[3], recordNumber, messages);
        if (!offsetOk) return (null, messages);

        Guid? enigmaId = null;
        if (builder.EnigmaId is not null)
        {
            if (Guid.TryParse(builder.EnigmaId.Trim(), out var id)) enigmaId = id;
            else messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgAafInvalidEnid, recordNumber, "enid", builder.EnigmaId));
        }

        var record = new AafRecord
        {
            LastName = a93[0],
            FirstName = a93[1],
            Type = a93[2].ToLowerInvariant(),
            Day = date.Value.Day,
            Month = date.Value.Month,
            Year = date.Value.Year,
            IsGregorian = date.Value.IsGregorian,
            Hour = time.Value.Hour,
            Minute = time.Value.Minute,
            Second = time.Value.Second,
            Place = a93[5],
            Country = a93[6],
            JulianDayRaw = b93[0],
            Latitude = latitude.Value,
            Longitude = longitude.Value,
            GreenwichOffsetSeconds = greenwichOffsetSeconds,
            TimeType = b93[4].ToLowerInvariant(),
            ZoneName = builder.ZoneName,
            Source = builder.Source,
            Via = builder.Via,
            Comment = builder.Comment,
            UnknownChunkNames = builder.UnknownChunkNames,
            EnigmaId = enigmaId
        };
        return (record, messages);
    }
}
