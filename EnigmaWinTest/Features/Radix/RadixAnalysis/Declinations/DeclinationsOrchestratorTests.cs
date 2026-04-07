// DeclinationsOrchestratorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.Declinations;

/// <summary>
/// Tests for DeclinationsOrchestrator.
///
/// Test data (shared between parallel and midpoint tests):
///   Sun    declination  +20.0° (north)
///   Moon   declination  +20.5° (north)  → parallel with Sun, orb 0.5°
///   Mars   declination  −20.0° (south)  → contra-parallel with Sun, orb 0.0°
///   Saturn declination  +10.0° (north)  → no parallel/contra-parallel within 1° orb
///
/// Midpoints (Sun/Moon/Mars active):
///   Sun/Moon  = (20.0 + 20.5) / 2 = +20.25°
///   Sun/Mars  = (20.0 + −20.0) / 2 =  0.0°
///   Moon/Mars = (20.5 + −20.0) / 2 = +0.25°
///
/// Longitude equivalents (obliquity = 23.4367°):
///   Sun:  decl=0°,       long=5°  → equivalent ≈ 0°,  isOutOfBounds=false
///   Moon: decl=obliquity, long=91° → equivalent ≈ 90°, isOutOfBounds=false
///   Mars: decl=30° (OOB), long=60° → isOutOfBounds=true
/// </summary>
[TestFixture]
public class DeclinationsOrchestratorTests
{
    private const double Delta      = 1e-10;
    private const double DeltaCoord = 1e-3;     // looser tolerance for trig-based equivalents
    private const double Obliquity  = 23.4367;

    // MARK: - Shared helpers

    private static FullFactorPosition MakePos(double declination) =>
        new(
            Ecliptical: [new MainAstronomicalPosition(0, 0, 1)],
            Equatorial: [new MainAstronomicalPosition(0, declination, 1)],
            Horizontal: [new HorizontalPosition(0, 0)]);

    private static FullFactorPosition MakePosLongDecl(double longitude, double declination) =>
        new(
            Ecliptical: [new MainAstronomicalPosition(longitude, 0, 1)],
            Equatorial: [new MainAstronomicalPosition(0, declination, 1)],
            Horizontal: [new HorizontalPosition(0, 0)]);

    private static FullChart MakeChart(IEnumerable<(Factors Factor, double Declination)> declinations)
    {
        var coords = new Dictionary<Factors, FullFactorPosition>();
        foreach (var (factor, decl) in declinations)
            coords[factor] = MakePos(decl);
        var dummy  = new FullCuspPosition(0, 0, 0, new HorizontalPosition(0, 0));
        var houses = new HousePositions([], dummy, dummy, dummy, dummy);
        return new FullChart(coords, houses, 0, 0, 23.45);
    }

    private static FullChart MakeChartLongDecl(
        IEnumerable<(Factors Factor, double Longitude, double Declination)> factors,
        double obliquity)
    {
        var coords = new Dictionary<Factors, FullFactorPosition>();
        foreach (var (factor, lon, decl) in factors)
            coords[factor] = MakePosLongDecl(lon, decl);
        var dummy  = new FullCuspPosition(0, 0, 0, new HorizontalPosition(0, 0));
        var houses = new HousePositions([], dummy, dummy, dummy, dummy);
        return new FullChart(coords, houses, 0, 0, obliquity);
    }

    private static FactorConfig MakeConfig(IEnumerable<Factors> factors) =>
        new(factors.Select(f => new FactorSettings(f, IsUsed: true, IsDrawn: true)).ToList());

    private static OrbConfig MakeOrbParallel(double orb) =>
        new(OrbSystems.Procentual, AspectBaseOrb: 10.0, Midpoint360DialOrb: 1.5,
            Midpoint90DialOrb: 1.0, Midpoint45DialOrb: 0.5, HarmonicOrb: 2.0,
            ParallelOrb: orb, DeclinationMidpointOrb: 0.75);

    private static OrbConfig MakeOrbMidpoint(double orb) =>
        new(OrbSystems.Procentual, AspectBaseOrb: 10.0, Midpoint360DialOrb: 1.5,
            Midpoint90DialOrb: 1.0, Midpoint45DialOrb: 0.5, HarmonicOrb: 2.0,
            ParallelOrb: 1.0, DeclinationMidpointOrb: orb);

    // MARK: - Parallels: delegation and basic results

