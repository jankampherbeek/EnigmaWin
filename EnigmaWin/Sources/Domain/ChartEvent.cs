// ChartEvent.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Domain;

// Named ChartEvent to avoid collision with the C# 'event' keyword.
public sealed record ChartEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; init; } = "";
    public string? EventDescription { get; init; }
    public double JulianDate { get; init; }
    public string TimeZoneIdentifier { get; init; } = "UTC";
    public string? OriginalInput { get; init; }
    public string? PlaceName { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public IReadOnlyList<Guid> HoroscopeIds { get; init; } = [];
}
