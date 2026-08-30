// BlaHousePositions.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Position in houses for the BLA schema, ported from Enigma.Core/Slices/BlaSchema/HousePositions.cs.
/// Renamed from "HousePositions" to avoid colliding with EnigmaWin.Sources.Features.AstronCalc.HousePositions.</summary>
public static class BlaHousePositions
{
    /// <summary>Find the house position for a point with a specific longitude.</summary>
    public static int FindSingleHousePosition(BlaChartLongitudes chart, double longitude)
    {
        return FindHouseForLongitude(longitude, chart.Cusps);
    }

    /// <summary>Define the position in a house for every chart point (Ascendant/Mc are angles, not placed in a house).</summary>
    /// <param name="chart">The calculated chart</param>
    /// <returns>Dictionary with chart points and the index of the house: 1..12. Points not found in any house are omitted.</returns>
    public static Dictionary<Factors, int> DefineHousePositions(BlaChartLongitudes chart)
    {
        var housePositions = new Dictionary<Factors, int>();
        foreach (var pos in chart.Points)
        {
            if (BlaSchemaDomain.IsAngle(pos.Key)) continue;
            var houseNumber = FindHouseForLongitude(pos.Value, chart.Cusps);
            if (houseNumber > 0)
            {
                housePositions.Add(pos.Key, houseNumber);
            }
        }
        return housePositions;
    }

    /// <summary>Count the points in the houses.</summary>
    /// <param name="pointDetails">Details for the chart points, including the house</param>
    /// <returns>Dictionary with the index for the houses (1..12) and the count for each house</returns>
    public static Dictionary<int, int> DefineHouseCounts(List<BlaPointDetails> pointDetails)
    {
        var houseCounts = new Dictionary<int, int>
        {
            { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 },
            { 7, 0 }, { 8, 0 }, { 9, 0 }, { 10, 0 }, { 11, 0 }, { 12, 0 }
        };
        foreach (var details in pointDetails)
        {
            if (!BlaSchemaDomain.IsAngle(details.Point))
            {
                houseCounts[details.House]++;
            }
        }
        return houseCounts;
    }

    /// <summary>Find which house a longitude belongs to.</summary>
    private static int FindHouseForLongitude(double longitude, Dictionary<int, double> houseLongitudes)
    {
        var nrOfHouses = houseLongitudes.Count;
        if (nrOfHouses == 0) return 0;

        var sortedCusps = houseLongitudes.OrderBy(x => x.Key).ToList();

        for (var i = 0; i < nrOfHouses; i++)
        {
            var currentCusp = sortedCusps[i].Value;
            var nextCusp = i == nrOfHouses - 1 ? sortedCusps[0].Value : sortedCusps[i + 1].Value;
            var houseNumber = sortedCusps[i].Key;

            if (currentCusp > nextCusp)
            {
                if ((longitude >= currentCusp && longitude <= 360.0) || (longitude >= 0.0 && longitude < nextCusp))
                {
                    return houseNumber;
                }
            }
            else
            {
                if (longitude >= currentCusp && longitude < nextCusp)
                {
                    return houseNumber;
                }
            }
        }
        return 0;
    }
}
