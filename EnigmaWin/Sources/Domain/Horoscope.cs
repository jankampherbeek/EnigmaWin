using System;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Domain;

public sealed record Horoscope
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public string? Notes { get; init; }
    public string? Source { get; init; }
    public RoddenRating RoddenRating { get; init; } = RoddenRating.None;
    public string? PlaceName { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public IReadOnlyList<HoroscopeDateTime> DateTimes { get; init; } = [];
}
