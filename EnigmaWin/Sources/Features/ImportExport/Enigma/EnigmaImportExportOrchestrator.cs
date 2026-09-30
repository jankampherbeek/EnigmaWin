// EnigmaImportExportOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EnigmaWin.Sources.Data.Event;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWin.Sources.Features.ImportExport.Enigma;

/// <summary>
/// Builds and applies the Enigma JSON import/export format for charts and events. Import skips any chart or
/// event whose id already exists locally, leaving the local record untouched. An event must be linked to at
/// least one chart in EnigmaWin, so an event whose charts are neither in the file nor in the database is
/// skipped with an error.
/// </summary>
public sealed class EnigmaImportExportOrchestrator(
    IHoroscopeRepository horoscopeRepository,
    IEventRepository eventRepository)
{
    // ── Export ───────────────────────────────────────────────────────────────

    public async Task<FormatExportResult> ExportAsync()
    {
        var horoscopes = (await horoscopeRepository.FetchAllAsync()).ToList();
        var events = (await eventRepository.FetchAllAsync()).ToList();

        var file = new EnigmaExportFile
        {
            FormatVersion = EnigmaExportFile.CurrentFormatVersion,
            ExportedAt = DateTimeOffset.UtcNow,
            Charts = horoscopes.Select(h => new EnigmaChartDto
            {
                Id = h.Id,
                Name = h.Name,
                Category = h.Category,
                Notes = h.Notes,
                Source = h.Source,
                RoddenRating = h.RoddenRating.ToString(),
                PlaceName = h.PlaceName,
                Latitude = h.Latitude,
                Longitude = h.Longitude,
                DateTimes = h.DateTimes.Select(dt => new EnigmaDateTimeDto
                {
                    Id = dt.Id,
                    JulianDate = dt.JulianDate,
                    TimeZoneIdentifier = dt.TimeZoneIdentifier,
                    TimeIsUnknown = dt.TimeIsUnknown,
                    IsPreferred = dt.IsPreferred,
                    Label = dt.Label,
                    OriginalInput = dt.OriginalInput
                }).ToList()
            }).ToList(),
            Events = events.Select(e => new EnigmaEventDto
            {
                Id = e.Id,
                Title = e.Title,
                EventDescription = e.EventDescription,
                JulianDate = e.JulianDate,
                TimeZoneIdentifier = e.TimeZoneIdentifier,
                OriginalInput = e.OriginalInput,
                PlaceName = e.PlaceName,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                ChartIds = e.HoroscopeIds.ToList()
            }).ToList()
        };

        return new FormatExportResult
        {
            Data = JsonSerializer.SerializeToUtf8Bytes(file, EnigmaExportFile.JsonOptions),
            ChartsExported = file.Charts.Count,
            EventsExported = file.Events.Count
        };
    }

    // ── Import ───────────────────────────────────────────────────────────────

    public async Task<FormatImportResult> ImportAsync(byte[] data)
    {
        var result = new FormatImportResult();

        EnigmaExportFile? file;
        try
        {
            file = JsonSerializer.Deserialize<EnigmaExportFile>(data, EnigmaExportFile.JsonOptions);
        }
        catch (Exception ex) when (ex is JsonException or FormatException)
        {
            result.Messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgEnigmaInvalidFile, null, null, ex.Message));
            return result;
        }
        if (file is null)
        {
            result.Messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgEnigmaInvalidFile, null, null, ""));
            return result;
        }
        if (file.FormatVersion > EnigmaExportFile.CurrentFormatVersion)
        {
            result.Messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgEnigmaUnsupportedVersion, null, null,
                file.FormatVersion, EnigmaExportFile.CurrentFormatVersion));
            return result;
        }

        var chartIds = (await horoscopeRepository.FetchAllAsync()).Select(h => h.Id).ToHashSet();
        var eventIds = (await eventRepository.FetchAllAsync()).Select(e => e.Id).ToHashSet();

        foreach (var (chartDto, index) in file.Charts.Select((c, i) => (c, i)))
        {
            if (chartIds.Contains(chartDto.Id))
            {
                result.ChartsSkipped++;
                continue;
            }
            try
            {
                await horoscopeRepository.AddAsync(ToHoroscope(chartDto));
                // Add the non-preferred date/times first: adding a preferred one clears the flag on the others.
                foreach (var dtDto in chartDto.DateTimes.OrderBy(dt => dt.IsPreferred))
                    await horoscopeRepository.AddDateTimeAsync(chartDto.Id, ToDateTime(dtDto, chartDto.Id));
                chartIds.Add(chartDto.Id);
                result.ChartsImported++;
            }
            catch (Exception ex)
            {
                result.Messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgSaveFailed, index + 1, null, ex.Message));
            }
        }

        foreach (var (eventDto, index) in file.Events.Select((e, i) => (e, i)))
        {
            if (eventIds.Contains(eventDto.Id))
            {
                result.EventsSkipped++;
                continue;
            }
            var linkedIds = eventDto.ChartIds.Where(chartIds.Contains).Distinct().ToList();
            if (linkedIds.Count == 0)
            {
                result.Messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgEnigmaEventNoCharts, null, "chartIds",
                    eventDto.Title));
                continue;
            }
            try
            {
                await eventRepository.AddAsync(ToChartEvent(eventDto, linkedIds));
                eventIds.Add(eventDto.Id);
                result.EventsImported++;
            }
            catch (Exception ex)
            {
                result.Messages.Add(ExchangeMessage.Fatal(ImportExportKeys.MsgSaveFailed, index + 1, null, ex.Message));
            }
        }
        return result;
    }

    private static Horoscope ToHoroscope(EnigmaChartDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Category = dto.Category,
        Notes = dto.Notes,
        Source = dto.Source,
        RoddenRating = Enum.TryParse<RoddenRating>(dto.RoddenRating, ignoreCase: true, out var rating)
            ? rating
            : RoddenRating.None,
        PlaceName = dto.PlaceName,
        Latitude = dto.Latitude,
        Longitude = dto.Longitude
    };

    private static HoroscopeDateTime ToDateTime(EnigmaDateTimeDto dto, Guid horoscopeId) => new()
    {
        Id = dto.Id,
        HoroscopeId = horoscopeId,
        JulianDate = dto.JulianDate,
        TimeZoneIdentifier = dto.TimeZoneIdentifier,
        TimeIsUnknown = dto.TimeIsUnknown,
        IsPreferred = dto.IsPreferred,
        Label = dto.Label,
        OriginalInput = dto.OriginalInput
    };

    private static ChartEvent ToChartEvent(EnigmaEventDto dto, IReadOnlyList<Guid> horoscopeIds) => new()
    {
        Id = dto.Id,
        Title = dto.Title,
        EventDescription = dto.EventDescription,
        JulianDate = dto.JulianDate,
        TimeZoneIdentifier = dto.TimeZoneIdentifier,
        OriginalInput = dto.OriginalInput,
        PlaceName = dto.PlaceName,
        Latitude = dto.Latitude,
        Longitude = dto.Longitude,
        HoroscopeIds = horoscopeIds
    };
}
