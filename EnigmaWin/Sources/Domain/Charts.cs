// Charts.cs
// EnigmaWin
// Created by Jan Kampherbeek on 27-12-2025

using System.Collections.Generic;

namespace EnigmaWin.Sources.Domain;

/// <summary>Full chart containing all astronomical positions and house positions.</summary>
/// <param name="Coordinates">Dictionary mapping factors to their full positions</param>
/// <param name="HousePositions">House positions including cusps and angular points</param>
/// <param name="SiderealTime">Sidereal time in degrees</param>
/// <param name="JulianDay">Julian day number</param>
/// <param name="Obliquity">Obliquity of the ecliptic in degrees</param>
public record FullChart(
    Dictionary<Factors, FullFactorPosition> Coordinates,
    HousePositions HousePositions,
    double SiderealTime,
    double JulianDay,
    double Obliquity);

