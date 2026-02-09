// SEFlags.cs
// EnigmaWin
// Created by Jan Kampherbeek on 27-12-2025

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.AstronCalc;

/// <summary>Calculates the flags for a calculation via the Swiss Ephemeris.</summary>
public static class SEFlags
{
    /// <summary>Define flags based on config and coordinate system (mirrors Swift defineFlags).</summary>
    /// <param name="configData">Configuration data.</param>
    /// <param name="coordSystem">The coordinate system.</param>
    /// <returns>The calculated flags for Swiss Ephemeris.</returns>
    public static int DefineFlags(ConfigData configData, CoordinateSystems coordSystem)
    {
        var flags = 2 + 256;  // use SE + speed

        if (coordSystem == CoordinateSystems.Equatorial)
        {
            flags += 2048;  // use equatorial positions
        }

        if (configData.ObserverPosition == ObserverPositions.Topocentric)
        {
            flags += 32 * 1024;  // use topocentric position (apply parallax)
        }

        if (configData.ObserverPosition == ObserverPositions.Heliocentric)
        {
            flags += 8;  // use heliocentric positions
        }

        if (configData.Ayanamsha != Ayanamshas.Tropical && coordSystem == CoordinateSystems.Ecliptical)
        {
            flags += 64 * 1024;  // use sidereal zodiac
        }

        return flags;
    }
}

