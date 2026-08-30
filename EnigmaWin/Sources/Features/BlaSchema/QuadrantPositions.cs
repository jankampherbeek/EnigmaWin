// QuadrantPositions.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Position in a quadrant, ported from Enigma.Core/Slices/BlaSchema/QuadrantPositions.cs.</summary>
public static class QuadrantPositions
{
    public static Dictionary<int, int> DefineQuadrants(List<BlaHouseDetails> houseDetails)
    {
        var quadrantCounts = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 } };
        foreach (var houseDetail in houseDetails)
        {
            switch (houseDetail.HouseNr)
            {
                case 1 or 2 or 3:
                    quadrantCounts[1] += houseDetail.PointsInHouse.Count;
                    break;
                case 4 or 5 or 6:
                    quadrantCounts[2] += houseDetail.PointsInHouse.Count;
                    break;
                case 7 or 8 or 9:
                    quadrantCounts[3] += houseDetail.PointsInHouse.Count;
                    break;
                case 10 or 11 or 12:
                    quadrantCounts[4] += houseDetail.PointsInHouse.Count;
                    break;
            }
        }
        return quadrantCounts;
    }
}
