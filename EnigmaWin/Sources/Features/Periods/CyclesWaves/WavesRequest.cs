// WavesRequest.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Periods.CyclesWaves;

/// <summary>Request for a waves calculation across a time period.
/// Describes the cycle type, interval between steps, time range, and coordinate to use.</summary>
public readonly struct WavesRequest(
    Factors cycleType,
    int interval,
    double jdStart,
    double jdEnd,
    Coordinates coordinate,
    ObserverPositions observerPosition = ObserverPositions.Geocentric)
{
    /// <summary>The cycle type that defines the set of planets to include.
    /// Must be Jupiter, Saturn, or Uranus.</summary>
    public Factors CycleType { get; } = cycleType;

    /// <summary>Interval in days between successive calculations.</summary>
    public int Interval { get; } = interval;

    /// <summary>Julian Day number for the start of the period.</summary>
    public double JdStart { get; } = jdStart;

    /// <summary>Julian Day number for the end of the period.</summary>
    public double JdEnd { get; } = jdEnd;

    /// <summary>The coordinate to calculate for each planet.</summary>
    public Coordinates Coordinate { get; } = coordinate;

    /// <summary>The observer position (geocentric, heliocentric, topocentric).</summary>
    public ObserverPositions ObserverPosition { get; } = observerPosition;
}
