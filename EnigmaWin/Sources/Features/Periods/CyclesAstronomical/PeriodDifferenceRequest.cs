// PeriodDifferenceRequest.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Periods.CyclesAstronomical;

/// <summary>Request for a period-based calculation of the difference between two celestial factors.
/// Describes the two factors to compare, the interval between steps, the time range, and the coordinate to use.</summary>
public readonly struct PeriodDifferenceRequest(
    Factors factor1,
    Factors factor2,
    double interval,
    double jdStart,
    double jdEnd,
    Coordinates coordinate,
    Ayanamshas ayanamsha,
    ObserverPositions observerPosition = ObserverPositions.Geocentric)
{
    /// <summary>The first celestial factor to compare.</summary>
    public Factors Factor1 { get; } = factor1;

    /// <summary>The second celestial factor to compare.</summary>
    public Factors Factor2 { get; } = factor2;

    /// <summary>Interval in days between successive calculations.</summary>
    public double Interval { get; } = interval;

    /// <summary>Julian Day number for the start of the period.</summary>
    public double JdStart { get; } = jdStart;

    /// <summary>Julian Day number for the end of the period.</summary>
    public double JdEnd { get; } = jdEnd;

    /// <summary>The coordinate to calculate for both factors.</summary>
    public Coordinates Coordinate { get; } = coordinate;

    /// <summary>The ayanamsha to apply; use Tropical for no sidereal correction.</summary>
    public Ayanamshas Ayanamsha { get; } = ayanamsha;

    /// <summary>The observer position (geocentric, heliocentric, topocentric).</summary>
    public ObserverPositions ObserverPosition { get; } = observerPosition;
}
