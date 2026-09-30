// QckImportExportOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWin.Sources.Features.ImportExport.Qck;

/// <summary>
/// Imports and exports charts in the QCK format. Charts are the only thing QCK can represent (no events).
/// A problem in one record never aborts the whole file: it is reported as a warning or fatal message scoped
/// to that record, and the remaining records are still processed.
/// </summary>
public sealed class QckImportExportOrchestrator(IHoroscopeRepository horoscopeRepository)
{
    // ── Import ───────────────────────────────────────────────────────────────

    public async Task<FormatImportResult> ImportAsync(byte[] data)
    {
        var result = new FormatImportResult();

        var decoded = LegacyTextEncoding.Decode(data);
        result.Messages.AddRange(decoded.Messages);

        var parseResult = QckFileParser.Parse(decoded.Content);
        result.Messages.AddRange(parseResult.Messages);

        for (var index = 0; index < parseResult.Records.Count; index++)
        {
            var recordNumber = index + 1;
            var (mapped, messages) = QckMapper.ToMappedChart(parseResult.Records[index], recordNumber);
            result.Messages.AddRange(messages);

            var horoscope = new Horoscope
            {
                Name = mapped.Name,
                PlaceName = mapped.PlaceName,
                Latitude = mapped.Latitude,
                Longitude = mapped.Longitude
            };
            var dateTime = new HoroscopeDateTime
            {
                HoroscopeId = horoscope.Id,
                JulianDate = mapped.JulianDate,
                TimeZoneIdentifier = "UTC",
                IsPreferred = true,
                OriginalInput = mapped.OriginalInput
            };

            try
            {
                await horoscopeRepository.AddAsync(horoscope);
                await horoscopeRepository.AddDateTimeAsync(horoscope.Id, dateTime);
                result.ChartsImported++;
            }
            catch (Exception ex)
            {
                result.Messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgSaveFailed, recordNumber, null, ex.Message));
            }
        }
        return result;
    }

    // ── Export ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Exports all charts as QCK records, one per line, CRLF-terminated (the traditional line ending for this
    /// DOS-era format), encoded as Windows-1252.
    /// </summary>
    public async Task<FormatExportResult> ExportAsync()
    {
        var messages = new List<ExchangeMessage>();
        var horoscopes = await horoscopeRepository.FetchAllAsync();

        var lines = new List<string>();
        var recordNumber = 0;
        foreach (var horoscope in horoscopes)
        {
            var dateTime = horoscope.DateTimes.FirstOrDefault(dt => dt.IsPreferred) ?? horoscope.DateTimes.FirstOrDefault();
            if (dateTime is null)
            {
                messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgNoDateTime, null, "dateTime", horoscope.Name));
                continue;
            }
            if (horoscope.Latitude is not { } latitude || horoscope.Longitude is not { } longitude)
            {
                messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgNoCoordinates, null, "coordinates", horoscope.Name));
                continue;
            }
            recordNumber++;

            var (record, mapMessages) = QckMapper.ToRecord(horoscope.Name, horoscope.PlaceName, latitude, longitude,
                dateTime.JulianDate, dateTime.TimeZoneIdentifier, recordNumber);
            messages.AddRange(mapMessages);

            var (line, writeMessages) = QckWriter.Write(record, recordNumber);
            messages.AddRange(writeMessages);
            if (line is not null) lines.Add(line);
        }

        var text = lines.Count == 0 ? "" : string.Join("\r\n", lines) + "\r\n";
        var (data, hadLoss) = LegacyTextEncoding.Encode(text);
        if (hadLoss)
            messages.Add(ExchangeMessage.Warning(ImportExportKeys.MsgEncodingLoss, null, null));

        return new FormatExportResult { Data = data, ChartsExported = lines.Count, Messages = messages };
    }
}
