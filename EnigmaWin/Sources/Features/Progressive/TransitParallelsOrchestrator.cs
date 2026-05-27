// TransitParallelsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

namespace EnigmaWin.Sources.Features.Progressive;

/// <summary>A found parallel or contra-parallel between a transit/progressive factor and a radix factor.</summary>
/// <param name="Factor1">Transit or progressive factor.</param>
/// <param name="Factor2">Radix factor.</param>
/// <param name="IsContraParallel">True when the two factors are on opposite hemispheres.</param>
/// <param name="Orb">Actual deviation from the exact parallel (0 = exact).</param>
/// <param name="MaxOrb">Maximum allowed orb.</param>
public record FoundParallel(
    Factors Factor1,
    Factors Factor2,
    bool IsContraParallel,
    double Orb,
    double MaxOrb);

/// <summary>
/// Calculates parallels and contra-parallels between transit/progressive positions and a radix chart.
/// Factor1 in each FoundParallel is the transit/progressive factor; Factor2 is the radix factor.
/// A parallel occurs when both declinations have the same sign and nearly equal absolute values.
/// A contra-parallel occurs when the declinations are nearly equal in absolute value but have opposite signs.
/// </summary>
public static class TransitParallelsOrchestrator
{
    public static List<FoundParallel> Calculate(
        Dictionary<Factors, ProgressivePosition> transitPositions,
        FullChart radixChart,
        FactorConfig factorConfig,
        double parallelOrb)
    {
        if (transitPositions.Count == 0) return [];

        var radixPairs = ActiveRadixDeclinations(radixChart, factorConfig);
        if (radixPairs.Count == 0) return [];

        var found = new List<FoundParallel>();

        foreach (var (tFactor, tPos) in transitPositions)
        {
            var tDecl = tPos.Declination;
            foreach (var (rFactor, rDecl) in radixPairs)
            {
                var orb = Math.Abs(Math.Abs(tDecl) - Math.Abs(rDecl));
                if (orb > parallelOrb) continue;

                var isContra = (tDecl >= 0.0) != (rDecl >= 0.0);
                found.Add(new FoundParallel(tFactor, rFactor, isContra, orb, parallelOrb));
            }
        }

        return [.. found.OrderBy(p => p.Orb)];
    }

    private static List<(Factors Factor, double Declination)> ActiveRadixDeclinations(
        FullChart chart,
        FactorConfig factorConfig)
    {
        return factorConfig.Settings
            .Where(s => s.IsUsed)
            .Select(s => (s.Factor, Decl: DeclMidpointsCalculator.GetDeclination(s.Factor, chart)))
            .Where(x => x.Decl.HasValue)
            .Select(x => (x.Factor, x.Decl!.Value))
            .ToList();
    }
}
