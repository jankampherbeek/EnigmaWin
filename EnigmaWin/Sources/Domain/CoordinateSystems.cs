// CoordinateSystems.cs
// EnigmaWin
// Created by Jan Kampherbeek on 27-12-2025

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>Coordinate systems, used to define a position.</summary>
public enum CoordinateSystems
{
    Ecliptical = 0,
    Equatorial = 1,
    Horizontal = 3
}

/// <summary>Extension methods for the CoordinateSystems enum.</summary>
public static class CoordinateSystemsExtensions
{
    /// <summary>Value for flag construction as defined by the Swiss Ephemeris.</summary>
    /// <param name="coordinateSystem">The coordinate system.</param>
    /// <returns>The flag value for this coordinate system.</returns>
    public static int ValueForFlag(this CoordinateSystems coordinateSystem)
    {
        return coordinateSystem switch
        {
            CoordinateSystems.Ecliptical => 0,  // No specific flags for ecliptical
            CoordinateSystems.Equatorial => 2048,  // SEFLG_EQUATORIAL (2*1024)
            CoordinateSystems.Horizontal => 0,  // No specific flags for horizontal
            _ => 0
        };
    }

    /// <summary>Localized name key for this coordinate system.</summary>
    /// <param name="coordinateSystem">The coordinate system.</param>
    /// <returns>The localized name key.</returns>
    public static string LocalizedName(this CoordinateSystems coordinateSystem)
    {
        return coordinateSystem switch
        {
            CoordinateSystems.Ecliptical => "enum.coordinatesys.ecliptic",
            CoordinateSystems.Equatorial => "enum.coordinatesys.equatorial",
            CoordinateSystems.Horizontal => "enum.coordinatesys.horizontal",
            _ => string.Empty
        };
    }

    /// <summary>Get a CoordinateSystems enum value from an index.</summary>
    /// <param name="index">The index (raw value: 0, 1, or 3).</param>
    /// <returns>The CoordinateSystems enum value, or null if the index is not valid.</returns>
    public static CoordinateSystems? FromIndex(int index)
    {
        return index switch
        {
            0 => CoordinateSystems.Ecliptical,
            1 => CoordinateSystems.Equatorial,
            3 => CoordinateSystems.Horizontal,
            _ => null
        };
    }
}

