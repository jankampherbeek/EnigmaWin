// PeriodCalculatorRequest.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Cycles.CyclesAstronomical;

/// <summary>Request for a period-based astronomical calculation.
/// Describes which factor to calculate, the interval between steps, the time range, and the coordinate to use.</summary>
public readonly struct PeriodCalculatorRequest(
    Factors factor,
    double interval,
    double jdStart,
    double jdEnd,
    Coordinates coordinate,
    Ayanamshas ayanamsha,
    ObserverPositions observerPosition = ObserverPositions.Geocentric)
{
    /// <summary>The celestial factor to calculate.</summary>
    public Factors Factor { get; } = factor;

    /// <summary>Interval in days between successive calculations.</summary>
    public double Interval { get; } = interval;

    /// <summary>Julian Day number for the start of the period.</summary>
    public double JdStart { get; } = jdStart;

    /// <summary>Julian Day number for the end of the period.</summary>
    public double JdEnd { get; } = jdEnd;

    /// <summary>The coordinate to calculate for the factor.</summary>
    public Coordinates Coordinate { get; } = coordinate;

    /// <summary>The ayanamsha to apply; use Tropical for no sidereal correction.</summary>
    public Ayanamshas Ayanamsha { get; } = ayanamsha;

    /// <summary>The observer position (geocentric, heliocentric, topocentric).</summary>
    public ObserverPositions ObserverPosition { get; } = observerPosition;
}
