// ZodiacSign.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions;

public static class ZodiacSign
{
    /// <returns>0 (Aries) … 11 (Pisces), or -1 if longitude is out of bounds.</returns>
    public static int IndexForSign(double longitude)
    {
        if (longitude < 0.0 || longitude >= 360.0) return -1;
        return (int)(longitude / 30.0);
    }
}