    [Test]
    public void TestParallelsDelegatesCorrectly()
    {
        var chart     = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 20.5), (Factors.Mars, -20.0)]);
        var config    = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mars]);
        var orbConfig = MakeOrbParallel(1.0);

        var direct = ParallelsCalculator.Calculate(chart, config, orbConfig);
        var via    = DeclinationsOrchestrator.Parallels(chart, config, orbConfig);

        Assert.That(via.Count, Is.EqualTo(direct.Count));
        var viaS    = via.OrderBy(r => r.ActualOrb).ToList();
        var directS = direct.OrderBy(r => r.ActualOrb).ToList();
        for (var i = 0; i < viaS.Count; i++)
        {
            Assert.That(viaS[i].Factor1,          Is.EqualTo(directS[i].Factor1));
            Assert.That(viaS[i].Factor2,          Is.EqualTo(directS[i].Factor2));
            Assert.That(viaS[i].IsContraParallel, Is.EqualTo(directS[i].IsContraParallel));
            Assert.That(Math.Abs(viaS[i].ActualOrb - directS[i].ActualOrb), Is.LessThan(Delta));
            Assert.That(Math.Abs(viaS[i].MaxOrb   - directS[i].MaxOrb),    Is.LessThan(Delta));
        }
    }

    [Test]
    public void TestParallelsFindsParallel()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 20.5)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = DeclinationsOrchestrator.Parallels(chart, config, MakeOrbParallel(1.0));

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].IsContraParallel, Is.False);
        Assert.That(Math.Abs(result[0].ActualOrb - 0.5), Is.LessThan(Delta));
    }

    [Test]
    public void TestParallelsFindsContraParallel()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Mars, -20.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Mars]);
        var result = DeclinationsOrchestrator.Parallels(chart, config, MakeOrbParallel(1.0));

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].IsContraParallel, Is.True);
        Assert.That(Math.Abs(result[0].ActualOrb - 0.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestParallelsExcludesOutsideOrb()
    {
        // Saturn +10.0° is more than 1° from Sun +20.0° → no parallel
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Saturn, 10.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Saturn]);
        var result = DeclinationsOrchestrator.Parallels(chart, config, MakeOrbParallel(1.0));

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestParallelsEmptyChart()
    {
        var chart  = MakeChart([]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = DeclinationsOrchestrator.Parallels(chart, config, MakeOrbParallel(1.0));

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestParallelsSingleFactor()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0)]);
        var config = MakeConfig([Factors.Sun]);
        var result = DeclinationsOrchestrator.Parallels(chart, config, MakeOrbParallel(1.0));

        Assert.That(result, Is.Empty);
    }

    // MARK: - OccupiedMidpoints: delegation and basic results

    [Test]
    public void TestOccupiedMidpointsDelegatesCorrectly()
    {
        var chart     = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, -10.0), (Factors.Mars, 5.0)]);
        var config    = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mars]);
        var orbConfig = MakeOrbMidpoint(1.0);

        var baseMidpoints = DeclMidpointsCalculator.BaseMidpoints(chart, config);
        var positions     = DeclMidpointsCalculator.ActivePairs(chart, config);
        var direct = DeclinationMidpointsMatchFinder.Find(baseMidpoints, positions, orbConfig.DeclinationMidpointOrb);
        var via    = DeclinationsOrchestrator.OccupiedMidpoints(chart, config, orbConfig);

        Assert.That(via.Count, Is.EqualTo(direct.Count));
        for (var i = 0; i < via.Count; i++)
        {
            Assert.That(via[i].Midpoint.Factor1,    Is.EqualTo(direct[i].Midpoint.Factor1));
            Assert.That(via[i].Midpoint.Factor2,    Is.EqualTo(direct[i].Midpoint.Factor2));
            Assert.That(via[i].OccupyingFactor,     Is.EqualTo(direct[i].OccupyingFactor));
            Assert.That(Math.Abs(via[i].ActualOrb - direct[i].ActualOrb), Is.LessThan(Delta));
            Assert.That(Math.Abs(via[i].Exactness  - direct[i].Exactness),  Is.LessThan(Delta));
        }
    }

    [Test]
    public void TestOccupiedMidpointsFindsExactOccupation()
    {
        // Sun +20°, Moon −10° → midpoint +5.0°; Mars at +5.0° → exact
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, -10.0), (Factors.Mars, 5.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mars]);
        var result = DeclinationsOrchestrator.OccupiedMidpoints(chart, config, MakeOrbMidpoint(1.0));

        var match = result.FirstOrDefault(r =>
            r.Midpoint.Factor1 == Factors.Sun && r.Midpoint.Factor2 == Factors.Moon
            && r.OccupyingFactor == Factors.Mars);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 0.0), Is.LessThan(Delta));
        Assert.That(Math.Abs(match.Exactness  - 100.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestOccupiedMidpointsExcludesOutsideOrb()
    {
        // Sun/Moon midpoint = +5.0°; Saturn at +6.5° → deviation 1.5° > 1.0° → excluded
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, -10.0), (Factors.Saturn, 6.5)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Saturn]);
        var result = DeclinationsOrchestrator.OccupiedMidpoints(chart, config, MakeOrbMidpoint(1.0));

        var match = result.FirstOrDefault(r =>
            r.Midpoint.Factor1 == Factors.Sun && r.Midpoint.Factor2 == Factors.Moon
            && r.OccupyingFactor == Factors.Saturn);
        Assert.That(match, Is.Null);
    }

    [Test]
    public void TestOccupiedMidpointsResultsSortedByActualOrb()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, -10.0),
                                (Factors.Saturn, 5.3), (Factors.Jupiter, 5.7)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Saturn, Factors.Jupiter]);
        var result = DeclinationsOrchestrator.OccupiedMidpoints(chart, config, MakeOrbMidpoint(1.0));

        for (var i = 0; i < result.Count - 1; i++)
            Assert.That(result[i].ActualOrb, Is.LessThanOrEqualTo(result[i + 1].ActualOrb));
    }

    [Test]
    public void TestOccupiedMidpointsEmptyChart()
    {
        var chart  = MakeChart([]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = DeclinationsOrchestrator.OccupiedMidpoints(chart, config, MakeOrbMidpoint(1.0));

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestOccupiedMidpointsSingleFactor()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0)]);
        var config = MakeConfig([Factors.Sun]);
        var result = DeclinationsOrchestrator.OccupiedMidpoints(chart, config, MakeOrbMidpoint(1.0));

        Assert.That(result, Is.Empty);
    }

    // MARK: - LongitudeEquivalents: delegation and basic results

    [Test]
    public void TestEquivalentsDelegatesCorrectly()
    {
        var chart  = MakeChartLongDecl(
            [(Factors.Sun, 5.0, 0.0), (Factors.Moon, 91.0, Obliquity)], Obliquity);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);

        var direct = LongEquivalentsCalculator.Calculate(chart, config);
        var via    = DeclinationsOrchestrator.LongitudeEquivalents(chart, config);

        Assert.That(via.Count, Is.EqualTo(direct.Count));
        var viaS    = via.OrderBy(r => (int)r.Factor).ToList();
        var directS = direct.OrderBy(r => (int)r.Factor).ToList();
        for (var i = 0; i < viaS.Count; i++)
        {
            Assert.That(viaS[i].Factor, Is.EqualTo(directS[i].Factor));
            Assert.That(Math.Abs(viaS[i].LongitudeEquivalent - directS[i].LongitudeEquivalent),
                Is.LessThan(DeltaCoord));
            Assert.That(viaS[i].IsOutOfBounds, Is.EqualTo(directS[i].IsOutOfBounds));
        }
    }

    [Test]
    public void TestEquivalentsZeroDecl()
    {
        // Sun: decl=0°, long=5° → candidate1=0°, is closest
        var chart  = MakeChartLongDecl([(Factors.Sun, 5.0, 0.0)], Obliquity);
        var config = MakeConfig([Factors.Sun]);
        var result = DeclinationsOrchestrator.LongitudeEquivalents(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].LongitudeEquivalent - 0.0), Is.LessThan(DeltaCoord));
        Assert.That(result[0].IsOutOfBounds, Is.False);
    }

    [Test]
    public void TestEquivalentsDeclEqualsObliquity()
    {
        // Moon: decl=obliquity, long=91° → equivalent ≈ 90°
        var chart  = MakeChartLongDecl([(Factors.Moon, 91.0, Obliquity)], Obliquity);
        var config = MakeConfig([Factors.Moon]);
        var result = DeclinationsOrchestrator.LongitudeEquivalents(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].LongitudeEquivalent - 90.0), Is.LessThan(DeltaCoord));
        Assert.That(result[0].IsOutOfBounds, Is.False);
    }

    [Test]
    public void TestEquivalentsOobFlag()
    {
        // Mars: decl=30° > obliquity(23.4367°) → OOB
        var chart  = MakeChartLongDecl([(Factors.Mars, 60.0, 30.0)], Obliquity);
        var config = MakeConfig([Factors.Mars]);
        var result = DeclinationsOrchestrator.LongitudeEquivalents(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].IsOutOfBounds, Is.True);
    }

    [Test]
    public void TestEquivalentsInactiveExcluded()
    {
        var chart  = MakeChartLongDecl(
            [(Factors.Sun, 5.0, 0.0), (Factors.Moon, 91.0, Obliquity)], Obliquity);
        var config = MakeConfig([Factors.Sun]);   // Moon excluded
        var result = DeclinationsOrchestrator.LongitudeEquivalents(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Factor, Is.EqualTo(Factors.Sun));
    }

    [Test]
    public void TestEquivalentsEmptyChart()
    {
        var chart  = MakeChartLongDecl([], Obliquity);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = DeclinationsOrchestrator.LongitudeEquivalents(chart, config);

        Assert.That(result, Is.Empty);
    }
}
