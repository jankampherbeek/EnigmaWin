// QckRecord.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.ImportExport.Qck;

/// <summary>
/// Format-specific intermediate representation of one Quick*Chart / QCK fixed-width record (100 or 101
/// characters). Holds exactly the data that is physically present in the record; no astrological calculation
/// and no timezone resolution happens here. <see cref="QckMapper"/> converts this to and from the Enigma
/// domain model.
/// </summary>
public sealed record QckRecord
{
    public string Name { get; init; } = "";
    public int Month { get; init; }
    public int Day { get; init; }
    public int Year { get; init; }
    public int Hour { get; init; }
    public int Minute { get; init; }
    public int Second { get; init; }
    public bool IsPm { get; init; }

    /// <summary>
    /// Three-letter timezone abbreviation as found in the record (e.g. "CET", "EST", "LMT"), or empty when
    /// the record used plain zone time (three spaces).
    /// </summary>
    public string TimeZoneAbbreviation { get; init; } = "";

    /// <summary>
    /// The QCK correction in minutes, using QCK's own sign convention: UT = LocalTime + correction.
    /// This is NOT the modern UTC-offset sign.
    /// </summary>
    public int QckCorrectionMinutes { get; init; }

    /// <summary>Decimal degrees, East positive.</summary>
    public double Longitude { get; init; }

    /// <summary>Decimal degrees, North positive.</summary>
    public double Latitude { get; init; }

    public string Place { get; init; } = "";

    public bool IsLmt => TimeZoneAbbreviation == "LMT";

    /// <summary>
    /// True when the second letter of the timezone abbreviation is "D" or "W", the legacy QCK convention for
    /// daylight or war time. Informational only: the numeric correction remains authoritative.
    /// </summary>
    public bool IndicatesDaylightOrWarTime =>
        TimeZoneAbbreviation.Length == 3 && TimeZoneAbbreviation[1] is 'D' or 'W';

    /// <summary>Effective modern UTC offset in minutes, per QCK's inverted sign convention.</summary>
    public int EffectiveUtcOffsetMinutes => -QckCorrectionMinutes;
}
