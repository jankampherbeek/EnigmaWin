// MidpointMatch.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;

/// <summary>The type of dial used when searching for midpoint matches.</summary>
public enum MidpointDialType
{
    Dial360 = 0,
    Dial90  = 1,
    Dial45  = 2
}

/// <summary>Extension methods for <see cref="MidpointDialType"/>.</summary>
public static class MidpointDialTypeExtensions
{
    public static double DialSize(this MidpointDialType dial) => dial switch
    {
        MidpointDialType.Dial360 => 360.0,
        MidpointDialType.Dial90  =>  90.0,
        MidpointDialType.Dial45  =>  45.0,
        _                        => 360.0
    };
}

/// <summary>A factor that is conjunct (or in hard aspect in the chosen dial) with a midpoint.</summary>
public record MidpointMatch(
    Factors Factor1,
    Factors Factor2,
    double MidpointPosition,
    Factors MatchingFactor,
    double MatchingPosition,
    double ActualOrb,
    double MaxOrb);
