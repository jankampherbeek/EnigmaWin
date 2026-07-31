// LongTimeEphemerisRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Periods.LongTimeEphemeris;

/// <summary>Which single coordinate to display in the long time ephemeris.</summary>
public enum LongTimeEphemerisCoordinate
{
    Longitude, Latitude, RightAscension, Declination, Distance, SpeedLongitude, SpeedDeclination
}

/// <summary>How to format calculated values in the results table and CSV export.</summary>
public enum LongTimeEphemerisDisplayFormat
{
    Dms, Decimal
}

/// <summary>One row of results: a single moment in time and the requested coordinate for every selected factor.</summary>
public sealed class LongTimeEphemerisRow(int id, double julianDay, string dateTimeText, IReadOnlyDictionary<Factors, double> values)
{
    public int Id { get; } = id;
    public double JulianDay { get; } = julianDay;
    public string DateTimeText { get; } = dateTimeText;
    public IReadOnlyDictionary<Factors, double> Values { get; } = values;
}
