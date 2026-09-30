// AafRecord.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.ImportExport.Aaf;

/// <summary>
/// Format-specific intermediate representation of one AAF'97 record (#A93 + #B93 + optional
/// #ZNAM/#SRC/#VIA/#COM/#ENID chunks). Holds exactly the data physically present in the record; no
/// calculation and no IANA timezone resolution happens here. <see cref="AafMapper"/> converts this to and
/// from the Enigma domain model.
/// </summary>
public sealed record AafRecord
{
    // ── #A93 ─────────────────────────────────────────────────────────────────

    /// <summary>Field 0: last name, or a title for an event/institution. "*" means unknown.</summary>
    public string LastName { get; init; } = "*";

    /// <summary>Field 1: first name. "*" or empty means unknown/not applicable.</summary>
    public string FirstName { get; init; } = "*";

    /// <summary>Field 2 type code: "m", "f", "w", "e", "l", "o", or "*".</summary>
    public string Type { get; init; } = "*";

    public int Day { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }

    /// <summary>
    /// Resolved calendar for this date: from the explicit 'g'/'j' suffix when present, otherwise from AAF's
    /// own cutover-date fallback rule.
    /// </summary>
    public bool IsGregorian { get; init; } = true;

    public int Hour { get; init; }
    public int Minute { get; init; }
    public int Second { get; init; }
    public string Place { get; init; } = "";

    /// <summary>Field 6: country/region code. "*" means unknown.</summary>
    public string Country { get; init; } = "*";

    // ── #B93 ─────────────────────────────────────────────────────────────────

    /// <summary>Field 0, kept as text: "*" or a Julian Day number.</summary>
    public string JulianDayRaw { get; init; } = "*";

    public double Latitude { get; init; }
    public double Longitude { get; init; }

    /// <summary>Greenwich offset in seconds (standard time, without DST), or null for "*".</summary>
    public int? GreenwichOffsetSeconds { get; init; }

    /// <summary>Field 4 time type code: "0", "1", "w", "2", "h", "l", or "m" (lowercased).</summary>
    public string TimeType { get; init; } = "0";

    // ── Free-text / metadata chunks ──────────────────────────────────────────

    public string? ZoneName { get; init; }
    public string? Source { get; init; }
    public string? Via { get; init; }
    public string? Comment { get; init; }

    /// <summary>Names of unrecognized chunks encountered for this record (informational only).</summary>
    public IReadOnlyList<string> UnknownChunkNames { get; init; } = [];

    /// <summary>
    /// Enigma's own chart id, round-tripped via a custom "#ENID" chunk. AAF'97 has no id field of its own,
    /// but the format tolerates unrecognized chunks, so other AAF readers simply ignore it. Null when the chunk
    /// is absent, in which case a fresh id is generated on import.
    /// </summary>
    public Guid? EnigmaId { get; init; }

    public bool IsEvent => Type == "e";
}
