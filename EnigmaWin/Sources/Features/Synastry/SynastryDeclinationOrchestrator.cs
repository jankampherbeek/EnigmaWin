// SynastryDeclinationOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Progressive;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

namespace EnigmaWin.Sources.Features.Synastry;

/// <summary>
/// Calculates parallels and contra-parallels between the declinations of two different charts
/// (synastry). Factor1 in each FoundParallel is chart A's factor; Factor2 is chart B's factor.
/// </summary>
public static class SynastryDeclinationOrchestrator
{
    public static List<FoundParallel> Calculate(
        FullChart chartA, FullChart chartB, FactorConfig factorConfig, double parallelOrb)
    {
        var declinationsA = ActiveDeclinations(chartA, factorConfig);
        var declinationsB = ActiveDeclinations(chartB, factorConfig);
        if (declinationsA.Count == 0 || declinationsB.Count == 0) return [];

        var found = new List<FoundParallel>();

        foreach (var (fA, declA) in declinationsA)
        {
            foreach (var (fB, declB) in declinationsB)
            {
                var orb = Math.Abs(Math.Abs(declA) - Math.Abs(declB));
                if (orb > parallelOrb) continue;

                var isContra = (declA >= 0.0) != (declB >= 0.0);
                found.Add(new FoundParallel(fA, fB, isContra, orb, parallelOrb));
            }
        }

        return [.. found.OrderBy(p => p.Orb)];
    }

    private static List<(Factors Factor, double Declination)> ActiveDeclinations(
        FullChart chart, FactorConfig factorConfig)
    {
        return factorConfig.Settings
            .Where(s => s.IsUsed)
            .Select(s => (s.Factor, Decl: DeclMidpointsCalculator.GetDeclination(s.Factor, chart)))
            .Where(x => x.Decl.HasValue)
            .Select(x => (x.Factor, x.Decl!.Value))
            .ToList();
    }
}
