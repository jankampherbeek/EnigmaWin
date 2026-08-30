// BlaDetails.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Details for the BLA schema, ported from Enigma.Core/Slices/BlaSchema/BlaDetails.cs.</summary>
public static class BlaDetails
{
    /// <summary>Create the details for the BLA schema.</summary>
    public static BlaDetailsData CreateDetails(
        Dictionary<int, int> signsOnCusps,
        Dictionary<Factors, int> planetsInHouses)
    {
        var asc = signsOnCusps[1];
        var ascRulers = BlaSchemaDomain.RulerPairs()[asc - 1];
        var subRulerAsc = ascRulers.SubRuler;
        var sisterSignAsc = 0;
        foreach (var rulerPair in BlaSchemaDomain.RulerPairs())
        {
            if (rulerPair.MainRuler == subRulerAsc)
            {
                sisterSignAsc = rulerPair.SignIndex;
            }
        }

        var clampedHouses = InterceptedClamped.DefineClampedHouses(signsOnCusps);
        var interceptedSigns = InterceptedClamped.DefineInterceptedSigns(signsOnCusps);

        var groundNote = new List<int> { 1, asc };
        foreach (var rulerPair in BlaSchemaDomain.RulerPairs())
        {
            if (rulerPair.MainRuler == subRulerAsc)
            {
                var signIndex = rulerPair.SignIndex;
                foreach (var cuspSign in signsOnCusps)
                {
                    if (cuspSign.Value == signIndex)
                    {
                        groundNote.Add(cuspSign.Key);
                    }
                }
            }
        }
        foreach (var sign in signsOnCusps)
        {
            if (sign.Key != 1 && sign.Value == asc)
            {
                groundNote.Add(sign.Key);
            }
        }

        var lordAscInHouses = new List<int>
        {
            planetsInHouses[ascRulers.MainRuler],
            planetsInHouses[ascRulers.SubRuler]
        };

        var moonInHouse = planetsInHouses[Factors.Moon];

        return new BlaDetailsData(sisterSignAsc, clampedHouses, interceptedSigns, groundNote, lordAscInHouses, moonInHouse);
    }
}
