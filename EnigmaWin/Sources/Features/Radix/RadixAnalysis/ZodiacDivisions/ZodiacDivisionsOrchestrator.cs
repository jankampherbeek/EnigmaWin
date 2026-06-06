// ZodiacDivisionsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions;

/// <summary>Routes zodiac division calculations to the appropriate calculator.</summary>
public static class ZodiacDivisionsOrchestrator
{
    /// <param name="longitude">Ecliptic longitude, must be >= 0 and &lt; 360.</param>
    /// <param name="method">The division method to apply.</param>
    /// <returns>The calculated index, or -1 if longitude is out of bounds.</returns>
    public static int Calculate(double longitude, ZodiacDivisionMethod method) => method switch
    {
        ZodiacDivisionMethod.Signs           => ZodiacSign.IndexForSign(longitude),
        ZodiacDivisionMethod.DecansPlanet    => DecanPlanets.IndexForDecanPlanet(longitude),
        ZodiacDivisionMethod.DecansSign      => DecanSign.IndexForDecanSign(longitude),
        ZodiacDivisionMethod.DodecatsOriginal => DodecatOriginal.IndexForDodecat(longitude),
        ZodiacDivisionMethod.DodecatsPaulus  => DodecatPaulus.IndexForDodecat(longitude),
        ZodiacDivisionMethod.BoundsEgyptian  => Bounds.IndexBound(longitude, egyptian: true),
        ZodiacDivisionMethod.BoundsPtolemy   => Bounds.IndexBound(longitude, egyptian: false),
        _                                    => -1
    };
}
