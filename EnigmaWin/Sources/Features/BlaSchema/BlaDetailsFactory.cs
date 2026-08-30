// BlaDetailsFactory.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Factory for BLA point/house details, ported from Enigma.Core/Slices/BlaSchema/BlaDetailsFactory.cs.</summary>
public static class BlaDetailsFactory
{
    /// <summary>Define details for a chart point in a BLA schema.</summary>
    public static BlaPointDetails CreateBlaPointDetails(Factors point, BlaChartLongitudes chart)
    {
        var longitude = chart.Points.TryGetValue(point, out var lon) ? lon : 0.0;

        var sign = (int)Math.Truncate(longitude / 30.0) + 1;
        var decanate = (int)Math.Truncate(longitude / 10.0) + 1;
        while (decanate > 7) decanate -= 7;
        var house = BlaHousePositions.FindSingleHousePosition(chart, longitude);
        var (mainRuledSign, subRuledSign) = FindSignsForRuler(point);
        var (mainRuledHouses, subRuledHouses) = FindHousesForRuler(mainRuledSign, subRuledSign, chart.Cusps);
        return new BlaPointDetails(point, longitude, sign, decanate, house, mainRuledSign, subRuledSign, mainRuledHouses, subRuledHouses);
    }

    /// <summary>Define details for a house in a BLA schema.</summary>
    public static BlaHouseDetails CreateBlaHouseDetails(int houseNr, BlaChartLongitudes chart)
    {
        var signOnCusp = FindSignOnCusp(houseNr, chart.Cusps);
        var (mainRuler, subRuler) = FindRulersForSign(signOnCusp);
        var pointsInHouse = new List<Factors>();
        var allPointsInHouses = BlaHousePositions.DefineHousePositions(chart);
        foreach (var point in allPointsInHouses)
        {
            if (point.Value == houseNr) pointsInHouse.Add(point.Key);
        }
        var longitude = chart.Cusps[houseNr];
        var decanate = (int)Math.Truncate(longitude / 10.0) + 1;
        while (decanate > 7) decanate -= 7;

        return new BlaHouseDetails(houseNr, signOnCusp, decanate, longitude, mainRuler, subRuler, pointsInHouse);
    }

    // Return signs that are ruled by a given point, main and sub, in that sequence
    private static (int, int) FindSignsForRuler(Factors point)
    {
        var signMain = 0;
        var signSub = 0;
        foreach (var rulers in BlaSchemaDomain.RulerPairs())
        {
            if (rulers.MainRuler == point) signMain = rulers.SignIndex;
            else if (rulers.SubRuler == point) signSub = rulers.SignIndex;
        }
        return (signMain, signSub);
    }

    // Return rulers for a given sign, first main ruler, then sub ruler
    private static (Factors, Factors) FindRulersForSign(int sign)
    {
        if (sign is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(sign), "Sign must be between 1 and 12");
        foreach (var rulersPair in BlaSchemaDomain.RulerPairs())
        {
            if (rulersPair.SignIndex == sign)
            {
                return (rulersPair.MainRuler, rulersPair.SubRuler);
            }
        }
        throw new Exception("Could not find rulers for sign " + sign);
    }

    // Return houses that are ruled by a given sign, main and sub, in that sequence
    private static (List<int>, List<int>) FindHousesForRuler(int mainSign, int subSign, Dictionary<int, double> houses)
    {
        var housesMain = new List<int>();
        var housesSub = new List<int>();
        foreach (var (house, longitude) in houses)
        {
            var sign = (int)Math.Truncate(longitude / 30.0) + 1;
            if (sign == mainSign) housesMain.Add(house);
            if (sign == subSign) housesSub.Add(house);
        }
        return (housesMain, housesSub);
    }

    // Return sign (1..12) on cusp for a given house
    private static int FindSignOnCusp(int houseNr, Dictionary<int, double> houses)
    {
        var signOnCusp = 0;
        foreach (var (house, longitude) in houses)
        {
            if (house == houseNr)
            {
                signOnCusp = (int)Math.Truncate(longitude / 30.0) + 1;
            }
        }
        return signOnCusp;
    }
}
