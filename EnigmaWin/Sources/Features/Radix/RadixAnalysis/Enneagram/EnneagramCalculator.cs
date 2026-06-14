// EnneagramCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram;

public static class EnneagramCalculator
{
    // Returns a list of (typeNumber 1-9, strength) pairs sorted descending by strength
    public static List<(int Type, double Strength)> CalcStrengths(
        FullChart chart,
        IReadOnlyList<Factors> selectedFactors,
        bool includeHouses,
        bool countPlutoTwice,
        bool useUpdated)
    {
        var signsData  = EnneagramDataReader.ReadSigns(useUpdated);
        var housesData = EnneagramDataReader.ReadHouses(useUpdated);
        var factorLists = new Dictionary<int, List<double>>();
        for (var i = 1; i <= 9; i++) factorLists[i] = new List<double>();

        // Chart points in signs
        foreach (var factor in selectedFactors)
        {
            var longitude = GetLongitude(chart, factor);
            if (longitude < 0) continue;
            var sign = LongitudeToSign(longitude);
            AddFactors(signsData, (int)factor, sign, factorLists);
            if (countPlutoTwice && factor == Factors.Pluto)
                AddFactors(signsData, (int)factor, sign, factorLists);
        }

        if (includeHouses)
        {
            var cusps = GetCusps(chart);

            // Chart points in houses
            foreach (var factor in selectedFactors)
            {
                var longitude = GetLongitude(chart, factor);
                if (longitude < 0) continue;
                var house = LongitudeToHouse(longitude, cusps);
                AddFactors(housesData, (int)factor, house, factorLists);
                if (countPlutoTwice && factor == Factors.Pluto)
                    AddFactors(housesData, (int)factor, house, factorLists);
            }

            // Ascendant and MC in signs and houses
            var ascLon = chart.HousePositions.Ascendant.Longitude;
            var mcLon  = chart.HousePositions.Midheaven.Longitude;

            AddFactors(signsData,  1001, LongitudeToSign(ascLon),         factorLists);
            AddFactors(signsData,  1002, LongitudeToSign(mcLon),          factorLists);
            AddFactors(housesData, 1001, LongitudeToHouse(ascLon, cusps), factorLists);
            AddFactors(housesData, 1002, LongitudeToHouse(mcLon,  cusps), factorLists);

            // House cusps in signs (IDs 2001-2012)
            for (var ci = 1; ci <= 12; ci++)
            {
                if (ci >= cusps.Length) break;
                AddFactors(housesData, 2000 + ci, LongitudeToSign(cusps[ci]), factorLists);
            }
        }

        return Enumerable.Range(1, 9)
            .Select(t => (Type: t, Strength: factorLists[t].Aggregate(1.0, (acc, f) => acc * f)))
            .OrderByDescending(x => x.Strength)
            .ToList();
    }

