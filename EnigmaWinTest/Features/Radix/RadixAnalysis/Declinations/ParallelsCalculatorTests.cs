// ParallelsCalculatorTests.cs
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
/// Tests for ParallelsCalculator.
/// Test data:
///   Sun    declination  +20.0° (north)
///   Moon   declination  +20.5° (north)  → parallel with Sun, orb 0.5°
///   Mars   declination  -20.0° (south)  → contra-parallel with Sun, orb 0.0°
///   Saturn declination  +10.0° (north)  → no parallel with any (nearest: Moon, 10.5°)
/// </summary>
[TestFixture]
public class ParallelsCalculatorTests
{
    private const double Delta = 1e-10;

    // MARK: - Shared helpers

    /// <summary>Builds a minimal FullFactorPosition with the supplied equatorial declination.</summary>
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

    /// <summary>FactorConfig with exactly the supplied factors marked IsUsed = true.</summary>
    private static FactorConfig MakeConfig(IEnumerable<Factors> factors) =>
        new(factors.Select(f => new FactorSettings(f, IsUsed: true, IsDrawn: true)).ToList());

    private static OrbConfig MakeOrb(double orb) =>
        new(OrbSystems.Procentual, AspectBaseOrb: 10.0, Midpoint360DialOrb: 1.5,
            Midpoint90DialOrb: 1.0, Midpoint45DialOrb: 0.5, HarmonicOrb: 2.0, ParallelOrb: orb,
            DeclinationMidpointOrb: 0.75);

    // MARK: - Happy flow: parallel found

    [Test]
    public void TestParallelNorthNorthWithinOrb()
    {
        // Sun +20.0°, Moon +20.5° → orb = 0.5°; maxOrb = 1.0°
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 20.5)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Factor1, Is.EqualTo(Factors.Sun));
        Assert.That(result[0].Factor2, Is.EqualTo(Factors.Moon));
        Assert.That(result[0].IsContraParallel, Is.False);
        Assert.That(Math.Abs(result[0].ActualOrb - 0.5), Is.LessThan(Delta));
        Assert.That(Math.Abs(result[0].MaxOrb - 1.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestParallelSouthSouthWithinOrb()
    {
        // Sun −18.0°, Moon −18.3° → orb = 0.3°; both south → parallel, not contra
        var chart  = MakeChart([(Factors.Sun, -18.0), (Factors.Moon, -18.3)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].IsContraParallel, Is.False);
        Assert.That(Math.Abs(result[0].ActualOrb - 0.3), Is.LessThan(Delta));
    }

    // MARK: - Happy flow: contra-parallel found

    [Test]
    public void TestContraParallelNorthSouth()
    {
        // Sun +20.0°, Mars −20.0° → orb = 0.0°; opposite sides → contra-parallel
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Mars, -20.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Mars]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].IsContraParallel, Is.True);
        Assert.That(Math.Abs(result[0].ActualOrb - 0.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestContraParallelWithinOrb()
    {
        // Sun +20.0°, Mars −20.8° → |20.0 − 20.8| = 0.8°; maxOrb = 1.0°
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Mars, -20.8)]);
        var config = MakeConfig([Factors.Sun, Factors.Mars]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].IsContraParallel, Is.True);
        Assert.That(Math.Abs(result[0].ActualOrb - 0.8), Is.LessThan(Delta));
    }

    // MARK: - Orb boundary

    [Test]
    public void TestExactOrbBoundaryIsIncluded()
    {
        // orb = |20.0 − 21.0| = 1.0°; maxOrb = 1.0° → included (≤)
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 21.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].ActualOrb - 1.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestJustOutsideOrbExcluded()
    {
        // orb = 1.01°; maxOrb = 1.0° → excluded
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 21.01)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        Assert.That(result, Is.Empty);
    }

    // MARK: - Multiple results

    [Test]
    public void TestMultipleParallels()
    {
        // Sun +20.0°, Moon +20.5° (parallel, orb 0.5°)
        // Mars −20.0° (contra-parallel with Sun, orb 0.0°; contra-parallel with Moon, orb 0.5°)
        // Saturn +10.0° — too far from all others (nearest diff = 10°)
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 20.5),
                                (Factors.Mars, -20.0), (Factors.Saturn, 10.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mars, Factors.Saturn]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        // Expected: Sun/Moon parallel, Sun/Mars contra, Moon/Mars contra = 3 results
        Assert.That(result.Count, Is.EqualTo(3));
    }

    [Test]
    public void TestMixedTypes()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 20.5), (Factors.Mars, -20.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mars]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        var parallels      = result.Where(r => !r.IsContraParallel).ToList();
        var contraParallels = result.Where(r => r.IsContraParallel).ToList();
        Assert.That(parallels.Count, Is.EqualTo(1));
        Assert.That(contraParallels.Count, Is.EqualTo(2));
    }

    // MARK: - FactorConfig filtering

    [Test]
    public void TestIgnoresFactorsNotInConfig()
    {
        // Chart has Sun and Moon, but config only marks Sun as used → no pairs → empty
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 20.5)]);
        var config = MakeConfig([Factors.Sun]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestIgnoresFactorsAbsentFromChart()
    {
        // Config marks Sun and Jupiter as used, but chart only has Sun → no pair
        var chart  = MakeChart([(Factors.Sun, 20.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Jupiter]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        Assert.That(result, Is.Empty);
    }

    // MARK: - Edge cases

    [Test]
    public void TestEmptyChartReturnsEmpty()
    {
        var chart  = MakeChart([]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestSingleFactorReturnsEmpty()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0)]);
        var config = MakeConfig([Factors.Sun]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(1.0));

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestZeroOrbExcludesNonExactMatch()
    {
        // Sun +20.0°, Moon +20.0001° → orb = 0.0001°; with maxOrb=0.0 → excluded
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 20.0001)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(0.0));

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestZeroOrbFindsExactParallel()
    {
        // Sun +20.0°, Moon +20.0° → orb = 0.0°; maxOrb = 0.0° → included
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 20.0)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(0.0));

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].ActualOrb - 0.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestNoDuplicatePairs()
    {
        // Three factors: exactly n*(n-1)/2 = 3 pairs.
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 20.3), (Factors.Mercury, 19.8)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mercury]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(5.0));

        // All three pairs are within orb; each appears once.
        var uniquePairs = result
            .Select(r => new HashSet<Factors> { r.Factor1, r.Factor2 })
            .Select(s => string.Join(",", s.Select(f => (int)f).OrderBy(x => x)))
            .Distinct()
            .Count();
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(uniquePairs, Is.EqualTo(3));
    }

    [Test]
    public void TestMaxOrbMatchesConfig()
    {
        var chart  = MakeChart([(Factors.Sun, 20.0), (Factors.Moon, 20.4)]);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = ParallelsCalculator.Calculate(chart, config, MakeOrb(2.5));

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].MaxOrb - 2.5), Is.LessThan(Delta));
    }
}
