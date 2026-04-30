// ResearchInputRecord.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Research.Pipeline;

/// <summary>One row from the horoscope_data SQLite table — represents a single chart to calculate.</summary>
/// <remarks>IsData = true means real research data; false means control-group data.</remarks>
public sealed record ResearchInputRecord(
    int Id,
    bool IsData,
    double GeoLat,
    double GeoLon,
    int Year,
    int Month,
    int Day,
    int Hour,
    int Minute,
    int Second,
    /// <summary>UTC offset in decimal hours (e.g. +2.0 for CEST).</summary>
    double Offset);
