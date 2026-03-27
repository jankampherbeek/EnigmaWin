// HoroscopeDateTime.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Domain;

public sealed record HoroscopeDateTime
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid HoroscopeId { get; init; }
    public double JulianDate { get; init; }
    public string TimeZoneIdentifier { get; init; } = "UTC";
    public bool TimeIsUnknown { get; init; }
    public bool IsPreferred { get; init; }
    public string? Label { get; init; }
    public string? OriginalInput { get; init; }
}
