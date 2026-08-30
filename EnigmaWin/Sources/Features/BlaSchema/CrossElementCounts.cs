// CrossElementCounts.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Counts for crosses and elements in signs and houses, ported from Enigma.Core/Slices/BlaSchema/CrossElementCounts.cs.</summary>
public static class CrossElementCounts
{
    /// <summary>Create the counts for crosses and elements.</summary>
    /// <returns>Two dictionaries, the first for crosses (1=cardinal, 2=fixed, 3=mutable),
    /// the second for elements (1=fire, 2=earth, 3=air, 4=water)</returns>
    public static (Dictionary<int, BlaSignHouseCountLine> Crosses, Dictionary<int, BlaSignHouseCountLine> Elements) CreateCrossesElementsCounts(
        Dictionary<int, int> signCounts,
        Dictionary<int, int> houseCounts,
        List<BlaHouseDetails> houseDetails)
    {
        var allLines = CreateAllLines(signCounts, houseCounts, houseDetails);
        var crossLines = CreateLinesForCrosses(allLines);
        var elementLines = CreateLinesForElements(allLines);
        return (crossLines, elementLines);
    }

    private static Dictionary<int, BlaSignHouseCountLine> CreateAllLines(
        Dictionary<int, int> signCounts,
        Dictionary<int, int> houseCounts,
        List<BlaHouseDetails> houseDetails)
    {
        var countLines = new Dictionary<int, BlaSignHouseCountLine>();

        foreach (var (signIndex, signCount) in signCounts)
        {
            var houseCount = houseCounts[signIndex];
            var sum = signCount + houseCount;
            var hCusp = 0;

            foreach (var houseDetail in houseDetails)
            {
                if (houseDetail.SignOnCusp == signIndex)
                {
                    hCusp += houseDetail.PointsInHouse.Count;
                }
            }
            var total = sum + hCusp;
            countLines.Add(signIndex, new BlaSignHouseCountLine(signCount, houseCount, sum, hCusp, total));
        }
        return countLines;
    }

    private static Dictionary<int, BlaSignHouseCountLine> CreateLinesForElements(Dictionary<int, BlaSignHouseCountLine> allCounts)
    {
        var fireLines = new List<BlaSignHouseCountLine>();
        var earthLines = new List<BlaSignHouseCountLine>();
        var airLines = new List<BlaSignHouseCountLine>();
        var waterLines = new List<BlaSignHouseCountLine>();
        foreach (var line in allCounts)
        {
            if (line.Key is 1 or 5 or 9) fireLines.Add(line.Value);
            if (line.Key is 2 or 6 or 10) earthLines.Add(line.Value);
            if (line.Key is 3 or 7 or 11) airLines.Add(line.Value);
            if (line.Key is 4 or 8 or 12) waterLines.Add(line.Value);
        }

        return new Dictionary<int, BlaSignHouseCountLine>
        {
            { 1, CreateLineWithTotals(fireLines) },
            { 2, CreateLineWithTotals(earthLines) },
            { 3, CreateLineWithTotals(airLines) },
            { 4, CreateLineWithTotals(waterLines) }
        };
    }

    private static Dictionary<int, BlaSignHouseCountLine> CreateLinesForCrosses(Dictionary<int, BlaSignHouseCountLine> allCounts)
    {
        var cardinalLines = new List<BlaSignHouseCountLine>();
        var fixedLines = new List<BlaSignHouseCountLine>();
        var mutableLines = new List<BlaSignHouseCountLine>();
        foreach (var line in allCounts)
        {
            if (line.Key is 1 or 4 or 7 or 10) cardinalLines.Add(line.Value);
            if (line.Key is 2 or 5 or 8 or 11) fixedLines.Add(line.Value);
            if (line.Key is 3 or 6 or 9 or 12) mutableLines.Add(line.Value);
        }

        return new Dictionary<int, BlaSignHouseCountLine>
        {
            { 1, CreateLineWithTotals(cardinalLines) },
            { 2, CreateLineWithTotals(fixedLines) },
            { 3, CreateLineWithTotals(mutableLines) }
        };
    }

    private static BlaSignHouseCountLine CreateLineWithTotals(List<BlaSignHouseCountLine> lines)
    {
        var signCount = 0;
        var houseCount = 0;
        var hCuspCount = 0;
        foreach (var line in lines)
        {
            signCount += line.Sign;
            houseCount += line.House;
            hCuspCount += line.HCusp;
        }

        var sum = signCount + houseCount;
        var total = sum + hCuspCount;
        return new BlaSignHouseCountLine(signCount, houseCount, sum, hCuspCount, total);
    }
}
