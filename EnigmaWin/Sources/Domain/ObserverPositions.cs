// ObserverPositions.cs
// EnigmaWin
// Created by Jan Kampherbeek on 27-12-2025

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>Observer positions for astronomical calculations.</summary>
public enum ObserverPositions
{
    Geocentric = 0,
    Topocentric = 1,
    Heliocentric = 2
}

/// <summary>Extension methods for the ObserverPositions enum.</summary>
public static class ObserverPositionsExtensions
{
    /// <summary>Localized name key for this observer position.</summary>
    /// <param name="observerPosition">The observer position.</param>
    /// <returns>The localized name key.</returns>
    public static string LocalizedName(this ObserverPositions observerPosition)
    {
        return observerPosition switch
        {
            ObserverPositions.Geocentric => "enum.observerpos.geocentric",
            ObserverPositions.Topocentric => "enum.observerpos.topocentric",
            ObserverPositions.Heliocentric => "enum.observerpos.heliocentric",
            _ => string.Empty
        };
    }

    /// <summary>Get an ObserverPositions enum value from an index.</summary>
    /// <param name="index">The index (0-based).</param>
    /// <returns>The ObserverPositions enum value, or null if the index is out of range.</returns>
    public static ObserverPositions? FromIndex(int index)
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        if (index < 0 || index >= allCases.Length)
        {
            return null;
        }
        return allCases[index];
    }
}

