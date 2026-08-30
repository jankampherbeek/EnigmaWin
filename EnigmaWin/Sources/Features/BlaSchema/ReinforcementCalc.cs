// ReinforcementCalc.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Calculations to support the construction of reinforcements for the BLA schema,
/// ported from Enigma.Core/Slices/BlaSchema/ReinforcementCalc.cs.</summary>
public static class ReinforcementCalc
{
    /// <summary>Data for receptions in houses.</summary>
    private sealed record HousesPosAndRuler(Factors Point, int HousePos, List<int> HouseRuled);

    /// <summary>Finds points that are in the sign they rule.</summary>
    public static Dictionary<Factors, int> FindPointsInOwnSign(Dictionary<Factors, int> planetsInSigns)
    {
        var pointsInOwnSign = new Dictionary<Factors, int>();
        foreach (var (signMain, mainRuler, subRuler) in BlaSchemaDomain.RulerPairs())
        {
            foreach (var planet in planetsInSigns)
            {
                if ((planet.Key == mainRuler || planet.Key == subRuler) && planet.Value == signMain)
                {
                    pointsInOwnSign.Add(planet.Key, planet.Value);
                }
            }
        }
        return pointsInOwnSign;
    }

    /// <summary>Finds points that are in the house they rule.</summary>
    public static Dictionary<Factors, int> FindPointsInOwnHouse(Dictionary<Factors, int> planetsInHouses, Dictionary<int, int> signsOnCusps)
    {
        var pointsInOwnHouse = new Dictionary<Factors, int>();

        foreach (var planetInHouse in planetsInHouses)
        {
            Factors? mainRuler = null;
            Factors? subRuler = null;
            foreach (var rulerPair in BlaSchemaDomain.RulerPairs())
            {
                if (rulerPair.SignIndex != signsOnCusps[planetInHouse.Value]) continue;
                mainRuler = rulerPair.MainRuler;
                subRuler = rulerPair.SubRuler;
            }

            if (mainRuler == null || subRuler == null) continue;
            if (planetInHouse.Key == mainRuler || planetInHouse.Key == subRuler)
            {
                pointsInOwnHouse.Add(planetInHouse.Key, planetInHouse.Value);
            }
        }
        return pointsInOwnHouse;
    }

    /// <summary>Find points that are in the mundane houses they rule.</summary>
    public static Dictionary<Factors, int> FindPointsInMundaneHouses(Dictionary<Factors, int> planetsInHouses)
    {
        var pointsInMundaneHouse = new Dictionary<Factors, int>();
        var houseRulers = BlaSchemaDomain.RulerPairs();

        foreach (var planetInHouse in planetsInHouses)
        {
            var house = planetInHouse.Value;
            var point = planetInHouse.Key;
            if (houseRulers.Any(r => (r.MainRuler == point || r.SubRuler == point) && r.SignIndex == house))
            {
                pointsInMundaneHouse.Add(planetInHouse.Key, planetInHouse.Value);
            }
        }
        return pointsInMundaneHouse;
    }

    /// <summary>Find rulers that are in a sign with the same index as the house they rule.</summary>
    public static Dictionary<Factors, int> FindRulerInHouseAsSign(Dictionary<int, int> signsOnCusps, Dictionary<Factors, int> planetsInSigns)
    {
        var signHouseRulers = new Dictionary<Factors, int>();
        foreach (var (house, sign) in signsOnCusps)
        {
            Factors? mainRulingPoint = null;
            Factors? subRulingPoint = null;
            foreach (var rulerPair in BlaSchemaDomain.RulerPairs())
            {
                if (rulerPair.SignIndex != sign) continue;
                mainRulingPoint = rulerPair.MainRuler;
                subRulingPoint = rulerPair.SubRuler;
            }
            var signMainRuler = 0;
            var signSubRuler = 0;
            foreach (var planetInSign in planetsInSigns)
            {
                if (planetInSign.Key == mainRulingPoint) signMainRuler = planetInSign.Value;
                if (planetInSign.Key == subRulingPoint) signSubRuler = planetInSign.Value;
            }
            if (mainRulingPoint != null && signMainRuler == house)
            {
                signHouseRulers.Add((Factors)mainRulingPoint, signMainRuler);
            }
            if (subRulingPoint != null && signSubRuler == house)
            {
                signHouseRulers.Add((Factors)subRulingPoint, signSubRuler);
            }
        }
        return signHouseRulers;
    }

    /// <summary>Find factor pairs with analogous house and sign.</summary>
    public static List<FactorPairAnalogHouseSign> FindFactorPairs(
        Dictionary<Factors, int> planetsInSigns,
        Dictionary<Factors, int> planetsInHouses,
        Factors northNode,
        Factors southNode)
    {
        var factorPairs = new List<FactorPairAnalogHouseSign>();

        foreach (var (point1, point2) in BlaSchemaDomain.FactorPairs(northNode, southNode))
        {
            if (planetsInSigns.ContainsKey(point1) && planetsInSigns.ContainsKey(point2)
                                                   && planetsInHouses.ContainsKey(point1)
                                                   && planetsInHouses.ContainsKey(point2))
            {
                var house1 = planetsInHouses[point1];
                var house2 = planetsInHouses[point2];
                var sign1 = planetsInSigns[point1];
                var sign2 = planetsInSigns[point2];

                if (sign1 == house2)
                {
                    factorPairs.Add(new FactorPairAnalogHouseSign(point1, sign1, point2, house2));
                }
                else if (sign2 == house1)
                {
                    factorPairs.Add(new FactorPairAnalogHouseSign(point2, sign2, point1, house1));
                }
            }
        }
        return factorPairs;
    }

