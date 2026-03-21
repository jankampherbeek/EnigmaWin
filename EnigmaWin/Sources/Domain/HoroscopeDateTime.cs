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
