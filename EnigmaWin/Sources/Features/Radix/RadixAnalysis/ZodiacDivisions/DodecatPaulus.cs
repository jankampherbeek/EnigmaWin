// DodecatPaulus.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions;

/// <summary>Paulus Alexandrinus dodecatemoria using the 13th harmonic method.</summary>
public static class DodecatPaulus
{
    /// <returns>Sign index 0 (Aries) … 11 (Pisces), or -1 if longitude is out of bounds.</returns>
    public static int IndexForDodecat(double longitude)
    {
        if (longitude < 0.0 || longitude >= 360.0) return -1;
        var multiplied = longitude * 13.0 % 360.0;
        if (multiplied < 0.0) multiplied += 360.0;
        return (int)(multiplied / 30.0);
    }
}
