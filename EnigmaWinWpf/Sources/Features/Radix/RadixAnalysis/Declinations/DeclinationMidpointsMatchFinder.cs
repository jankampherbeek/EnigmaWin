// DeclinationMidpointsMatchFinder.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

public static class DeclinationMidpointsMatchFinder
{
    public static List<DeclOccupiedMidpoint> Find(
        List<DeclBaseMidpoint> baseMidpoints,
        List<(Factors Factor, double Declination)> positions,
        double orb)
    {
        var results = new List<DeclOccupiedMidpoint>();

        foreach (var midpoint in baseMidpoints)
        {
            foreach (var (factor, declination) in positions)
            {
                var deviation = Math.Abs(midpoint.Position - declination);
                if (deviation > orb) continue;
                var exactness = orb > 0.0 ? 100.0 - (deviation / orb * 100.0) : 100.0;
                results.Add(new DeclOccupiedMidpoint(midpoint, factor, declination, deviation, exactness));
            }
        }

        return [.. results.OrderBy(r => r.ActualOrb)];
    }
}
