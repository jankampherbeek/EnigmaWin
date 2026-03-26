// AverageSpeed.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Speed;

/// <summary>
/// Average daily speeds (in decimal degrees) for celestial factors that support slow/stationary detection.
/// Returns null for factors without a known average speed — only Direct/Retrograde applies to those.
/// </summary>
internal static class AverageSpeed
{
    /// <summary>Returns the average daily speed for a given factor, or null if not applicable.</summary>
    internal static double? AverageFor(Factors factor) => factor switch
    {
        Factors.Sun     => 0.985555,
        Factors.Moon    => 13.17666,
        Factors.Mercury => 1.3833,
        Factors.Venus   => 1.2,
        Factors.Mars    => 0.5242,
        Factors.Jupiter => 0.0831,
        Factors.Saturn  => 0.0336,
        Factors.Uranus  => 0.026666,
        Factors.Neptune => 0.006668,
        Factors.Pluto   => 0.0041666,
        _               => null
    };
}
