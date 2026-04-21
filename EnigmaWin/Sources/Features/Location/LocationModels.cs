// LocationModels.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Location;

/// <summary>A country entry from the database.</summary>
public sealed record LocationCountry(string Code, string Name, string Continent);

/// <summary>A city entry from the database, including its resolved IANA timezone name.</summary>
public sealed record LocationCity(
    int Id,
    string CountryCode,
    string Name,
    double Latitude,
    double Longitude,
    string? RegionCode,
    int? Elevation,
    int TimezoneId,
    string TimezoneName);

/// <summary>A parsed row from the TzData table.</summary>
internal sealed record TzDataRow(
    string ZoneName,
    int OffsetH,
    int OffsetM,
    int OffsetS,
    string Rule,
    string Format,
    int UntilYear,
    int UntilMonth,
    string UntilDay,
    int AtH,
    int AtM,
    int AtS)
{
    public int StandardOffsetSeconds => OffsetH * 3600 + OffsetM * 60 + OffsetS;
}

/// <summary>A parsed row from the DstData table.</summary>
internal sealed record DstDataRow(
    string RuleName,
    int FromYear,
    string ToYear,
    int Month,
    string Day,
    int AtH,
    int AtM,
    int AtS,
    string UseUt,
    int SaveH,
    int SaveM,
    int SaveS,
    string Letter)
{
    public int SaveSeconds => SaveH * 3600 + SaveM * 60 + SaveS;
}

/// <summary>The result of a full timezone resolution for a given date/time.</summary>
public sealed record ZoneInfo(
    /// <summary>Total UTC offset in seconds (standard + DST).</summary>
    int OffsetSeconds,
    /// <summary>Formatted timezone name, e.g. "CET", "CEST", "GMT+1".</summary>
    string TzName,
    /// <summary>Whether DST is currently active.</summary>
    bool DstUsed,
    /// <summary>True when the local time falls in the DST gap (clocks spring forward).</summary>
    bool IsInvalid,
    /// <summary>True when the local time is ambiguous (clocks fall back).</summary>
    bool IsAmbiguous);
