// HarmonicsOrchestratorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.Harmonics;

/// <summary>
/// Tests for HarmonicsOrchestrator.
/// Helpers mirror AspectsOrchestratorTests for consistency.
/// </summary>
[TestFixture]
public class HarmonicsOrchestratorTests
{
    private const double Delta = 1e-8;

    // MARK: - Calculate: happy flow

    /// <summary>Sun=30° with harmonic 3 → 90°.</summary>
    [Test]
    public void TestCalculateReturnsCorrectHarmonicLongitude()
    {
        var chart  = MakeChart([(Factors.Sun, 30.0)]);
        var config = MakeConfig([Factors.Sun], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Calculate(chart, config, 3.0);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Factor, Is.EqualTo(Factors.Sun));
        Assert.That(Math.Abs(result[0].Longitude - 90.0), Is.LessThan(Delta));
    }

    /// <summary>Result is reduced to 0–360°: Moon=200° × 2 = 400° → 40°.</summary>
    [Test]
    public void TestCalculateReducesResultBelow360()
    {
        var chart  = MakeChart([(Factors.Moon, 200.0)]);
        var config = MakeConfig([Factors.Moon], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Calculate(chart, config, 2.0);

        Assert.That(Math.Abs(result[0].Longitude - 40.0), Is.LessThan(Delta));
    }

    /// <summary>All active factors are returned, in config order.</summary>
    [Test]
    public void TestCalculateReturnsAllActiveFactors()
    {
        var chart  = MakeChart([(Factors.Sun, 30.0), (Factors.Moon, 90.0), (Factors.Mars, 150.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mars], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Calculate(chart, config, 3.0);

        Assert.That(result.Count, Is.EqualTo(3));
    }

    /// <summary>Factor marked IsUsed=false is excluded from the result.</summary>
    [Test]
    public void TestCalculateExcludesUnusedFactors()
    {
        var chart = MakeChart([(Factors.Sun, 30.0), (Factors.Moon, 90.0)]);
        var config = new UserConfiguration
        {
            FactorConfig = new FactorConfig(
            [
                new FactorSettings(Factors.Sun,  IsUsed: true,  IsDrawn: false),
                new FactorSettings(Factors.Moon, IsUsed: false, IsDrawn: false)
            ]),
            OrbConfig = OrbConf(harmonicOrb: 2.0)
        };
        var result = HarmonicsOrchestrator.Calculate(chart, config, 3.0);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Factor, Is.EqualTo(Factors.Sun));
    }

    /// <summary>Factor in config but absent from chart is silently skipped.</summary>
    [Test]
    public void TestCalculateSkipsFactorAbsentFromChart()
    {
        var chart  = MakeChart([(Factors.Sun, 30.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Calculate(chart, config, 3.0);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Factor, Is.EqualTo(Factors.Sun));
    }

    // MARK: - Calculate: guard conditions

    /// <summary>Empty chart returns empty list.</summary>
    [Test]
    public void TestCalculateEmptyChartReturnsEmpty()
    {
        var chart  = MakeChart([]);
        var config = MakeConfig([Factors.Sun], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Calculate(chart, config, 3.0);

        Assert.That(result, Is.Empty);
    }

    /// <summary>No used factors in config returns empty list.</summary>
    [Test]
    public void TestCalculateNoUsedFactorsReturnsEmpty()
    {
        var chart = MakeChart([(Factors.Sun, 30.0)]);
        var config = new UserConfiguration
        {
            FactorConfig = new FactorConfig(
                [new FactorSettings(Factors.Sun, IsUsed: false, IsDrawn: false)]),
            OrbConfig = OrbConf(harmonicOrb: 2.0)
        };
        var result = HarmonicsOrchestrator.Calculate(chart, config, 3.0);

        Assert.That(result, Is.Empty);
    }

    // MARK: - Calculate: mundane factors

    /// <summary>Ascendant (mundane) longitude is read from HousePositions.</summary>
    [Test]
    public void TestCalculateIncludesAscendantFromHousePositions()
    {
        var asc    = MakeCusp(60.0);
        var dummy  = MakeCusp(0.0);
        var houses = new HousePositions([], asc, dummy, dummy, dummy);
        var coords = new Dictionary<Factors, FullFactorPosition>();
        var chart  = new FullChart(coords, houses, 0, 0, 23.5);
        var config = new UserConfiguration
        {
            FactorConfig = new FactorConfig(
                [new FactorSettings(Factors.Ascendant, IsUsed: true, IsDrawn: false)]),
            OrbConfig = OrbConf(harmonicOrb: 2.0)
        };
        var result = HarmonicsOrchestrator.Calculate(chart, config, 3.0);

        // Asc=60° × 3 = 180°
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Factor, Is.EqualTo(Factors.Ascendant));
        Assert.That(Math.Abs(result[0].Longitude - 180.0), Is.LessThan(Delta));
    }

    // MARK: - Matches: happy flow

    /// <summary>
    /// Sun=30° and Moon=90°, harmonic 3: Sun harmonic=90° coincides exactly with radix Moon=90°.
    /// </summary>
    [Test]
    public void TestMatchesFindsExactMatch()
    {
        var chart  = MakeChart([(Factors.Sun, 30.0), (Factors.Moon, 90.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Matches(chart, config, 3.0);

        var match = result.Find(m => m.HarmonicFactor == Factors.Sun
                                  && m.RadixFactor    == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 0.0), Is.LessThan(Delta));
    }

    /// <summary>Match within orb: harmonic at 90°, radix factor at 91° → orb 1° ≤ 2°.</summary>
    [Test]
    public void TestMatchesFindsMatchWithinOrb()
    {
        var chart  = MakeChart([(Factors.Sun, 30.0), (Factors.Moon, 91.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Matches(chart, config, 3.0);

        var match = result.Find(m => m.HarmonicFactor == Factors.Sun
                                  && m.RadixFactor    == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 1.0), Is.LessThan(Delta));
    }

    /// <summary>Radix factor outside orb is not returned.</summary>
    [Test]
    public void TestMatchesExcludesMatchOutsideOrb()
    {
        var chart  = MakeChart([(Factors.Sun, 30.0), (Factors.Moon, 95.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Matches(chart, config, 3.0);

        var match = result.Find(m => m.HarmonicFactor == Factors.Sun
                                  && m.RadixFactor    == Factors.Moon);
        Assert.That(match, Is.Null);
    }

    /// <summary>MaxOrb stored in every match equals OrbConfig.HarmonicOrb.</summary>
    [Test]
    public void TestMatchesMaxOrbEqualsConfiguredHarmonicOrb()
    {
        var chart  = MakeChart([(Factors.Sun, 30.0), (Factors.Moon, 90.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Matches(chart, config, 3.0);

        Assert.That(result, Is.Not.Empty);
        foreach (var m in result)
            Assert.That(Math.Abs(m.MaxOrb - 2.0), Is.LessThan(Delta));
    }

    /// <summary>Results are sorted by ActualOrb ascending (most exact first).</summary>
    [Test]
    public void TestMatchesResultSortedByOrb()
    {
        // Sun=30, Moon=90, Mars=150, Jupiter=200 with harmonic 3
        var chart  = MakeChart([(Factors.Sun, 30.0), (Factors.Moon, 90.0),
                                (Factors.Mars, 150.0), (Factors.Jupiter, 200.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mars, Factors.Jupiter],
                                harmonicOrb: 60.0);
        var result = HarmonicsOrchestrator.Matches(chart, config, 3.0);

        for (var i = 0; i < result.Count - 1; i++)
            Assert.That(result[i].ActualOrb, Is.LessThanOrEqualTo(result[i + 1].ActualOrb));
    }

    /// <summary>A factor is never matched against its own harmonic position.</summary>
    [Test]
    public void TestMatchesSelfMatchIsExcluded()
    {
        var chart  = MakeChart([(Factors.Sun, 30.0), (Factors.Moon, 90.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Matches(chart, config, 3.0);

        Assert.That(result.Find(m => m.HarmonicFactor == m.RadixFactor), Is.Null);
    }

    /// <summary>Wrap-around: harmonic at 1°, radix Moon at 359° → orb 2° ≤ configuredOrb.</summary>
    [Test]
    public void TestMatchesWrapAroundNearZero()
    {
        // Sun=120° → harmonic 3 = 360° → 0°; move Sun slightly to get harmonic = 1/3°
        // Use explicit positions: harmonic factor = Sun at lon so that Sun*3 ≈ 1°,
        // radix Moon = 359°. Easier to drive via direct positions.
        // Sun at 120.333...° × 3 = 361° → 1°; Moon at 359° → deviation = 2°.
        var chart  = MakeChart([(Factors.Sun, 120.0 + 1.0 / 3.0), (Factors.Moon, 359.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Matches(chart, config, 3.0);

        var match = result.Find(m => m.HarmonicFactor == Factors.Sun
                                  && m.RadixFactor    == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 2.0), Is.LessThan(1e-6));
    }

    // MARK: - Matches: guard conditions

    /// <summary>Only one active factor → fewer than two → empty result.</summary>
    [Test]
    public void TestMatchesSingleFactorReturnsEmpty()
    {
        var chart  = MakeChart([(Factors.Sun, 30.0)]);
        var config = MakeConfig([Factors.Sun], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Matches(chart, config, 3.0);

        Assert.That(result, Is.Empty);
    }

    /// <summary>Empty chart returns empty result.</summary>
    [Test]
    public void TestMatchesEmptyChartReturnsEmpty()
    {
        var chart  = MakeChart([]);
        var config = MakeConfig([Factors.Sun, Factors.Moon], harmonicOrb: 2.0);
        var result = HarmonicsOrchestrator.Matches(chart, config, 3.0);

        Assert.That(result, Is.Empty);
    }

    /// <summary>No used factors returns empty result.</summary>
    [Test]
    public void TestMatchesNoUsedFactorsReturnsEmpty()
    {
        var chart = MakeChart([(Factors.Sun, 30.0), (Factors.Moon, 90.0)]);
        var config = new UserConfiguration
        {
            FactorConfig = new FactorConfig(
            [
                new FactorSettings(Factors.Sun,  IsUsed: false, IsDrawn: false),
                new FactorSettings(Factors.Moon, IsUsed: false, IsDrawn: false)
            ]),
            OrbConfig = OrbConf(harmonicOrb: 2.0)
        };
        var result = HarmonicsOrchestrator.Matches(chart, config, 3.0);

        Assert.That(result, Is.Empty);
    }

    /// <summary>Larger orb finds matches that a smaller orb misses.</summary>
    [Test]
    public void TestMatchesOrbScaling()
    {
        // Sun harmonic=90°, Moon=93° → deviation=3°; orb=2 misses, orb=4 finds
        var chart       = MakeChart([(Factors.Sun, 30.0), (Factors.Moon, 93.0)]);
        var smallConfig = MakeConfig([Factors.Sun, Factors.Moon], harmonicOrb: 2.0);
        var largeConfig = MakeConfig([Factors.Sun, Factors.Moon], harmonicOrb: 4.0);

        var smallResult = HarmonicsOrchestrator.Matches(chart, smallConfig, 3.0);
        var largeResult = HarmonicsOrchestrator.Matches(chart, largeConfig, 3.0);

        Assert.That(smallResult.Find(m => m.HarmonicFactor == Factors.Sun
                                       && m.RadixFactor    == Factors.Moon), Is.Null);
        Assert.That(largeResult.Find(m => m.HarmonicFactor == Factors.Sun
                                       && m.RadixFactor    == Factors.Moon), Is.Not.Null);
    }

    // MARK: - Helpers

    private static FullChart MakeChart(IEnumerable<(Factors Factor, double Longitude)> pairs)
    {
        var dummy  = MakeCusp(0.0);
        var houses = new HousePositions([], dummy, dummy, dummy, dummy);
        var coords = new Dictionary<Factors, FullFactorPosition>();
        foreach (var (factor, lon) in pairs)
            coords[factor] = MakePosition(lon);
        return new FullChart(coords, houses, 0, 0, 23.5);
    }

    private static FullFactorPosition MakePosition(double longitude) =>
        new(Ecliptical: [new MainAstronomicalPosition(longitude, 0, 1)],
            Equatorial: [],
            Horizontal: []);

    private static FullCuspPosition MakeCusp(double longitude) =>
        new(longitude, 0, 0, new HorizontalPosition(0, 0));

    private static UserConfiguration MakeConfig(IEnumerable<Factors> factors, double harmonicOrb) =>
        new()
        {
            FactorConfig = new FactorConfig(
                factors.Select(f => new FactorSettings(f, IsUsed: true, IsDrawn: false)).ToList()),
            OrbConfig = OrbConf(harmonicOrb)
        };

    private static OrbConfig OrbConf(double harmonicOrb) =>
        new(OrbSystems.Procentual,
            AspectBaseOrb:      10.0,
            Midpoint360DialOrb: 1.5,
            Midpoint90DialOrb:  1.0,
            Midpoint45DialOrb:  0.5,
            HarmonicOrb:        harmonicOrb,
            ParallelOrb:        1.0);
}
