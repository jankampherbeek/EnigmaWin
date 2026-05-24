// DeclinationsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

public static class DeclinationsOrchestrator
{
    public static List<DefinedParallel> Parallels(
        FullChart chart,
        FactorConfig factorConfig,
        OrbConfig orbConfig)
        => ParallelsCalculator.Calculate(chart, factorConfig, orbConfig);

    public static List<DeclOccupiedMidpoint> OccupiedMidpoints(
        FullChart chart,
        FactorConfig factorConfig,
        OrbConfig orbConfig)
    {
        var baseMidpoints = DeclMidpointsCalculator.BaseMidpoints(chart, factorConfig);
        var positions     = DeclMidpointsCalculator.ActivePairs(chart, factorConfig);
        return DeclinationMidpointsMatchFinder.Find(baseMidpoints, positions, orbConfig.DeclinationMidpointOrb);
    }

    public static List<LongEquivalentResult> LongitudeEquivalents(
        FullChart chart,
        FactorConfig factorConfig)
        => LongEquivalentsCalculator.Calculate(chart, factorConfig);
}
