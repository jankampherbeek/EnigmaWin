// DodecatOriginal.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions;

/// <summary>Babylonian dodecatemoria: each sign divided into 12 sub-portions of 2.5°.</summary>
public static class DodecatOriginal
{
    /// <returns>Sign index 0 (Aries) … 11 (Pisces), or -1 if longitude is out of bounds.</returns>
    public static int IndexForDodecat(double longitude)
    {
        if (longitude < 0.0 || longitude >= 360.0) return -1;
        var signIndex = (int)(longitude / 30.0);
        var subportion = (int)(longitude % 30.0 / 2.5);
        var result = signIndex + subportion;
        if (result > 11) result -= 12;
        return result;
    }
}
