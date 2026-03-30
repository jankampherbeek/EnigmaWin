// MidpointsCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;

/// <summary>Calculates all base midpoints for a list of (factor, longitude) pairs.</summary>
public static class MidpointsCalculator
{
    /// <summary>
    /// Returns all base midpoints sorted by position (ascending).
    /// </summary>
    /// <param name="positions">List of (Factors, ecliptic longitude) tuples.</param>
    /// <returns>Sorted list of <see cref="BaseMidpoint"/>.</returns>
    public static List<BaseMidpoint> Calculate(List<(Factors Factor, double Longitude)> positions)
    {
        var midpoints = new List<BaseMidpoint>();
        var count = positions.Count;

        for (var i = 0; i < count; i++)
        {
            for (var j = i + 1; j < count; j++)
            {
                var (f1, long1) = positions[i];
                var (f2, long2) = positions[j];
                var midPos = MidpointPosition(long1, long2);
                midpoints.Add(new BaseMidpoint(f1, f2, midPos));
            }
        }

        return [.. midpoints.OrderBy(m => m.Position)];
    }

    /// <summary>
    /// Calculates the midpoint position along the shortest arc between two longitudes.
    /// </summary>
    /// <returns>Midpoint longitude in 0–360°.</returns>
    private static double MidpointPosition(double long1, double long2)
    {
        var small = Math.Min(long1, long2);
        var large = Math.Max(long1, long2);
        var diff = large - small;

        double firstOnArc;
        double arcedDiff;

        if (diff < 180.0)
        {
            // Shortest arc goes from small to large directly
            firstOnArc = small;
            arcedDiff = diff;
        }
        else
        {
            // Shortest arc wraps around 0°
            firstOnArc = large;
            arcedDiff = 360.0 - diff;
        }

        var midPos = firstOnArc + arcedDiff / 2.0;
        if (midPos >= 360.0) midPos -= 360.0;
        return midPos;
    }
}
