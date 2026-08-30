// SignsOnCusps.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Ecliptical signs of house cusps, ported from Enigma.Core/Slices/BlaSchema/SignsOnCusps.cs.</summary>
public static class SignsOnCusps
{
    /// <summary>Define the signs that are on all house cusps (intercepted signs are ignored).</summary>
    /// <returns>Dictionary with the index of the cusp (1..12) and the index of the sign (1..12)</returns>
    public static Dictionary<int, int> DefineSignsOnCusps(Dictionary<int, double> houseLongitudes)
    {
        var signsOnCusps = new Dictionary<int, int>();
        foreach (var (house, longitude) in houseLongitudes)
        {
            var sign = (int)Math.Truncate(longitude / 30.0) + 1;
            signsOnCusps.Add(house, sign);
        }
        return signsOnCusps;
    }
}
