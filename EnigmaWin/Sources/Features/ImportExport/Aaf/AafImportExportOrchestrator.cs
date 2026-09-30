// AafImportExportOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWin.Sources.Features.ImportExport.Aaf;

/// <summary>
/// Imports and exports charts in the AAF'97 format. Every AAF record becomes a <see cref="Horoscope"/>
/// (see <see cref="AafMapper"/> for why events are not created). A problem in one record never aborts the
/// whole file. Enigma's own chart id round-trips via the custom "#ENID" chunk, so import skips a record whose
/// id already exists locally; a record without an id is always imported as a new chart.
/// </summary>
public sealed class AafImportExportOrchestrator(IHoroscopeRepository horoscopeRepository)
{
    // ── Import ───────────────────────────────────────────────────────────────

    public async Task<FormatImportResult> ImportAsync(byte[] data)
    {
        var result = new FormatImportResult();

        var decoded = LegacyTextEncoding.DecodePreferringUtf8(data);
        result.Messages.AddRange(decoded.Messages);

        var parseResult = AafRecordParser.Parse(decoded.Content);
        result.Messages.AddRange(parseResult.Messages);

        var existingIds = (await horoscopeRepository.FetchAllAsync()).Select(h => h.Id).ToHashSet();

        for (var index = 0; index < parseResult.Records.Count; index++)
        {
            var recordNumber = index + 1;
            var (mapped, messages) = AafMapper.ToMappedChart(parseResult.Records[index], recordNumber);
            result.Messages.AddRange(messages);
            if (mapped is null) continue;

            if (mapped.Id is { } id && existingIds.Contains(id))
            {
                result.ChartsSkipped++;
                continue;
            }

            var horoscope = new Horoscope
            {
                Id = mapped.Id ?? Guid.NewGuid(),
                Name = mapped.Name,
                Category = mapped.Category,
                Notes = mapped.Notes,
                Source = mapped.Source,
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
                existingIds.Add(horoscope.Id);
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
    /// Exports all charts as AAF'97 records, UTF-8 encoded, separated by a blank line.
    /// </summary>
    public async Task<FormatExportResult> ExportAsync()
    {
        var messages = new List<ExchangeMessage>();
        var horoscopes = await horoscopeRepository.FetchAllAsync();

        var blocks = new List<string>();
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

            var (record, mapMessages) = AafMapper.ToRecord(horoscope.Id, horoscope.Name, horoscope.Category,
                horoscope.Source, horoscope.Notes, horoscope.PlaceName, latitude, longitude,
                dateTime.JulianDate, dateTime.TimeZoneIdentifier, recordNumber);
            messages.AddRange(mapMessages);

            var (text, writeMessages) = AafWriter.Write(record, recordNumber);
            messages.AddRange(writeMessages);
            blocks.Add(text);
        }

        var content = blocks.Count == 0 ? "" : string.Join("\n\n", blocks) + "\n";
        return new FormatExportResult
        {
            Data = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(content),
            ChartsExported = blocks.Count,
            Messages = messages
        };
    }
}
