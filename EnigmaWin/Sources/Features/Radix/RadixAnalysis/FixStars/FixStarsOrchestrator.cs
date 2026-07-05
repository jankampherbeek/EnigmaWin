// FixStarsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars;

public static class FixStarsOrchestrator
{
    public static FixStarResult? GetFixStar(string starName, double jdUt, ObserverPositions obsPos, Ayanamshas ayanamsha)
    {
        PrepareWrapper(ayanamsha);
        int eclFlags = BuildFlags(obsPos, ayanamsha, equatorial: false);
        int eqFlags  = BuildFlags(obsPos, ayanamsha, equatorial: true);
        string seName = "," + starName;

        var eclResult = SEWrapper.CalculateFixStar(jdUt, seName, eclFlags);
        var eqResult  = SEWrapper.CalculateFixStar(jdUt, seName, eqFlags);
        var magnitude = SEWrapper.CalculateFixStarMagnitude(seName);

        if (eclResult is null || eqResult is null || magnitude is null)
            return null;

        return new FixStarResult(
            StarName:    starName.Trim(),
            Magnitude:   magnitude.Value,
            Longitude:   eclResult.MainPos,
            Latitude:    eclResult.Deviation,
            Declination: eqResult.Deviation);
    }

    public static IReadOnlyList<FixStarResult> GetFixStarSet(
        IEnumerable<string> starNames, double jdUt, ObserverPositions obsPos, Ayanamshas ayanamsha)
        => starNames
            .Select(n => GetFixStar(n, jdUt, obsPos, ayanamsha))
            .OfType<FixStarResult>()
            .ToList();

    private static void PrepareWrapper(Ayanamshas ayanamsha)
    {
        if (ayanamsha != Ayanamshas.Tropical)
            SEWrapper.SetAyanamsha(ayanamsha.SeId());
    }

    private static int BuildFlags(ObserverPositions obsPos, Ayanamshas ayanamsha, bool equatorial)
    {
        int flags = 2 + 256; // SEFLG_SWIEPH + SEFLG_SPEED
        if (equatorial)
            flags += 2048;   // SEFLG_EQUATORIAL
        if (obsPos == ObserverPositions.Topocentric)
            flags += 32768;  // SEFLG_TOPOCTR
        if (obsPos == ObserverPositions.Heliocentric)
            flags += 8;      // SEFLG_HELCTR
        if (ayanamsha != Ayanamshas.Tropical && !equatorial)
            flags += 65536;  // SEFLG_SIDEREAL
        return flags;
    }
}
