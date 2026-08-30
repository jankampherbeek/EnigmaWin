// SignPositions.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Positions in signs for the BLA schema, ported from Enigma.Core/Slices/BlaSchema/SignPositions.cs.</summary>
public static class SignPositions
{
    /// <summary>Count the points in the signs.</summary>
    /// <param name="pointDetails">Details for each point, including the sign</param>
    /// <returns>Dictionary with the index for the signs (1..12) and the count for each sign</returns>
    public static Dictionary<int, int> DefineSignCounts(List<BlaPointDetails> pointDetails)
    {
        var signCounts = new Dictionary<int, int>
        {
            { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 },
            { 7, 0 }, { 8, 0 }, { 9, 0 }, { 10, 0 }, { 11, 0 }, { 12, 0 }
        };
        foreach (var detail in pointDetails)
        {
            signCounts[detail.Sign]++;
        }
        return signCounts;
    }
}
