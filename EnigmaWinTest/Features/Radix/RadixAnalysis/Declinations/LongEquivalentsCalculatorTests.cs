// LongEquivalentsCalculatorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.Declinations;

/// <summary>
/// Tests for LongEquivalentsCalculator.
///
/// Reference obliquity used throughout: 23.4367° (close to J2000 true obliquity).
///
/// Analytically derived test expectations
/// ──────────────────────────────────────
/// Sun: decl=0°, long=5° (first quadrant)
///   candidate1 = asin(0/sin(23.4367°)) = 0°   diff=5°
///   candidate2 = 90+(90−0) = 180°              diff=175°  → equivalent=0°
///
/// Moon: decl=23.4367° (=obliquity), long=91°
///   candidate1 = asin(1) = 90°   diff=1°
///   candidate2 = 90+(90−90) = 90°              → equivalent≈90°
///
/// Mars: decl=obliquity/2, long=45°
///   sin(decl)/sin(obl) = 0.5 → candidate1≈30.71°   diff≈14.29°
///   candidate2=90+(90−30.71)≈149.29°                diff≈104.29°  → c1 wins
///
/// Saturn: decl=−23.4367°, long=270°
///   candidate1=asin(−1)=−90°→normalised 270°  diff=0°  → equivalent=270°
///
/// Jupiter: decl=30.0° > 23.4367° → OOB
///   reflected≈16.8734°  candidate1≈39.4°   long=60° → c1 wins
///
/// Mercury: decl=−30.0° → OOB south
///   reflected≈−16.8734°  candidate1≈320.6°  long=300° ≥ 180° → c1 wins
/// </summary>
[TestFixture]
public class LongEquivalentsCalculatorTests
{
    private const double Delta     = 1e-3;       // ≈ 0.001° tolerance for trig rounding
    private const double Obliquity = 23.4367;    // reference obliquity for all tests

    // MARK: - Shared helpers

    private static FullFactorPosition MakePos(double longitude, double declination) =>
        new(
            Ecliptical: [new MainAstronomicalPosition(longitude, 0, 1)],
            Equatorial: [new MainAstronomicalPosition(0, declination, 1)],
            Horizontal: [new HorizontalPosition(0, 0)]);

    private static FullChart MakeChart(
        IEnumerable<(Factors Factor, double Longitude, double Declination)> factors,
        double obliquity)
    {
        var coords = new Dictionary<Factors, FullFactorPosition>();
        foreach (var (factor, lon, decl) in factors)
            coords[factor] = MakePos(lon, decl);
        var dummy  = new FullCuspPosition(0, 0, 0, new HorizontalPosition(0, 0));
        var houses = new HousePositions([], dummy, dummy, dummy, dummy);
        return new FullChart(coords, houses, 0, 0, obliquity);
    }

    private static FactorConfig MakeConfig(IEnumerable<Factors> factors) =>
        new(new System.Collections.Generic.List<FactorSettings>(
            System.Linq.Enumerable.Select(factors,
                f => new FactorSettings(f, IsUsed: true, IsDrawn: true))));

    // MARK: - Zero declination

