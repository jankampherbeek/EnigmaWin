// InterceptedClamped.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Intercepted signs and clamped houses, ported from Enigma.Core/Slices/BlaSchema/InterceptedClamped.cs.</summary>
public static class InterceptedClamped
{
    public static List<int> DefineInterceptedSigns(Dictionary<int, int> signsOnCusps)
    {
        var intercepted = new List<int>();
        for (var signIndex = 1; signIndex <= 12; signIndex++)
        {
            if (!signsOnCusps.ContainsValue(signIndex))
            {
                intercepted.Add(signIndex);
            }
        }
        return intercepted;
    }

    public static List<int> DefineClampedHouses(Dictionary<int, int> signsOnCusps)
    {
        var clampedHouses = new List<int>();
        for (var houseIndex = 1; houseIndex <= 12; houseIndex++)
        {
            if (!signsOnCusps.TryGetValue(houseIndex, out var currentSign)) continue;
            var nextHouseIndex = houseIndex == 12 ? 1 : houseIndex + 1;
            if (!signsOnCusps.TryGetValue(nextHouseIndex, out var nextSign)) continue;
            if (currentSign == nextSign)
            {
                clampedHouses.Add(houseIndex);
            }
        }
        return clampedHouses;
    }
}
