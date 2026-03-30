// MidpointMatchFinder.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;

/// <summary>Finds all factors that are conjunct with a midpoint (or in a hard aspect within the chosen dial).</summary>
public static class MidpointMatchFinder
{
    /// <summary>
    /// Returns all midpoint matches, sorted by actual orb (most exact first).
    /// </summary>
    /// <param name="baseMidpoints">All base midpoints (as produced by <see cref="MidpointsCalculator"/>).</param>
    /// <param name="positions">All active (factor, ecliptic longitude) pairs.</param>
    /// <param name="dialType">The dial to use for matching.</param>
    /// <param name="orb">Maximum allowed orb in degrees.</param>
    /// <returns>Sorted list of <see cref="MidpointMatch"/>, most exact first.</returns>
    public static List<MidpointMatch> Find(
        List<BaseMidpoint> baseMidpoints,
        List<(Factors Factor, double Longitude)> positions,
        MidpointDialType dialType,
        double orb)
    {
        var dialSize = dialType.DialSize();
        var matches = new List<MidpointMatch>();

        foreach (var midpoint in baseMidpoints)
        {
            var midPos = ReduceToDial(midpoint.Position, dialSize);
            foreach (var (factor, longitude) in positions)
            {
                var factorPos = ReduceToDial(longitude, dialSize);
                var deviation = ShortestDeviation(midPos, factorPos, dialSize);
                if (deviation <= orb)
                {
                    matches.Add(new MidpointMatch(
                        Factor1:          midpoint.Factor1,
                        Factor2:          midpoint.Factor2,
                        MidpointPosition: midpoint.Position,
                        MatchingFactor:   factor,
                        MatchingPosition: longitude,
                        ActualOrb:        deviation,
                        MaxOrb:           orb));
                }
            }
        }

        return [.. matches.OrderBy(m => m.ActualOrb)];
    }

    /// <summary>Reduces a 360° longitude to the given dial by taking modulo.</summary>
    private static double ReduceToDial(double longitude, double dialSize) =>
        longitude % dialSize;

    /// <summary>
    /// Shortest angular distance between two positions within a dial of <paramref name="dialSize"/> degrees.
    /// The result lies in [0, dialSize/2].
    /// </summary>
    private static double ShortestDeviation(double pos1, double pos2, double dialSize)
    {
        var small = Math.Min(pos1, pos2);
        var large = Math.Max(pos1, pos2);
        var diff = large - small;
        return diff >= dialSize / 2.0 ? dialSize - diff : diff;
    }
}
