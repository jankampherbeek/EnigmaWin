// DecanPlanets.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions;

/// <summary>Planetary decans using the Chaldean order (Bouché-Leclercq sequence).</summary>
/// <remarks>Ruler indices: Sun=0, Moon=1, Mercury=2, Venus=3, Mars=4, Jupiter=5, Saturn=6.</remarks>
public static class DecanPlanets
{
    private static readonly int[] Planets = [4, 0, 3, 2, 1, 6, 5];

    /// <returns>Planet index (0–6), or -1 if longitude is out of bounds.</returns>
    public static int IndexForDecanPlanet(double longitude)
    {
        if (longitude < 0.0 || longitude >= 360.0) return -1;
        var decanIndex = (int)(longitude / 10.0);
        return Planets[decanIndex % Planets.Length];
    }
}
