// EventsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnigmaWin.Sources.Data.Event;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Progressive.Events;

/// <summary>
/// Orchestrates create / read / update / delete operations on ChartEvent,
/// including the many-to-many relationship with Horoscope.
/// </summary>
public sealed class EventsOrchestrator(IEventRepository repository)
{
    // MARK: - Create

    /// <summary>
    /// Creates a new ChartEvent, persists it, and links it to the given horoscope.
    /// </summary>
    public async Task<ChartEvent> CreateAsync(EventValues values, Guid horoscopeId)
    {
        var chartEvent = new ChartEvent
        {
            Title              = values.Title,
            EventDescription   = values.EventDescription,
            JulianDate         = values.JulianDate,
            TimeZoneIdentifier = values.TimeZoneIdentifier,
            OriginalInput      = values.OriginalInput,
            PlaceName          = values.PlaceName,
            Latitude           = values.Latitude,
            Longitude          = values.Longitude,
            HoroscopeIds       = [horoscopeId]
        };
        await repository.AddAsync(chartEvent);
        return chartEvent;
    }

    // MARK: - Read

    /// <summary>
    /// Returns all events linked to the given horoscope, sorted by Julian Date.
    /// </summary>
    public async Task<IReadOnlyList<ChartEvent>> EventsForHoroscopeAsync(Guid horoscopeId)
    {
        var all = await repository.FetchAllAsync();
        return all
            .Where(e => e.HoroscopeIds.Contains(horoscopeId))
            .OrderBy(e => e.JulianDate)
            .ToList();
    }

    // MARK: - Update values

    /// <summary>
    /// Overwrites all scalar fields of an existing event with the supplied values.
    /// </summary>
    public async Task UpdateValuesAsync(ChartEvent chartEvent, EventValues values)
    {
        var updated = chartEvent with
        {
            Title              = values.Title,
            EventDescription   = values.EventDescription,
            JulianDate         = values.JulianDate,
            TimeZoneIdentifier = values.TimeZoneIdentifier,
            OriginalInput      = values.OriginalInput,
            PlaceName          = values.PlaceName,
            Latitude           = values.Latitude,
            Longitude          = values.Longitude
        };
        await repository.UpdateAsync(updated);
    }

    // MARK: - Link / Unlink

    /// <summary>
    /// Links the event to an additional horoscope. No-op if already linked.
    /// </summary>
    public async Task LinkHoroscopeAsync(Guid horoscopeId, Guid eventId)
    {
        await repository.LinkAsync(horoscopeId, eventId);
    }

    /// <summary>
    /// Removes the link between an event and a horoscope.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the event has only one linked horoscope.</exception>
    public async Task UnlinkHoroscopeAsync(Guid horoscopeId, Guid eventId)
    {
        await repository.UnlinkAsync(horoscopeId, eventId);
    }

    // MARK: - Delete

    /// <summary>Deletes the event.</summary>
    public async Task DeleteAsync(Guid eventId)
    {
        await repository.DeleteAsync(eventId);
    }

    /// <summary>
    /// Deletes every event that is linked exclusively to the given horoscope.
    /// Events shared with other horoscopes are left untouched.
    /// Call this before deleting a horoscope to avoid leaving orphaned events.
    /// </summary>
    public async Task DeleteOrphansAsync(Guid horoscopeId)
    {
        var events = await EventsForHoroscopeAsync(horoscopeId);
        var orphans = events.Where(e => e.HoroscopeIds.Count == 1);
        foreach (var orphan in orphans)
            await repository.DeleteAsync(orphan.Id);
    }
}
