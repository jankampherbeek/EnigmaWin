// DeclMidpointsCalculatorTests.cs
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
/// Tests for DeclMidpointsCalculator.
/// Test data:
///   Sun   declination  +20.0° (north)
///   Moon  declination  −10.0° (south)
///   Mars  declination   +5.0° (north)
///
/// Expected base midpoints:
///   Sun/Moon  = (20.0 + −10.0) / 2 =  +5.0°
///   Sun/Mars  = (20.0 +   5.0) / 2 = +12.5°
///   Moon/Mars = (−10.0 +  5.0) / 2 =  −2.5°
/// </summary>
[TestFixture]
public class DeclMidpointsCalculatorTests
{
    private const double Delta = 1e-10;

    // MARK: - Shared helpers

    private static FullFactorPosition MakePos(double declination) =>
        new(
            Ecliptical: [new MainAstronomicalPosition(0, 0, 1)],
            Equatorial: [new MainAstronomicalPosition(0, declination, 1)],
            Horizontal: [new HorizontalPosition(0, 0)]);

    private static FullChart MakeChart(IEnumerable<(Factors Factor, double Declination)> declinations)
    {
        var coords = new Dictionary<Factors, FullFactorPosition>();
        foreach (var (factor, decl) in declinations)
            coords[factor] = MakePos(decl);
        var dummy = new FullCuspPosition(0, 0, 0, new HorizontalPosition(0, 0));
        var houses = new HousePositions([], dummy, dummy, dummy, dummy);
        return new FullChart(coords, houses, 0, 0, 23.45);
    }

    private static FactorConfig MakeConfig(IEnumerable<Factors> factors) =>
        new(factors.Select(f => new FactorSettings(f, IsUsed: true, IsDrawn: true)).ToList());

    // MARK: - BaseMidpoints: positions

    [Test]
    public void TestMidpointTwoNorth()
    {
        // Sun +20°, Mars +5° → midpoint = +12.5°
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Mars, 5.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Mars]);
        var result = DeclMidpointsCalculator.BaseMidpoints(chart, config);

        var mid = result.FirstOrDefault(m => m.Factor1 == Factors.Sun && m.Factor2 == Factors.Mars);
        Assert.That(mid, Is.Not.Null);
        Assert.That(Math.Abs(mid!.Position - 12.5), Is.LessThan(Delta));
    }

    [Test]
    public void TestMidpointNorthAndSouth()
    {
        // Sun +20°, Moon −10° → midpoint = +5.0°
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, -10.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = DeclMidpointsCalculator.BaseMidpoints(chart, config);

        var mid = result.FirstOrDefault(m => m.Factor1 == Factors.Sun && m.Factor2 == Factors.Moon);
        Assert.That(mid, Is.Not.Null);
        Assert.That(Math.Abs(mid!.Position - 5.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestMidpointTwoSouth()
    {
        // Moon −10°, Mars −4° → midpoint = −7.0°
        var chart  = MakeChart([(Factors.Moon, -10.0), (Factors.Mars, -4.0)]);
        var config = MakeConfig([Factors.Moon, Factors.Mars]);
        var result = DeclMidpointsCalculator.BaseMidpoints(chart, config);

        var mid = result.FirstOrDefault(m => m.Factor1 == Factors.Moon && m.Factor2 == Factors.Mars);
        Assert.That(mid, Is.Not.Null);
        Assert.That(Math.Abs(mid!.Position - (-7.0)), Is.LessThan(Delta));
    }

    // MARK: - BaseMidpoints: count

    [Test]
    public void TestBaseMidpointCount()
    {
        // 4 factors → 4*3/2 = 6 base midpoints
        var chart  = MakeChart([(Factors.Sun, 0.0), (Factors.Moon, 4.0),
                                (Factors.Mars, 8.0), (Factors.Jupiter, 12.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mars, Factors.Jupiter]);
        var result = DeclMidpointsCalculator.BaseMidpoints(chart, config);

        Assert.That(result.Count, Is.EqualTo(6));
    }

    [Test]
    public void TestThreeFactorsProduceThreeMidpoints()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, -10.0), (Factors.Mars, 5.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mars]);
        var result = DeclMidpointsCalculator.BaseMidpoints(chart, config);

        Assert.That(result.Count, Is.EqualTo(3));
    }

    // MARK: - BaseMidpoints: edge cases

    [Test]
    public void TestEmptyChartReturnsEmpty()
    {
        var chart  = MakeChart([]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = DeclMidpointsCalculator.BaseMidpoints(chart, config);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestSingleFactorReturnsEmpty()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0)]);
        var config = MakeConfig([Factors.Sun]);
        var result = DeclMidpointsCalculator.BaseMidpoints(chart, config);

        Assert.That(result, Is.Empty);
    }

    // MARK: - BaseMidpoints: FactorConfig filtering

    [Test]
    public void TestIgnoresInactiveFactors()
    {
        // Chart has Sun, Moon, Mars. Config marks only Sun and Moon as used.
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, -10.0), (Factors.Mars, 5.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);   // Mars excluded
        var result = DeclMidpointsCalculator.BaseMidpoints(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        var marsMid = result.FirstOrDefault(m => m.Factor1 == Factors.Mars || m.Factor2 == Factors.Mars);
        Assert.That(marsMid, Is.Null);
    }

    [Test]
    public void TestIgnoresFactorsAbsentFromChart()
    {
        // Config asks for Sun, Moon, Jupiter but chart has no Jupiter.
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, -10.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Jupiter]);
        var result = DeclMidpointsCalculator.BaseMidpoints(chart, config);

        // Only Sun/Moon midpoint; Jupiter absent from chart → skipped
        Assert.That(result.Count, Is.EqualTo(1));
    }

    // MARK: - ActivePairs

    [Test]
    public void TestActivePairsReturnsCorrectDeclinations()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, -10.0), (Factors.Mars, 5.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mars]);
        var pairs  = DeclMidpointsCalculator.ActivePairs(chart, config);

        Assert.That(pairs.Count, Is.EqualTo(3));
        var sunDecl  = pairs.FirstOrDefault(p => p.Factor == Factors.Sun).Declination;
        var moonDecl = pairs.FirstOrDefault(p => p.Factor == Factors.Moon).Declination;
        Assert.That(Math.Abs(sunDecl  - 20.0),  Is.LessThan(Delta));
        Assert.That(Math.Abs(moonDecl - (-10.0)), Is.LessThan(Delta));
    }

    [Test]
    public void TestActivePairsExcludesInactive()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, -10.0), (Factors.Mars, 5.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);   // Mars inactive
        var pairs  = DeclMidpointsCalculator.ActivePairs(chart, config);

        Assert.That(pairs.Count, Is.EqualTo(2));
        Assert.That(pairs.Any(p => p.Factor == Factors.Mars), Is.False);
    }

    [Test]
    public void TestActivePairsEmptyChart()
    {
        var chart  = MakeChart([]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var pairs  = DeclMidpointsCalculator.ActivePairs(chart, config);

        Assert.That(pairs, Is.Empty);
    }
}
