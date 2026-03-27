// IEventRepository.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Data.Event;

public interface IEventRepository
{
    Task<IEnumerable<ChartEvent>> FetchAllAsync();
    Task<ChartEvent?> FetchAsync(Guid id);
    Task AddAsync(ChartEvent chartEvent);
    Task UpdateAsync(ChartEvent chartEvent);
    Task DeleteAsync(Guid id);

    Task LinkAsync(Guid horoscopeId, Guid eventId);

    /// <exception cref="InvalidOperationException">When the event has only one linked horoscope.</exception>
    Task UnlinkAsync(Guid horoscopeId, Guid eventId);
}