    public static List<EnneagramDetailsLine> CalcDetails(
        FullChart chart,
        IReadOnlyList<Factors> selectedFactors,
        bool includeHouses,
        bool countPlutoTwice,
        bool useUpdated)
    {
        var signsData  = EnneagramDataReader.ReadSigns(useUpdated);
        var housesData = EnneagramDataReader.ReadHouses(useUpdated);
        var result     = new List<EnneagramDetailsLine>();

        foreach (var factor in selectedFactors)
        {
            var longitude = GetLongitude(chart, factor);
            if (longitude < 0) continue;
            var sign = LongitudeToSign(longitude);
            var f = FindFactors(signsData, (int)factor, sign);
            if (f != null)
            {
                result.Add(new EnneagramDetailsLine(factor, sign, true, f));
                if (countPlutoTwice && factor == Factors.Pluto)
                    result.Add(new EnneagramDetailsLine(factor, sign, true, f));
            }
        }

        if (includeHouses)
        {
            var cusps = GetCusps(chart);

            // Ascendant and MC in signs
            var ascLon = chart.HousePositions.Ascendant.Longitude;
            var mcLon  = chart.HousePositions.Midheaven.Longitude;
            var ascSignFactors = FindFactors(signsData, 1001, LongitudeToSign(ascLon));
            if (ascSignFactors != null) result.Add(new EnneagramDetailsLine(Factors.Ascendant, LongitudeToSign(ascLon), true, ascSignFactors));
            var mcSignFactors = FindFactors(signsData, 1002, LongitudeToSign(mcLon));
            if (mcSignFactors != null) result.Add(new EnneagramDetailsLine(Factors.Mc, LongitudeToSign(mcLon), true, mcSignFactors));

            // Chart points in houses
            foreach (var factor in selectedFactors)
            {
                var longitude = GetLongitude(chart, factor);
                if (longitude < 0) continue;
                var house = LongitudeToHouse(longitude, cusps);
                var f = FindFactors(housesData, (int)factor, house);
                if (f != null)
                {
                    result.Add(new EnneagramDetailsLine(factor, house, false, f));
                    if (countPlutoTwice && factor == Factors.Pluto)
                        result.Add(new EnneagramDetailsLine(factor, house, false, f));
                }
            }

            // Ascendant and MC in houses
            var ascHouseFactors = FindFactors(housesData, 1001, LongitudeToHouse(ascLon, cusps));
            if (ascHouseFactors != null) result.Add(new EnneagramDetailsLine(Factors.Ascendant, LongitudeToHouse(ascLon, cusps), false, ascHouseFactors));
            var mcHouseFactors = FindFactors(housesData, 1002, LongitudeToHouse(mcLon, cusps));
            if (mcHouseFactors != null) result.Add(new EnneagramDetailsLine(Factors.Mc, LongitudeToHouse(mcLon, cusps), false, mcHouseFactors));
        }

        return result;
    }

    private static double GetLongitude(FullChart chart, Factors factor)
    {
        if (factor == Factors.Ascendant) return chart.HousePositions.Ascendant.Longitude;
        if (factor == Factors.Mc)        return chart.HousePositions.Midheaven.Longitude;
        if (chart.Coordinates.TryGetValue(factor, out var pos) && pos.Ecliptical.Length > 0)
            return pos.Ecliptical[0].MainPos;
        return -1.0;
    }

    private static double[] GetCusps(FullChart chart)
    {
        // Returns array where index 1=house1cusp ... index 12=house12cusp; index 0 unused
        var cusps = new double[13];
        for (var i = 1; i <= 12 && i < chart.HousePositions.Cusps.Length; i++)
            cusps[i] = chart.HousePositions.Cusps[i].Longitude;
        return cusps;
    }

    private static int LongitudeToSign(double longitude)
    {
        var norm = longitude % 360.0;
        if (norm < 0) norm += 360.0;
        var sign = (int)(norm / 30.0) + 1;
        return sign > 12 ? 12 : sign < 1 ? 1 : sign;
    }

    private static int LongitudeToHouse(double longitude, double[] cusps)
    {
        var norm = longitude % 360.0;
        if (norm < 0) norm += 360.0;

        for (var h = 1; h <= 12; h++)
        {
            var cur  = cusps[h] % 360.0; if (cur < 0) cur += 360.0;
            var next = cusps[h == 12 ? 1 : h + 1] % 360.0; if (next < 0) next += 360.0;

            if (cur <= next)
            { if (norm >= cur && norm < next) return h; }
            else
            { if (norm >= cur || norm < next) return h; }
        }
        return 1;
    }

    private static void AddFactors(List<EnneagramData> data, int pointId, int index, Dictionary<int, List<double>> lists)
    {
        var f = FindFactors(data, pointId, index);
        if (f == null) return;
        for (var t = 0; t < 9; t++) lists[t + 1].Add(f[t]);
    }

    private static double[]? FindFactors(List<EnneagramData> data, int pointId, int index) =>
        data.FirstOrDefault(d => d.Point == pointId && d.Index == index)?.Factors;
}
