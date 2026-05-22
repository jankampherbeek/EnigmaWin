// Coordinates.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>Coordinate types used for positional data.</summary>
public enum Coordinates
{
    Longitude = 0,
    Latitude = 1,
    RightAscension = 2,
    Declination = 3,
    Distance = 4
}

/// <summary>Extension methods for the Coordinates enum.</summary>
public static class CoordinatesExtensions
{
    /// <summary>Localized name key for this coordinate type.</summary>
    public static string LocalizedName(this Coordinates coordinate)
    {
        return coordinate switch
        {
            Coordinates.Longitude => "enum.coordinates.longitude",
            Coordinates.Latitude => "enum.coordinates.latitude",
            Coordinates.RightAscension => "enum.coordinates.rightascension",
            Coordinates.Declination => "enum.coordinates.declination",
            Coordinates.Distance => "enum.coordinates.distance",
            _ => string.Empty
        };
    }

    /// <summary>Get a Coordinates enum value from an index.</summary>
    public static Coordinates? FromIndex(int index)
    {
        var allCases = Enum.GetValues<Coordinates>();
        if (index < 0 || index >= allCases.Length)
        {
            return null;
        }
        return allCases[index];
    }
}
