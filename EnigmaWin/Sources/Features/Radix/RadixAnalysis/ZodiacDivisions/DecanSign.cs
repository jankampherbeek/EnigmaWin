// DecanSign.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions;

/// <summary>Sign-based decans using the triplicity method (each decan adds 4 signs, mod 12).</summary>
public static class DecanSign
{
    /// <returns>Sign index 0 (Aries) … 11 (Pisces), or -1 if longitude is out of bounds.</returns>
    public static int IndexForDecanSign(double longitude)
    {
        if (longitude < 0.0 || longitude >= 360.0) return -1;
        var signIndex = (int)(longitude / 30.0);
        var decanWithinSign = (int)(longitude % 30.0 / 10.0);
        var result = signIndex + decanWithinSign * 4;
        if (result > 11) result -= 12;
        return result;
    }
}
