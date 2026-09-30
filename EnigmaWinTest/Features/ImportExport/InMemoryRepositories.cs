// InMemoryRepositories.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Data.Event;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;

namespace EnigmaWintest.Features.ImportExport;

/// <summary>In-memory stand-in for the horoscope repository, for import/export orchestrator tests.</summary>
internal sealed class InMemoryHoroscopeRepository : IHoroscopeRepository
{
    private readonly List<Horoscope> _horoscopes = [];

    public Task<IEnumerable<Horoscope>> FetchAllAsync() =>
        Task.FromResult<IEnumerable<Horoscope>>(_horoscopes.OrderBy(h => h.Name).ToList());

    public Task<Horoscope?> FetchAsync(Guid id) => Task.FromResult(_horoscopes.FirstOrDefault(h => h.Id == id));

    public Task AddAsync(Horoscope horoscope)
    {
        if (_horoscopes.Any(h => h.Id == horoscope.Id)) throw new InvalidOperationException("Duplicate id.");
        _horoscopes.Add(horoscope with { DateTimes = [] });
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Horoscope horoscope) => throw new NotSupportedException();
    public Task DeleteAsync(Guid id) => throw new NotSupportedException();

    public Task AddDateTimeAsync(Guid horoscopeId, HoroscopeDateTime dateTime)
    {
        var index = _horoscopes.FindIndex(h => h.Id == horoscopeId);
        var existing = _horoscopes[index].DateTimes
            .Select(dt => dateTime.IsPreferred ? dt with { IsPreferred = false } : dt);
        _horoscopes[index] = _horoscopes[index] with
        {
            DateTimes = existing.Append(dateTime with { HoroscopeId = horoscopeId }).ToList()
        };
        return Task.CompletedTask;
    }

    public Task SetPreferredAsync(Guid dateTimeId, Guid horoscopeId) => throw new NotSupportedException();
    public Task UpdateDateTimeAsync(HoroscopeDateTime dateTime) => throw new NotSupportedException();
    public Task DeleteDateTimeAsync(Guid dateTimeId) => throw new NotSupportedException();
}

/// <summary>In-memory stand-in for the event repository, for import/export orchestrator tests.</summary>
internal sealed class InMemoryEventRepository : IEventRepository
{
    private readonly List<ChartEvent> _events = [];

    public Task<IEnumerable<ChartEvent>> FetchAllAsync() => Task.FromResult<IEnumerable<ChartEvent>>(_events.ToList());
    public Task<ChartEvent?> FetchAsync(Guid id) => Task.FromResult(_events.FirstOrDefault(e => e.Id == id));

    public Task AddAsync(ChartEvent chartEvent)
    {
        if (chartEvent.HoroscopeIds.Count == 0)
            throw new InvalidOperationException("An event must be linked to at least one horoscope.");
        _events.Add(chartEvent);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ChartEvent chartEvent) => throw new NotSupportedException();
    public Task DeleteAsync(Guid id) => throw new NotSupportedException();
    public Task LinkAsync(Guid horoscopeId, Guid eventId) => throw new NotSupportedException();
    public Task UnlinkAsync(Guid horoscopeId, Guid eventId) => throw new NotSupportedException();
}