    [Test]
    public void TestZeroDeclination()
    {
        // Sun: decl=0°, long=5° → candidate1=0°, diff=5°; c2=180°, diff=175° → 0°
        var chart  = MakeChart([(Factors.Sun, 5.0, 0.0)], Obliquity);
        var config = MakeConfig([Factors.Sun]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].LongitudeEquivalent - 0.0), Is.LessThan(Delta));
        Assert.That(result[0].IsOutOfBounds, Is.False);
    }

    // MARK: - Declination equals obliquity

    [Test]
    public void TestDeclEqualObliquity()
    {
        // Moon: decl=obliquity, long=91° → candidate1≈90°, is closest
        var chart  = MakeChart([(Factors.Moon, 91.0, Obliquity)], Obliquity);
        var config = MakeConfig([Factors.Moon]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].LongitudeEquivalent - 90.0), Is.LessThan(Delta));
        Assert.That(result[0].IsOutOfBounds, Is.False);
    }

    // MARK: - Half obliquity

    [Test]
    public void TestHalfObliquity()
    {
        // Mars: decl=obliquity/2, long=45° (first quadrant)
        var decl     = Obliquity / 2.0;
        var sinRatio = Math.Sin(decl * Math.PI / 180.0) / Math.Sin(Obliquity * Math.PI / 180.0);
        var c1       = Math.Asin(sinRatio) * 180.0 / Math.PI;   // ≈ 30.71°
        var c2       = 90.0 + (90.0 - c1);                       // ≈ 149.29°
        var expected = Math.Abs(c1 - 45.0) < Math.Abs(c2 - 45.0) ? c1 : c2;  // c1 is closer

        var chart  = MakeChart([(Factors.Mars, 45.0, decl)], Obliquity);
        var config = MakeConfig([Factors.Mars]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].LongitudeEquivalent - expected), Is.LessThan(Delta));
        Assert.That(result[0].IsOutOfBounds, Is.False);
    }

    // MARK: - South declination (candidate in third/fourth quadrant)

    [Test]
    public void TestSouthDeclEqualObliquity()
    {
        // Saturn: decl=−obliquity, long=270° → asin(−1)=−90° → normalised 270°
        var chart  = MakeChart([(Factors.Saturn, 270.0, -Obliquity)], Obliquity);
        var config = MakeConfig([Factors.Saturn]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].LongitudeEquivalent - 270.0), Is.LessThan(Delta));
        Assert.That(result[0].IsOutOfBounds, Is.False);
    }

    // MARK: - Out of bounds (OOB)

    [Test]
    public void TestOobFlagSet()
    {
        // Jupiter: decl=30° > 23.4367° → OOB
        var chart  = MakeChart([(Factors.Jupiter, 60.0, 30.0)], Obliquity);
        var config = MakeConfig([Factors.Jupiter]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].IsOutOfBounds, Is.True);
    }

    [Test]
    public void TestOobNorthEquivalent()
    {
        // Jupiter: decl=30°, oobPart=6.5633°, reflected=16.8734°
        // candidate1 = asin(sin(16.8734°)/sin(23.4367°))
        var chart  = MakeChart([(Factors.Jupiter, 60.0, 30.0)], Obliquity);
        var config = MakeConfig([Factors.Jupiter]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        var reflected = Obliquity - (30.0 - Obliquity);
        var expected  = Math.Asin(Math.Sin(reflected * Math.PI / 180.0)
                            / Math.Sin(Obliquity * Math.PI / 180.0))
                        * 180.0 / Math.PI;   // ≈ 39.4°

        Assert.That(Math.Abs(result[0].LongitudeEquivalent - expected), Is.LessThan(Delta));
    }

    [Test]
    public void TestOobSouthFlag()
    {
        // Mercury: decl=−30° < −23.4367° → OOB
        var chart  = MakeChart([(Factors.Mercury, 300.0, -30.0)], Obliquity);
        var config = MakeConfig([Factors.Mercury]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].IsOutOfBounds, Is.True);
    }

    [Test]
    public void TestExactlyAtObliquityIsNotOob()
    {
        var chart  = MakeChart([(Factors.Sun, 90.0, Obliquity)], Obliquity);
        var config = MakeConfig([Factors.Sun]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result[0].IsOutOfBounds, Is.False);
    }

    // MARK: - Candidate selection

    [Test]
    public void TestSecondQuadrantCandidateSelection()
    {
        // decl=obliquity/2, long=140° < 180°
        // candidate2 is closer to 140° than candidate1
        var decl     = Obliquity / 2.0;
        var sinRatio = Math.Sin(decl * Math.PI / 180.0) / Math.Sin(Obliquity * Math.PI / 180.0);
        var c1       = Math.Asin(sinRatio) * 180.0 / Math.PI;   // ≈ 30.71°
        var c2       = 90.0 + (90.0 - c1);                       // ≈ 149.29°
        var expected = Math.Abs(c1 - 140.0) < Math.Abs(c2 - 140.0) ? c1 : c2;  // c2 is closer

        var chart  = MakeChart([(Factors.Sun, 140.0, decl)], Obliquity);
        var config = MakeConfig([Factors.Sun]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(Math.Abs(result[0].LongitudeEquivalent - expected), Is.LessThan(Delta));
    }

    [Test]
    public void TestThirdQuadrantCandidateSelection()
    {
        // Sun: decl=−obliquity/2, long=220° ≥ 180°
        // candidate1 normalised > 180°; candidate2=270+(270−c1); c2 is closer to 220°
        var decl     = Obliquity / 2.0;
        var sinRatio = Math.Sin(-decl * Math.PI / 180.0) / Math.Sin(Obliquity * Math.PI / 180.0);
        var c1       = Math.Asin(sinRatio) * 180.0 / Math.PI;
        if (c1 < 0.0) c1 += 360.0;                               // ≈ 329.29°
        var c2       = 270.0 + (270.0 - c1);                     // ≈ 210.71°
        var expected = Math.Abs(c1 - 220.0) < Math.Abs(c2 - 220.0) ? c1 : c2;  // c2 is closer

        var chart  = MakeChart([(Factors.Sun, 220.0, -decl)], Obliquity);
        var config = MakeConfig([Factors.Sun]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(Math.Abs(result[0].LongitudeEquivalent - expected), Is.LessThan(Delta));
    }

    // MARK: - FactorConfig filtering

    [Test]
    public void TestInactiveFactorExcluded()
    {
        // Chart has Sun and Moon; config marks only Sun as used
        var chart  = MakeChart([(Factors.Sun, 5.0, 0.0), (Factors.Moon, 91.0, Obliquity)], Obliquity);
        var config = MakeConfig([Factors.Sun]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Factor, Is.EqualTo(Factors.Sun));
    }

    [Test]
    public void TestAbsentFactorExcluded()
    {
        // Config asks for Sun and Jupiter; chart has only Sun
        var chart  = MakeChart([(Factors.Sun, 5.0, 0.0)], Obliquity);
        var config = MakeConfig([Factors.Sun, Factors.Jupiter]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Factor, Is.EqualTo(Factors.Sun));
    }

    // MARK: - Multiple factors

    [Test]
    public void TestResultCountMatchesActiveFactors()
    {
        var chart = MakeChart(
        [
            (Factors.Sun,    5.0,   0.0),
            (Factors.Moon,   91.0,  Obliquity),
            (Factors.Mars,   45.0,  Obliquity / 2.0),
            (Factors.Saturn, 270.0, -Obliquity)
        ], Obliquity);
        var config = MakeConfig([Factors.Sun, Factors.Moon, Factors.Mars, Factors.Saturn]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result.Count, Is.EqualTo(4));
    }

    // MARK: - Edge cases

    [Test]
    public void TestEmptyChart()
    {
        var chart  = MakeChart([], Obliquity);
        var config = MakeConfig([Factors.Sun, Factors.Moon]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestFactorPropertyIsCorrect()
    {
        var chart  = MakeChart([(Factors.Mars, 45.0, Obliquity / 2.0)], Obliquity);
        var config = MakeConfig([Factors.Mars]);
        var result = LongEquivalentsCalculator.Calculate(chart, config);

        Assert.That(result[0].Factor, Is.EqualTo(Factors.Mars));
    }
}
