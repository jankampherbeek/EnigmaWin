// HarmonicsCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics;

/// <summary>Calculates harmonic positions for a list of (factor, longitude) pairs.</summary>
public static class HarmonicsCalculator
{
    /// <summary>
    /// Returns harmonic positions for all factors.
    /// Each longitude is multiplied by the harmonic number and reduced to 0–360°.
    /// When the harmonic number is effectively an integer, integer arithmetic is used for better precision.
    /// </summary>
    /// <param name="positions">List of (Factors, ecliptic longitude) tuples.</param>
    /// <param name="harmonic">The harmonic number (typically a whole number such as 2, 3, 4 …).</param>
    /// <returns>List of (Factors, harmonic longitude) tuples in the same order as the input.</returns>
    public static List<(Factors Factor, double Longitude)> Calculate(
        List<(Factors Factor, double Longitude)> positions,
        double harmonic)
    {
        var result = new List<(Factors, double)>(positions.Count);

        // Use integer multiplication when the harmonic is effectively a whole number,
        // to avoid floating-point drift for the common case (h = 2, 3, 4 …).
        var intHarmonic = TryGetIntHarmonic(harmonic);

        foreach (var (factor, longitude) in positions)
        {
            var harmonicLongitude = intHarmonic.HasValue
                ? HarmonicLongitudeInt(longitude, intHarmonic.Value)
                : HarmonicLongitudeDouble(longitude, harmonic);

            result.Add((factor, harmonicLongitude));
        }

        return result;
    }

    /// <summary>
    /// Returns the integer value of <paramref name="harmonic"/> when it is within 1e-9 of a whole number,
    /// otherwise returns <c>null</c>.
    /// </summary>
    private static int? TryGetIntHarmonic(double harmonic)
    {
        var rounded = (int)Math.Round(harmonic);
        return Math.Abs(harmonic - rounded) < 1e-9 ? rounded : null;
    }

    private static double HarmonicLongitudeInt(double longitude, int harmonic)
    {
        var result = longitude * harmonic;
        result %= 360.0;
        if (result < 0.0) result += 360.0;
        return result;
    }

    private static double HarmonicLongitudeDouble(double longitude, double harmonic)
    {
        var result = longitude * harmonic;
        result %= 360.0;
        if (result < 0.0) result += 360.0;
        return result;
    }
}
