// DeclinationMidpointsMatchFinder.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

/// <summary>
/// Finds all factors whose declination coincides with a declination midpoint within the allowed orb.
/// </summary>
/// <remarks>
/// Comparable to <c>MidpointMatchFinder</c> for ecliptic midpoints, but operates on the linear
/// declination scale (roughly −90° to +90°) so no circular reduction is needed.
/// <para>Algorithm:</para>
/// <list type="number">
///   <item>For each <see cref="DeclBaseMidpoint"/>, compare its position with every (factor, declination) pair.</item>
///   <item><c>deviation = |midpointPosition − factorDeclination|</c></item>
///   <item>If <c>deviation ≤ orb</c>, record a <see cref="DeclOccupiedMidpoint"/> with
///     <c>exactness = 100 − (deviation / orb × 100)</c>.</item>
/// </list>
/// </remarks>
public static class DeclinationMidpointsMatchFinder
{
    /// <summary>
    /// Returns all occupied declination midpoints, sorted by <see cref="DeclOccupiedMidpoint.ActualOrb"/>
    /// ascending (most exact first).
    /// </summary>
    /// <param name="baseMidpoints">All declination base midpoints (as produced by <see cref="DeclMidpointsCalculator"/>).</param>
    /// <param name="positions">All active (factor, declination) pairs to test against each midpoint.</param>
    /// <param name="orb">Maximum allowed deviation in degrees.</param>
    /// <returns>Every combination of midpoint + occupying factor whose deviation ≤ orb,
    /// sorted by <see cref="DeclOccupiedMidpoint.ActualOrb"/> ascending.</returns>
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
