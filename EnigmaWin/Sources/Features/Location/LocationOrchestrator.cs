// LocationOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Location;

/// <summary>
/// Single public entry point for all geographic and timezone lookups.
///
/// Typical usage:
/// <code>
/// var orch = new LocationOrchestrator();
///
/// // Populate a country picker
/// var countries = orch.AllCountries("en");
///
/// // After the user picks a country, list matching cities
/// var cities = orch.Cities("NL", "A*");
///
/// // After the user picks a city, resolve the UTC offset for the birth date/time
/// var zone = orch.TimezoneInfo(city.TimezoneName, dateTime);
/// // zone.OffsetSeconds → total UTC offset including DST
/// </code>
/// </summary>
public sealed class LocationOrchestrator : System.IDisposable
{
    private readonly LocationDb _db;

    public LocationOrchestrator()
    {
        _db = new LocationDb();
    }

    public void Dispose() => _db.Dispose();

    // ── Country lookup ─────────────────────────────────────────────────────────

    /// <summary>Returns all countries sorted by localised name for the given language code
    /// (e.g. "en", "nl", "de", "fr"). Falls back to English when the language has no entries.</summary>
    public IReadOnlyList<LocationCountry> AllCountries(string lang) =>
        _db.AllCountries(lang);

    /// <summary>Returns countries whose localised name matches the pattern (wildcard: <c>*</c>),
    /// for the given language code.</summary>
    public IReadOnlyList<LocationCountry> Countries(string lang, string pattern) =>
        _db.Countries(lang, pattern);

    // ── City lookup ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns cities for the given country, optionally filtered by a name pattern.
    /// <para>Only <c>*</c> is supported as a wildcard. An empty string or bare <c>*</c> returns all cities
    /// for the country (no limit). Any other pattern is capped at 50 results.
    /// The match is case-insensitive.</para>
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 code, e.g. "NL".</param>
    /// <param name="pattern">Name filter. Examples: "A*" (starts with A), "*dam", "*ster*".</param>
    public IReadOnlyList<LocationCity> Cities(string countryCode, string pattern = "*") =>
        _db.Cities(countryCode, pattern);

    // ── Timezone resolution ────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the full UTC offset (including DST) and timezone name for the given local date/time.
    /// </summary>
    /// <param name="tzName">IANA timezone identifier, e.g. "Europe/Amsterdam".
    /// Obtained from <see cref="LocationCity.TimezoneName"/>.</param>
    /// <param name="dateTime">Local date and time for which the offset should be calculated.</param>
    /// <param name="longitude">Optional geographic longitude (decimal degrees, East positive).
    /// Supply this to enable LMT (Local Mean Time) support for historical dates.</param>
    /// <returns>A <see cref="ZoneInfo"/> with <c>OffsetSeconds</c>, <c>TzName</c>,
    /// <c>DstUsed</c>, <c>IsInvalid</c>, and <c>IsAmbiguous</c>.</returns>
    public ZoneInfo TimezoneInfo(string tzName, AstronomicalDateTime dateTime, double? longitude = null)
    {
        var resolver = new TzResolver(_db);
        return resolver.Resolve(dateTime, tzName, longitude);
    }
}
