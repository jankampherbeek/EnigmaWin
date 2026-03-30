// MidpointMatch.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;

/// <summary>The type of dial used when searching for midpoint matches.</summary>
public enum MidpointDialType
{
    /// <summary>Full 360° dial — matches at 0° and 180° from the midpoint.</summary>
    Dial360 = 0,
    /// <summary>90° dial — matches at multiples of 90° (0°, 90°, 180°, 270°).</summary>
    Dial90  = 1,
    /// <summary>45° dial — matches at multiples of 45°.</summary>
    Dial45  = 2
}

/// <summary>Extension methods for <see cref="MidpointDialType"/>.</summary>
public static class MidpointDialTypeExtensions
{
    /// <summary>The modulus applied to reduce positions into this dial.</summary>
    public static double DialSize(this MidpointDialType dial) => dial switch
    {
        MidpointDialType.Dial360 => 360.0,
        MidpointDialType.Dial90  =>  90.0,
        MidpointDialType.Dial45  =>  45.0,
        _                        => 360.0
    };
}

/// <summary>A factor that is conjunct (or in hard aspect in the chosen dial) with a midpoint.</summary>
/// <param name="Factor1">First factor forming the midpoint pair.</param>
/// <param name="Factor2">Second factor forming the midpoint pair.</param>
/// <param name="MidpointPosition">Ecliptic longitude of the midpoint (0–360°).</param>
/// <param name="MatchingFactor">The factor whose position matches the midpoint.</param>
/// <param name="MatchingPosition">Ecliptic longitude of the matching factor (0–360°).</param>
/// <param name="ActualOrb">Actual angular deviation from the exact match (0 = exact).</param>
/// <param name="MaxOrb">Maximum allowed orb for this match.</param>
public record MidpointMatch(
    Factors Factor1,
    Factors Factor2,
    double MidpointPosition,
    Factors MatchingFactor,
    double MatchingPosition,
    double ActualOrb,
    double MaxOrb);
