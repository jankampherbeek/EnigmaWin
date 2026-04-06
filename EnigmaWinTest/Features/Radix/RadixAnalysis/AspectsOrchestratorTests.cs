// AspectsOrchestratorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;

namespace EnigmaWintest.Features.Radix.RadixAnalysis;

/// <summary>
/// Tests for AspectsOrchestrator.
/// All tests use baseOrb=10.0 and 100% percentages unless stated otherwise.
/// maxOrb = max(factorPct1, factorPct2) / 100 * aspectPct / 100 * baseOrb
/// </summary>
[TestFixture]
public class AspectsOrchestratorTests
{
    private const double Delta = 1e-8;

    // MARK: - Happy flow

    /// <summary>Sun=0°, Moon=0° → exact conjunction, orb=0.</summary>
    [Test]
    public void TestExactConjunctionIsFound()
    {
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 0.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Aspect, Is.EqualTo(Aspects.Conjunction));
        Assert.That(Math.Abs(result[0].Orb), Is.LessThan(Delta));
    }

    /// <summary>Sun=0°, Moon=180° → exact opposition, orb=0.</summary>
    [Test]
    public void TestExactOppositionIsFound()
    {
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 180.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Opposition]),
            OrbConf());

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Aspect, Is.EqualTo(Aspects.Opposition));
        Assert.That(Math.Abs(result[0].Orb), Is.LessThan(Delta));
    }

    /// <summary>Sun=0°, Moon=120° → exact trine, orb=0.</summary>
    [Test]
    public void TestExactTrineIsFound()
    {
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 120.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Trine]),
            OrbConf());

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Aspect, Is.EqualTo(Aspects.Trine));
        Assert.That(Math.Abs(result[0].Orb), Is.LessThan(Delta));
    }

    /// <summary>Sun=0°, Moon=7° → conjunction within orb, orb=7.</summary>
    [Test]
    public void TestConjunctionWithinOrbHasCorrectOrb()
    {
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 7.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].Orb - 7.0), Is.LessThan(Delta));
    }

    /// <summary>FoundAspect contains correct Aspect, Orb and MaxOrb fields.</summary>
    [Test]
    public void TestFoundAspectFieldsAreCorrect()
    {
        // Sun=0, Moon=120: exact trine; all 100%, baseOrb=10 → maxOrb=10.0
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 120.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Trine]),
            OrbConf(baseOrb: 10.0));

        Assert.That(result.Count, Is.EqualTo(1));
        var found = result[0];
        Assert.That(found.Aspect, Is.EqualTo(Aspects.Trine));
        Assert.That(Math.Abs(found.Orb), Is.LessThan(Delta));
        Assert.That(Math.Abs(found.MaxOrb - 10.0), Is.LessThan(Delta));
    }

    /// <summary>Results are sorted by Orb ascending (most exact first).</summary>
    [Test]
    public void TestResultSortedByOrbAscending()
    {
        // Sun=0, Moon=3 (conj orb=3), Mars=178 (opp from Sun orb=2, opp from Moon orb=5)
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 3.0), (Factors.Mars, 178.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon, Factors.Mars]),
            AspectConf([Aspects.Conjunction, Aspects.Opposition]),
            OrbConf());

        Assert.That(result.Count, Is.GreaterThanOrEqualTo(2));
        for (var i = 0; i < result.Count - 1; i++)
            Assert.That(result[i].Orb, Is.LessThanOrEqualTo(result[i + 1].Orb));
    }

    /// <summary>Wrap-around distance near 0°/360° boundary is handled correctly.</summary>
    [Test]
    public void TestWrapAroundDistanceNearZero()
    {
        // Sun=355°, Moon=5°: |355-5|=350 > 180 → shortestDistance=10 → conjunction orb=10 (at boundary)
        var chart = MakeChart([(Factors.Sun, 355.0), (Factors.Moon, 5.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Aspect, Is.EqualTo(Aspects.Conjunction));
        Assert.That(Math.Abs(result[0].Orb - 10.0), Is.LessThan(Delta));
    }

    // MARK: - Orb boundary

    /// <summary>Deviation exactly equal to maxOrb is included (<=).</summary>
    [Test]
    public void TestDeviationAtMaxOrbBoundaryIsIncluded()
    {
        // Sun=0, Moon=10: deviation=10; maxOrb=10 → found
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 10.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result.Count, Is.EqualTo(1));
    }

    /// <summary>Deviation just outside maxOrb is excluded.</summary>
    [Test]
    public void TestDeviationJustOutsideMaxOrbIsExcluded()
    {
        // Sun=0, Moon=11: deviation=11 > maxOrb=10 → not found
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 11.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result, Is.Empty);
    }

    // MARK: - Guard conditions

    /// <summary>Empty chart (no coordinates) returns empty result.</summary>
    [Test]
    public void TestEmptyChartReturnsEmpty()
    {
        var chart = MakeChart([]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result, Is.Empty);
    }

    /// <summary>Only one active factor → no pairs → empty result.</summary>
    [Test]
    public void TestSingleFactorReturnsEmpty()
    {
        var chart = MakeChart([(Factors.Sun, 0.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun]),
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result, Is.Empty);
    }

    /// <summary>All factors marked IsUsed=false → empty result.</summary>
    [Test]
    public void TestNoUsedFactorsReturnsEmpty()
    {
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 0.0)]);
        var factorConfig = new FactorConfig(
        [
            new FactorSettings(Factors.Sun,  IsUsed: false, IsDrawn: false),
            new FactorSettings(Factors.Moon, IsUsed: false, IsDrawn: false)
        ]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            factorConfig,
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result, Is.Empty);
    }

    /// <summary>All aspects marked IsUsed=false → empty result.</summary>
    [Test]
    public void TestNoUsedAspectsReturnsEmpty()
    {
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 0.0)]);
        var aspectConfig = new AspectConfig(
        [
            new AspectSettings(Aspects.Conjunction, IsUsed: false, IsDrawn: false,
                OrbPercentage: 100, Color: new ColorConfig(0, 0, 1))
        ]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            aspectConfig,
            OrbConf());

        Assert.That(result, Is.Empty);
    }

    /// <summary>Factor in config but absent from chart is silently skipped; remaining pair is still found.</summary>
    [Test]
    public void TestFactorAbsentFromChartIsSkipped()
    {
        // Mercury is in factorConfig but not in chart; Sun-Moon conjunction still found
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 0.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon, Factors.Mercury]),
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Aspect, Is.EqualTo(Aspects.Conjunction));
    }

    // MARK: - Orb percentage

    /// <summary>Aspect orbPercentage=50 halves the allowed orb; deviation=6 exceeds it.</summary>
    [Test]
    public void TestAspectOrbPercentageReducesOrb()
    {
        // maxOrb = 1.0 * 0.5 * 10 = 5 → deviation=6 > 5 → not found
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 6.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Conjunction], orbPct: 50),
            OrbConf());

        Assert.That(result, Is.Empty);
    }

    /// <summary>Aspect orbPercentage=100 → maxOrb=10; deviation=6 found.</summary>
    [Test]
    public void TestAspectOrbPercentageFull()
    {
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 6.0)]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Conjunction], orbPct: 100),
            OrbConf());

        Assert.That(result.Count, Is.EqualTo(1));
    }

    /// <summary>Both factors at 50% → max(0.5,0.5)=0.5 → maxOrb=5; deviation=6 not found.</summary>
    [Test]
    public void TestBothFactorOrbPercentagesReducedExcludesAspect()
    {
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 6.0)]);
        var factorConfig = new FactorConfig(
        [
            new FactorSettings(Factors.Sun,  IsUsed: true, IsDrawn: false, OrbPercentage: 50),
            new FactorSettings(Factors.Moon, IsUsed: true, IsDrawn: false, OrbPercentage: 50)
        ]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            factorConfig,
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result, Is.Empty);
    }

    /// <summary>Sun=100%, Moon=50% → max(1.0,0.5)=1.0 → maxOrb=10; deviation=6 found.</summary>
    [Test]
    public void TestHigherFactorOrbPercentageDominatesViaMax()
    {
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 6.0)]);
        var factorConfig = new FactorConfig(
        [
            new FactorSettings(Factors.Sun,  IsUsed: true, IsDrawn: false, OrbPercentage: 100),
            new FactorSettings(Factors.Moon, IsUsed: true, IsDrawn: false, OrbPercentage: 50)
        ]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            factorConfig,
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result.Count, Is.EqualTo(1));
    }

    /// <summary>Larger baseOrb allows aspects that a smaller baseOrb excludes.</summary>
    [Test]
    public void TestBaseOrbScaling()
    {
        // Sun=0, Moon=12; baseOrb=10 → maxOrb=10 < 12 → not found; baseOrb=15 → maxOrb=15 ≥ 12 → found
        var chart = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 12.0)]);
        var smallOrbResult = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Conjunction]),
            OrbConf(baseOrb: 10.0));
        var largeOrbResult = AspectsOrchestrator.Calculate(
            chart,
            FactorConf([Factors.Sun, Factors.Moon]),
            AspectConf([Aspects.Conjunction]),
            OrbConf(baseOrb: 15.0));

        Assert.That(smallOrbResult, Is.Empty);
        Assert.That(largeOrbResult.Count, Is.EqualTo(1));
    }

    // MARK: - Mundane factors

    /// <summary>Ascendant (mundane) position is read from HousePositions, not Coordinates.</summary>
    [Test]
    public void TestAscendantMundaneFactorIsIncluded()
    {
        // Sun at 0°, Ascendant at 0° in HousePositions → conjunction found
        var ascendant = MakeCusp(0.0);
        var dummy = MakeCusp(0.0);
        var houses = new HousePositions([], ascendant, dummy, dummy, dummy);
        var coords = new Dictionary<Factors, FullFactorPosition>
        {
            [Factors.Sun] = MakePosition(0.0)
        };
        var chart = new FullChart(coords, houses, 0, 0, 23.5);
        var factorConfig = new FactorConfig(
        [
            new FactorSettings(Factors.Sun,       IsUsed: true, IsDrawn: false, OrbPercentage: 100),
            new FactorSettings(Factors.Ascendant,  IsUsed: true, IsDrawn: false, OrbPercentage: 100)
        ]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            factorConfig,
            AspectConf([Aspects.Conjunction]),
            OrbConf());

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Aspect, Is.EqualTo(Aspects.Conjunction));
    }

    /// <summary>MC (mundane) at 90° from Sun → square found.</summary>
    [Test]
    public void TestMcMundaneFactorSquareFound()
    {
        var mc = MakeCusp(90.0);
        var dummy = MakeCusp(0.0);
        var houses = new HousePositions([], dummy, mc, dummy, dummy);
        var coords = new Dictionary<Factors, FullFactorPosition>
        {
            [Factors.Sun] = MakePosition(0.0)
        };
        var chart = new FullChart(coords, houses, 0, 0, 23.5);
        var factorConfig = new FactorConfig(
        [
            new FactorSettings(Factors.Sun, IsUsed: true, IsDrawn: false, OrbPercentage: 100),
            new FactorSettings(Factors.Mc,  IsUsed: true, IsDrawn: false, OrbPercentage: 100)
        ]);
        var result = AspectsOrchestrator.Calculate(
            chart,
            factorConfig,
            AspectConf([Aspects.Square]),
            OrbConf());

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Aspect, Is.EqualTo(Aspects.Square));
    }

    // MARK: - Helpers

    private static FullChart MakeChart(IEnumerable<(Factors Factor, double Longitude)> pairs)
    {
        var dummy = MakeCusp(0.0);
        var houses = new HousePositions([], dummy, dummy, dummy, dummy);
        var coords = new Dictionary<Factors, FullFactorPosition>();
        foreach (var (factor, lon) in pairs)
            coords[factor] = MakePosition(lon);
        return new FullChart(coords, houses, 0, 0, 23.5);
    }

    private static FullFactorPosition MakePosition(double longitude) =>
        new(
            Ecliptical: [new MainAstronomicalPosition(longitude, 0, 1)],
            Equatorial: [],
            Horizontal: []);

    private static FullCuspPosition MakeCusp(double longitude) =>
        new(longitude, 0, 0, new HorizontalPosition(0, 0));

    private static FactorConfig FactorConf(IEnumerable<Factors> factors, int orbPct = 100) =>
        new(factors.Select(f => new FactorSettings(f, IsUsed: true, IsDrawn: false, OrbPercentage: orbPct)).ToList());

    private static AspectConfig AspectConf(IEnumerable<Aspects> aspects, int orbPct = 100) =>
        new(aspects.Select(a => new AspectSettings(
            a, IsUsed: true, IsDrawn: false,
            OrbPercentage: orbPct,
            Color: AspectSettings.DefaultColor(a))).ToList());

    private static OrbConfig OrbConf(double baseOrb = 10.0) =>
        new(OrbSystems.Procentual, AspectBaseOrb: baseOrb, Midpoint360DialOrb: 1.5, Midpoint90DialOrb: 1.0, Midpoint45DialOrb: 0.5, HarmonicOrb: 2.0, ParallelOrb: 1.0, DeclinationMidpointOrb: 0.75);
}
