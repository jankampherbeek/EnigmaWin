// EnigmaExportModels.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnigmaWin.Sources.Features.ImportExport.Enigma;

/// <summary>
/// Top-level container for the Enigma JSON import/export format. Covers all persisted chart and event data
/// needed to reconstruct them; calculated positions are deliberately excluded, since they are derived at
/// calculation time from a date/time plus the active configuration. The field names, id and date notation
/// match the Apple version of Enigma, so files can be exchanged in both directions.
/// </summary>
public sealed class EnigmaExportFile
{
    public const int CurrentFormatVersion = 1;

    public int FormatVersion { get; set; }
    public DateTimeOffset ExportedAt { get; set; }
    public List<EnigmaChartDto> Charts { get; set; } = [];
    public List<EnigmaEventDto> Events { get; set; } = [];

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new UpperCaseGuidConverter(), new Iso8601SecondsConverter() }
    };
}

public sealed class EnigmaDateTimeDto
{
    public Guid Id { get; set; }
    public double JulianDate { get; set; }
    public string TimeZoneIdentifier { get; set; } = "UTC";
    public bool TimeIsUnknown { get; set; }
    public bool IsPreferred { get; set; }
    public string? Label { get; set; }
    public string? OriginalInput { get; set; }
}

public sealed class EnigmaChartDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Notes { get; set; }
    public string? Source { get; set; }
    public string RoddenRating { get; set; } = "None";
    public string? PlaceName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<EnigmaDateTimeDto> DateTimes { get; set; } = [];
}

public sealed class EnigmaEventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? EventDescription { get; set; }
    public double JulianDate { get; set; }
    public string TimeZoneIdentifier { get; set; } = "UTC";
    public string? OriginalInput { get; set; }
    public string? PlaceName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    /// <summary>Written by the Apple version; EnigmaWin has no such field and ignores it on import.</summary>
    public string? Country { get; set; }

    /// <summary>Written by the Apple version; EnigmaWin has no such field and ignores it on import.</summary>
    public string? Location { get; set; }

    /// <summary>
    /// Ids of the charts this event is linked to. Reconstructs the many-to-many relationship on import;
    /// the chart side does not carry the inverse list.
    /// </summary>
    public List<Guid> ChartIds { get; set; } = [];
}

/// <summary>Writes ids in upper case, as the Apple version does; reading accepts any case.</summary>
internal sealed class UpperCaseGuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Guid.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString().ToUpperInvariant());
}

/// <summary>
/// Writes timestamps as ISO 8601 in UTC without fractional seconds ("2026-09-30T10:15:00Z"), the only form
/// the Apple version can read; reading accepts any ISO 8601 form.
/// </summary>
internal sealed class Iso8601SecondsConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateTimeOffset.Parse(reader.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));
}
