// Dispositors.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Schema for dispositors, ported from Enigma.Core/Slices/BlaSchema/Dispositors.cs.</summary>
public static class Dispositors
{
    /// <summary>Create dispositors.</summary>
    public static List<BlaDispositorLine> CreateDispositors(
        Dictionary<int, int> signCounts,
        Dictionary<int, int> houseCounts,
        Dictionary<int, int> signsOnHouseCusps,
        Dictionary<Factors, int> planetsInSign,
        Dictionary<Factors, int> planetsInHouses,
        Dictionary<Factors, int> planetsInDecanates,
        bool useDecanates)
    {
        var dispositors = new List<BlaDispositorLine>();

        var decansWithFactors = new Dictionary<int, List<Factors>>
        {
            { 1, [] }, { 2, [] }, { 3, [] }, { 4, [] }, { 5, [] }, { 6, [] }, { 7, [] }
        };
        foreach (var factor in planetsInDecanates)
        {
            decansWithFactors[factor.Value].Add(factor.Key);
        }

        foreach (var rulerPair in RulerPairsForDispositors())
        {
            var mainRuler = rulerPair.MainRuler;
            var subRuler = rulerPair.SubRuler;

            // rulers in signs

            var (signMainRuler, signSubRuler) = FindSignsRuledBy(mainRuler);
            var signMainRulerCount = signCounts[signMainRuler];
            var signSubRulerCount = signCounts[signSubRuler];
            var directSignCount = signMainRulerCount + signSubRulerCount;

            var indirectSignCount = 0;
            var processedSigns = new List<int>();
            foreach (var point in planetsInSign)
            {
                if (point.Key == mainRuler || point.Key == subRuler || !IsRuler(point.Key)) continue;
                if (point.Value == signMainRuler || point.Value == signSubRuler)
                {
                    var signs = FindSignsRuledBy(point.Key);
                    if (processedSigns.Contains(signs.Item1) || processedSigns.Contains(signs.Item2)) continue;
                    indirectSignCount += signCounts[signs.Item1];
                    indirectSignCount += signCounts[signs.Item2];
                    processedSigns.Add(signs.Item1);
                    processedSigns.Add(signs.Item2);
                }
            }
            var totalSignCount = directSignCount + indirectSignCount;

            // rulers in decanates

            var directDecanateCount = 0;
            if (useDecanates)
            {
                foreach (var decanateRuler in BlaSchemaDomain.DecanateRulers())
                {
                    if (decanateRuler.Ruler == mainRuler || decanateRuler.Ruler == subRuler)
                    {
                        foreach (var df in decansWithFactors)
                        {
                            if (df.Key != decanateRuler.Decan) continue;
                            directDecanateCount = df.Value.Count;
                        }
                    }
                }
            }

            // rulers in houses

            var directHouseCount = 0;
            var housesRuledByMainRuler = FindCuspsRuledBy(mainRuler, signsOnHouseCusps);
            foreach (var house in housesRuledByMainRuler)
            {
                directHouseCount += houseCounts[house];
            }

            var indirectHouseCount = 0;
            var pointsInHousesForIndirectRulership = new List<Factors>();
            foreach (var house in housesRuledByMainRuler)
            {
                foreach (var point in planetsInHouses)
                {
                    if (point.Key == mainRuler || point.Key == subRuler || !IsRuler(point.Key)) continue;
                    if (point.Value == house)
                    {
                        pointsInHousesForIndirectRulership.Add(point.Key);
                    }
                }
            }
            // remove points from same rulerPair
            var uniquePoints = new List<Factors>();
            foreach (var pair in BlaSchemaDomain.RulerPairs())
            {
                var handledPairs = new List<RulerPair>();
                foreach (var point in pointsInHousesForIndirectRulership)
                {
                    if (point != pair.MainRuler && point != pair.SubRuler) continue;
                    if (handledPairs.Contains(pair)) continue;
                    handledPairs.Add(pair);
                    if (uniquePoints.Contains(point)) continue;
                    uniquePoints.Add(point);
                }
            }

            foreach (var point in uniquePoints)
            {
                var houses = FindCuspsRuledBy(point, signsOnHouseCusps);
                foreach (var h in houses)
                {
                    indirectHouseCount += houseCounts[h];
                }
            }
            var totalHouseCount = directHouseCount + indirectHouseCount;
            var totalCount = totalSignCount + totalHouseCount + directDecanateCount;

            dispositors.Add(new BlaDispositorLine(mainRuler, subRuler, signMainRulerCount, signSubRulerCount,
                directSignCount, indirectSignCount, totalSignCount, directHouseCount, indirectHouseCount, totalHouseCount,
                directDecanateCount, totalCount));
        }
        return dispositors;
    }

    private static bool IsRuler(Factors factor)
    {
        return factor is Factors.Sun or Factors.Moon or Factors.Mercury or Factors.Venus
            or Factors.Mars or Factors.Jupiter or Factors.Neptune or Factors.Pluto
            or Factors.ApogeeMean or Factors.Priapus or Factors.PersephoneCarteret or Factors.VulcanusCarteret;
    }

    private static List<RulerPair> RulerPairsForDispositors()
    {
        return
        [
            new RulerPair(1, Factors.Mars, Factors.Pluto),
            new RulerPair(2, Factors.Venus, Factors.PersephoneCarteret),
            new RulerPair(3, Factors.Mercury, Factors.VulcanusCarteret),
            new RulerPair(4, Factors.Moon, Factors.Priapus),
            new RulerPair(5, Factors.Sun, Factors.ApogeeMean),
            new RulerPair(9, Factors.Jupiter, Factors.Neptune)
        ];
    }

    private static List<int> FindCuspsRuledBy(Factors ruler, Dictionary<int, int> signsOnCusps)
    {
        var (signMainRuler, signSubRuler) = FindSignsRuledBy(ruler);
        var cusps = new List<int>();
        foreach (var (house, sign) in signsOnCusps)
        {
            if (sign == signMainRuler || sign == signSubRuler)
            {
                cusps.Add(house);
            }
        }
        return cusps;
    }

    private static (int, int) FindSignsRuledBy(Factors ruler)
    {
        var signMain = 0;
        var signSub = 0;
        foreach (var rulers in BlaSchemaDomain.RulerPairs())
        {
            if (ruler == rulers.MainRuler) signMain = rulers.SignIndex;
            if (ruler == rulers.SubRuler) signSub = rulers.SignIndex;
        }
        return (signMain, signSub);
    }
}
