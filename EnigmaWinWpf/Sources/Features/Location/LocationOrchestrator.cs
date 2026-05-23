// LocationOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Location;

/// <summary>Single public entry point for all geographic and timezone lookups.</summary>
public sealed class LocationOrchestrator : System.IDisposable
{
    private readonly LocationDb _db;

    public LocationOrchestrator()
    {
        _db = new LocationDb();
    }

    public void Dispose() => _db.Dispose();

    public IReadOnlyList<LocationCountry> AllCountries(string lang) =>
        _db.AllCountries(lang);

    public IReadOnlyList<LocationCountry> Countries(string lang, string pattern) =>
        _db.Countries(lang, pattern);

    public IReadOnlyList<LocationCity> Cities(string countryCode, string pattern = "*") =>
        _db.Cities(countryCode, pattern);

    public ZoneInfo TimezoneInfo(string tzName, AstronomicalDateTime dateTime, double? longitude = null)
    {
        var resolver = new TzResolver(_db);
        return resolver.Resolve(dateTime, tzName, longitude);
    }
}
