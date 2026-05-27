// EventValues.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Progressive.Events;

/// <summary>
/// Carries the mutable fields of a ChartEvent.
/// Used as input for both creation and updates to avoid long parameter lists.
/// </summary>
public sealed record EventValues(
    string Title,
    double JulianDate,
    string TimeZoneIdentifier = "UTC",
    string? EventDescription  = null,
    string? OriginalInput     = null,
    string? PlaceName         = null,
    double? Latitude          = null,
    double? Longitude         = null);
