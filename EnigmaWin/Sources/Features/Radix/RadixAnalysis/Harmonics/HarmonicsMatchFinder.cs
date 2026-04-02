// HarmonicsMatchFinder.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics;

/// <summary>Finds all radix factors that share a position with a harmonic position within a given orb.</summary>
public static class HarmonicsMatchFinder
{
    /// <summary>
    /// Returns all harmonic matches, sorted by actual orb (most exact first).
    /// A match occurs when a harmonic position and a radix factor position are within
    /// <paramref name="orb"/> degrees of each other (wrap-around at 0°/360° included).
    /// A factor is not matched against its own harmonic position.
    /// </summary>
    /// <param name="harmonicPositions">Harmonic positions as produced by <see cref="HarmonicsCalculator"/>.</param>
    /// <param name="radixPositions">Radix (ecliptic longitude) positions for all active factors.</param>
    /// <param name="orb">Maximum allowed orb in degrees.</param>
    /// <returns>Sorted list of <see cref="HarmonicsMatch"/>, most exact first.</returns>
    public static List<HarmonicsMatch> Find(
        List<(Factors Factor, double Longitude)> harmonicPositions,
        List<(Factors Factor, double Longitude)> radixPositions,
        double orb)
    {
        var matches = new List<HarmonicsMatch>();

        foreach (var (harmonicFactor, harmonicLon) in harmonicPositions)
        {
            foreach (var (radixFactor, radixLon) in radixPositions)
            {
                if (harmonicFactor == radixFactor) continue;

                var deviation = ShortestDeviation(harmonicLon, radixLon);
                if (deviation <= orb)
                {
                    matches.Add(new HarmonicsMatch(
                        RadixFactor:    radixFactor,
                        HarmonicFactor: harmonicFactor,
                        ActualOrb:      deviation,
                        MaxOrb:         orb));
                }
            }
        }

        return [.. matches.OrderBy(m => m.ActualOrb)];
    }

    /// <summary>
    /// Shortest angular distance between two positions on a 360° circle.
    /// The result lies in [0, 180].
    /// </summary>
    private static double ShortestDeviation(double pos1, double pos2)
    {
        var diff = Math.Abs(pos1 - pos2) % 360.0;
        return diff > 180.0 ? 360.0 - diff : diff;
    }
}
