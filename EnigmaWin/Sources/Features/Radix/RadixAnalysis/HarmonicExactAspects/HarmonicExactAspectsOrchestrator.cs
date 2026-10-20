// HarmonicExactAspectsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicExactAspects;

/// <summary>Finds, for each harmonic in a range, the aspect with the smallest orb between the harmonic positions.</summary>
public static class HarmonicExactAspectsOrchestrator
{
    public const int MinHarmonic = 2;
    public const int DefaultMaxHarmonic = 12;
    public const int MaxHarmonicLimit = 1000;

    /// <summary>Orbs that differ less than this are considered equal; all of them are returned.</summary>
    private const double TieTolerance = 1e-8;

    /// <summary>Conjunction, opposition, square, trine and sextile.</summary>
    public static readonly IReadOnlyList<Domain.Aspects> MajorAspects =
    [
        Domain.Aspects.Conjunction,
        Domain.Aspects.Opposition,
        Domain.Aspects.Square,
        Domain.Aspects.Trine,
        Domain.Aspects.Sextile
    ];

    /// <summary>
    /// Returns the most exact aspect for each harmonic from <see cref="MinHarmonic"/> up to and including
    /// <paramref name="maxHarmonic"/>. Factors follow the configuration, except for the Ascendant and MC,
    /// which are included or excluded as indicated.
    /// </summary>
    public static List<HarmonicExactAspect> Calculate(
        FullChart chart,
        UserConfiguration config,
        bool includeAscendant,
        bool includeMc,
        bool useMajorAspects,
        int maxHarmonic)
    {
        var factorConfig = WithAngles(config.FactorConfig, includeAscendant, includeMc);
        var positions = HarmonicsOrchestrator.ActivePositions(chart, factorConfig);
        var aspects = useMajorAspects
            ? MajorAspects
            : config.AspectConfig.Settings.Where(s => s.IsUsed).Select(s => s.Aspect).ToList();
        return Find(positions, aspects, maxHarmonic);
    }

    /// <summary>
    /// Returns the most exact aspect for each harmonic from <see cref="MinHarmonic"/> up to and including
    /// <paramref name="maxHarmonic"/>, between the harmonic positions of the given radix positions.
    /// If several aspects within a harmonic have the same smallest orb, all of them are returned.
    /// Harmonics without any pair of factors are skipped.
    /// </summary>
    public static List<HarmonicExactAspect> Find(
        List<(Factors Factor, double Longitude)> positions,
        IReadOnlyList<Domain.Aspects> aspects,
        int maxHarmonic)
    {
        var result = new List<HarmonicExactAspect>();
        if (positions.Count < 2 || aspects.Count == 0) return result;

        for (var harmonic = MinHarmonic; harmonic <= maxHarmonic; harmonic++)
        {
            var harmonicPositions = HarmonicsCalculator.Calculate(positions, harmonic);
            var best = new List<HarmonicExactAspect>();
            var bestOrb = double.MaxValue;

            for (var i = 0; i < harmonicPositions.Count; i++)
            {
                for (var j = i + 1; j < harmonicPositions.Count; j++)
                {
                    var (f1, long1) = harmonicPositions[i];
                    var (f2, long2) = harmonicPositions[j];
                    if (ImplicitAspects.IsImplicit(f1, f2)) continue;
                    var distance = ShortestDistance(long1, long2);

                    foreach (var aspect in aspects)
                    {
                        var orb = Math.Abs(distance - aspect.Angle());
                        if (orb < bestOrb - TieTolerance)
                        {
                            bestOrb = orb;
                            best.Clear();
                        }
                        if (Math.Abs(orb - bestOrb) <= TieTolerance)
                            best.Add(new HarmonicExactAspect(harmonic, f1, f2, aspect, orb));
                    }
                }
            }

            result.AddRange(best);
        }

        return result;
    }

    private static FactorConfig WithAngles(FactorConfig factorConfig, bool includeAscendant, bool includeMc)
    {
        var settings = factorConfig.Settings.Select(s => s.Factor switch
        {
            Factors.Ascendant => s with { IsUsed = includeAscendant },
            Factors.Mc        => s with { IsUsed = includeMc },
            _                 => s
        }).ToList();
        if (includeAscendant && settings.All(s => s.Factor != Factors.Ascendant))
            settings.Add(new FactorSettings(Factors.Ascendant, IsUsed: true, IsDrawn: true));
        if (includeMc && settings.All(s => s.Factor != Factors.Mc))
            settings.Add(new FactorSettings(Factors.Mc, IsUsed: true, IsDrawn: true));
        return new FactorConfig(settings);
    }

    private static double ShortestDistance(double long1, double long2)
    {
        var diff = Math.Abs(long1 - long2) % 360.0;
        return diff > 180.0 ? 360.0 - diff : diff;
    }
}