    /// <summary>Find reception in signs.</summary>
    public static List<Reception> FindReceptionInSigns(Dictionary<Factors, int> planetsInSigns)
    {
        var receptions = new List<Reception>();

        foreach (var rulerPair1 in BlaSchemaDomain.RulerPairs())
        {
            var point1 = rulerPair1.MainRuler;
            foreach (var rulerPair2 in BlaSchemaDomain.RulerPairs())
            {
                var point2 = rulerPair2.MainRuler;
                if (point1 == point2) continue;
                var sign1 = 0;
                var sign2 = 0;
                foreach (var planet in planetsInSigns)
                {
                    if (planet.Key == point1) sign1 = planet.Value;
                    if (planet.Key == point2) sign2 = planet.Value;
                }

                var ruledBy1 = new List<int>();
                var ruledBy2 = new List<int>();
                foreach (var rulerPair in BlaSchemaDomain.RulerPairs())
                {
                    if (rulerPair.MainRuler == point1 || rulerPair.SubRuler == point1) ruledBy1.Add(rulerPair.SignIndex);
                    if (rulerPair.MainRuler == point2 || rulerPair.SubRuler == point2) ruledBy2.Add(rulerPair.SignIndex);
                }
                if (ruledBy1.Count < 2 || ruledBy2.Count < 2) continue;
                if ((sign1 == ruledBy2[0] || sign1 == ruledBy2[1]) && (sign2 == ruledBy1[0] || sign2 == ruledBy1[1]))
                {
                    var reception = new Reception(point1, sign1, point2, sign2);
                    var reception2 = new Reception(point2, sign2, point1, sign1);
                    if (receptions.Contains(reception) || receptions.Contains(reception2)) continue;
                    var samePair = false;
                    foreach (var rulerPair in BlaSchemaDomain.RulerPairs())
                    {
                        if ((point1 == rulerPair.MainRuler && point2 == rulerPair.SubRuler) ||
                            (point1 == rulerPair.SubRuler && point2 == rulerPair.MainRuler))
                        {
                            samePair = true;
                        }
                    }
                    if (!samePair) receptions.Add(new Reception(point1, sign1, point2, sign2));
                }
            }
        }
        return receptions;
    }

    /// <summary>Find reception in houses.</summary>
    public static List<Reception> FindReceptionInHouses(Dictionary<Factors, int> planetsInHouses, Dictionary<int, int> signsOnCusps)
    {
        var housesPosAndRuler = new List<HousesPosAndRuler>();
        foreach (var rulerAndSigns in BlaSchemaDomain.AllRulerAndSigns())
        {
            if (!planetsInHouses.TryGetValue(rulerAndSigns.Ruler, out var housePos)) continue;
            var housesRuled = new List<int>();
            foreach (var signOnCusp in signsOnCusps)
            {
                if (signOnCusp.Value == rulerAndSigns.MainSign || signOnCusp.Value == rulerAndSigns.SubSign)
                {
                    housesRuled.Add(signOnCusp.Key);
                }
            }
            if (housesRuled.Count == 0) continue; // sign is intercepted: no house is ruled
            var hpr = new HousesPosAndRuler(rulerAndSigns.Ruler, housePos, housesRuled);
            if (!housesPosAndRuler.Contains(hpr)) housesPosAndRuler.Add(hpr);
        }

        return FindMutualReceptions(housesPosAndRuler);
    }

    public static List<Reception> FindReceptionInMundaneHouses(Dictionary<Factors, int> planetsInHouses, Dictionary<int, int> signsOnCusps)
    {
        var housesPosAndRuler = new List<HousesPosAndRuler>();
        foreach (var rulerAndSigns in BlaSchemaDomain.AllRulerAndSigns())
        {
            if (!planetsInHouses.TryGetValue(rulerAndSigns.Ruler, out var housePos)) continue;
            var mundaneHousesRuled = new List<int> { rulerAndSigns.MainSign, rulerAndSigns.SubSign };
            housesPosAndRuler.Add(new HousesPosAndRuler(rulerAndSigns.Ruler, housePos, mundaneHousesRuled));
        }

        return FindMutualReceptions(housesPosAndRuler);
    }

    private static List<Reception> FindMutualReceptions(List<HousesPosAndRuler> housesPosAndRuler)
    {
        var receptions = new List<Reception>();
        var count = housesPosAndRuler.Count;

        for (var i = 0; i < count; i++)
        {
            var hpr1 = housesPosAndRuler[i];
            for (var j = i + 1; j < count; j++)
            {
                var hpr2 = housesPosAndRuler[j];
                if (hpr2.HouseRuled.Contains(hpr1.HousePos) && hpr1.HouseRuled.Contains(hpr2.HousePos))
                {
                    var samePair = false;
                    foreach (var rulerPair in BlaSchemaDomain.RulerPairs())
                    {
                        if ((hpr1.Point == rulerPair.MainRuler && hpr2.Point == rulerPair.SubRuler) ||
                            (hpr1.Point == rulerPair.SubRuler && hpr2.Point == rulerPair.MainRuler))
                        {
                            samePair = true;
                        }
                    }
                    if (!samePair) receptions.Add(new Reception(hpr1.Point, hpr1.HousePos, hpr2.Point, hpr2.HousePos));
                }
            }
        }
        return receptions;
    }
}
