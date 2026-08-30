// BlaCycles.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Cycles for BLA schema calculations, ported from Enigma.Core/Slices/BlaSchema/BlaCycles.cs.</summary>
public static class BlaCycles
{
    /// <summary>Create cycles data.</summary>
    public static BlaCyclesData CreateCyclesData(Dictionary<Factors, int> planetsInHouses, Dictionary<int, int> signsOnCusps)
    {
        var rulersForHouses = CreateRulersForHouses(signsOnCusps);
        var rulerHouseRuledHouse = RuledHouseRulerInHouse(rulersForHouses, planetsInHouses);
        var cardinal = FindCycles([1, 4, 7, 10], rulerHouseRuledHouse);
        var fix = FindCycles([2, 5, 8, 11], rulerHouseRuledHouse);
        var mutable = FindCycles([3, 6, 9, 12], rulerHouseRuledHouse);
        var fire = FindCycles([1, 5, 9], rulerHouseRuledHouse);
        var earth = FindCycles([2, 6, 10], rulerHouseRuledHouse);
        var air = FindCycles([3, 7, 11], rulerHouseRuledHouse);
        var water = FindCycles([4, 8, 12], rulerHouseRuledHouse);

        return new BlaCyclesData(cardinal, fix, mutable, fire, earth, air, water);
    }

    /// <summary>Create shortened cycles data.</summary>
    public static BlaCyclesData CreateShortenedCyclesData(Dictionary<Factors, int> planetsInHouses, Dictionary<int, int> signsOnCusps)
    {
        var rulersForHouses = CreateRulersForHouses(signsOnCusps);
        var cardinal = FindShortenedCycles([1, 4, 7, 10], rulersForHouses);
        var fix = FindShortenedCycles([2, 5, 8, 11], rulersForHouses);
        var mutable = FindShortenedCycles([3, 6, 9, 12], rulersForHouses);
        var fire = FindShortenedCycles([1, 5, 9], rulersForHouses);
        var earth = FindShortenedCycles([2, 6, 10], rulersForHouses);
        var air = FindShortenedCycles([3, 7, 11], rulersForHouses);
        var water = FindShortenedCycles([4, 8, 12], rulersForHouses);
        return new BlaCyclesData(cardinal, fix, mutable, fire, earth, air, water);
    }

    // Return a dictionary with the index of the cusp and a list of rulers
    private static Dictionary<int, List<Factors>> CreateRulersForHouses(Dictionary<int, int> signsOnCusps)
    {
        var rulersForHouses = new Dictionary<int, List<Factors>>();
        foreach (var cusps in signsOnCusps)
        {
            var rulerPair = BlaSchemaDomain.RulerPairs()[cusps.Value - 1];
            rulersForHouses.Add(cusps.Key, [rulerPair.MainRuler, rulerPair.SubRuler]);
        }
        return rulersForHouses;
    }

    // Return a list with factors, the house it rules and the house where it is located
    private static List<(Factors, int, int)> RuledHouseRulerInHouse(
        Dictionary<int, List<Factors>> rulersForHouses,
        Dictionary<Factors, int> planetsInHouses)
    {
        var rulerHouseRuledHouse = new List<(Factors, int, int)>();
        foreach (var ruler in rulersForHouses)
        {
            foreach (var house in ruler.Value)
            {
                rulerHouseRuledHouse.Add((house, ruler.Key, planetsInHouses[house]));
            }
        }
        return rulerHouseRuledHouse;
    }

    // Find cycles in a specific group of houses
    private static List<(int, int)> FindCycles(List<int> houses, List<(Factors, int, int)> ruledHousesRulerInHouse)
    {
        var cycles = new List<(int, int)>();
        foreach (var ruler in ruledHousesRulerInHouse)
        {
            if (houses.Contains(ruler.Item2) && houses.Contains(ruler.Item3))
            {
                cycles.Add((ruler.Item2, ruler.Item3));
            }
        }
        return cycles;
    }

    // Find shortened cycles in a specific group of houses: a shortened cycle exists if houses from the
    // same element or cross are ruled by points from the same ruler pair.
    private static List<(int, int)> FindShortenedCycles(List<int> houses, Dictionary<int, List<Factors>> rulersForHouses)
    {
        var cycles = new List<(int, int)>();

        var rulers = new List<(Factors, int)>();
        foreach (var house in houses)
        {
            var ruler = rulersForHouses[house];
            rulers.Add((ruler[0], house));
            rulers.Add((ruler[1], house));
        }

        for (var i = 0; i < rulers.Count; i++)
        {
            for (var j = i + 1; j < rulers.Count; j++)
            {
                foreach (var rulerPair in BlaSchemaDomain.RulerPairs())
                {
                    if ((rulerPair.MainRuler != rulers[i].Item1 || rulerPair.SubRuler != rulers[j].Item1) &&
                        (rulerPair.MainRuler != rulers[j].Item1 || rulerPair.SubRuler != rulers[i].Item1)) continue;
                    if (rulers[i].Item2 == rulers[j].Item2) continue;
                    if (!houses.Contains(rulers[i].Item2) || !houses.Contains(rulers[j].Item2)) continue;
                    if (!cycles.Contains((rulers[i].Item2, rulers[j].Item2)))
                        cycles.Add((rulers[i].Item2, rulers[j].Item2));
                }
            }
        }
        return cycles;
    }
}
