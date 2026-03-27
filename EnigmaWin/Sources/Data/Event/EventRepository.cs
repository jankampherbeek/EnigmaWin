// EventRepository.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EnigmaWin.Sources.Data.Db;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Data.Event;

internal sealed class EventRepository(IDbConnectionFactory factory) : IEventRepository
{
    public async Task<IEnumerable<ChartEvent>> FetchAllAsync()
    {
        using var conn = factory.Create();
        var events = await conn.QueryAsync<ChartEvent>(
            "SELECT * FROM ChartEvent ORDER BY JulianDate");
        return events;
    }

    public async Task<ChartEvent?> FetchAsync(Guid id)
    {
        using var conn = factory.Create();
        var chartEvent = await conn.QuerySingleOrDefaultAsync<ChartEvent>(
            "SELECT * FROM ChartEvent WHERE Id = @Id", new { Id = id });
        if (chartEvent is null) return null;

        var horoscopeIds = await conn.QueryAsync<Guid>(
            "SELECT HoroscopeId FROM HoroscopeEvent WHERE EventId = @Id", new { Id = id });
        return chartEvent with { HoroscopeIds = horoscopeIds.ToList() };
    }

    public async Task AddAsync(ChartEvent chartEvent)
    {
        if (chartEvent.HoroscopeIds.Count == 0)
            throw new InvalidOperationException("An event must be linked to at least one horoscope.");

        using var conn = factory.Create();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync("""
            INSERT INTO ChartEvent
                (Id, Title, EventDescription, JulianDate, TimeZoneIdentifier, OriginalInput, PlaceName, Latitude, Longitude)
            VALUES
                (@Id, @Title, @EventDescription, @JulianDate, @TimeZoneIdentifier, @OriginalInput, @PlaceName, @Latitude, @Longitude)
            """, chartEvent, tx);

        foreach (var horoscopeId in chartEvent.HoroscopeIds)
            await conn.ExecuteAsync(
                "INSERT OR IGNORE INTO HoroscopeEvent (HoroscopeId, EventId) VALUES (@HoroscopeId, @EventId)",
                new { HoroscopeId = horoscopeId, EventId = chartEvent.Id }, tx);

        tx.Commit();
    }

    public async Task UpdateAsync(ChartEvent chartEvent)
    {
        using var conn = factory.Create();
        await conn.ExecuteAsync("""
            UPDATE ChartEvent SET
                Title              = @Title,
                EventDescription   = @EventDescription,
                JulianDate         = @JulianDate,
                TimeZoneIdentifier = @TimeZoneIdentifier,
                OriginalInput      = @OriginalInput,
                PlaceName          = @PlaceName,
                Latitude           = @Latitude,
                Longitude          = @Longitude
            WHERE Id = @Id
            """, chartEvent);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var conn = factory.Create();
        await conn.ExecuteAsync("DELETE FROM ChartEvent WHERE Id = @Id", new { Id = id });
    }

    public async Task LinkAsync(Guid horoscopeId, Guid eventId)
    {
        using var conn = factory.Create();
        await conn.ExecuteAsync(
            "INSERT OR IGNORE INTO HoroscopeEvent (HoroscopeId, EventId) VALUES (@HoroscopeId, @EventId)",
            new { HoroscopeId = horoscopeId, EventId = eventId });
    }

    public async Task UnlinkAsync(Guid horoscopeId, Guid eventId)
    {
        using var conn = factory.Create();
        using var tx = conn.BeginTransaction();

        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM HoroscopeEvent WHERE EventId = @EventId",
            new { EventId = eventId }, tx);

        if (count <= 1)
            throw new InvalidOperationException("An event must remain linked to at least one horoscope.");

        await conn.ExecuteAsync(
            "DELETE FROM HoroscopeEvent WHERE HoroscopeId = @HoroscopeId AND EventId = @EventId",
            new { HoroscopeId = horoscopeId, EventId = eventId }, tx);

        tx.Commit();
    }
}
