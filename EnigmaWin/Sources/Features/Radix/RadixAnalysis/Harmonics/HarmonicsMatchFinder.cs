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

    private static double ShortestDeviation(double pos1, double pos2)
    {
        var diff = Math.Abs(pos1 - pos2) % 360.0;
        return diff > 180.0 ? 360.0 - diff : diff;
    }
}
