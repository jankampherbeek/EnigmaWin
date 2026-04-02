// HarmonicsCalculatorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.Harmonics;

/// <summary>Tests for HarmonicsCalculator.</summary>
[TestFixture]
public class HarmonicsCalculatorTests
{
    private const double Delta = 1e-8;

    // MARK: - Happy flow (integer harmonic)

    [Test]
    public void TestHarmonic2MultipliesLongitude()
    {
        // Sun at 45° × 2 = 90°
        var positions = new List<(Factors, double)> { (Factors.Sun, 45.0) };
        var result = HarmonicsCalculator.Calculate(positions, 2.0);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].Longitude - 90.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestHarmonic3MultipliesLongitude()
    {
        // Moon at 60° × 3 = 180°
        var positions = new List<(Factors, double)> { (Factors.Moon, 60.0) };
        var result = HarmonicsCalculator.Calculate(positions, 3.0);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].Longitude - 180.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestHarmonic4MultipliesLongitude()
    {
        // Mars at 50° × 4 = 200°
        var positions = new List<(Factors, double)> { (Factors.Mars, 50.0) };
        var result = HarmonicsCalculator.Calculate(positions, 4.0);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].Longitude - 200.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestHarmonic1ReturnsUnchangedLongitude()
    {
        // Harmonic 1 is identity: 123.456° × 1 = 123.456°
        var positions = new List<(Factors, double)> { (Factors.Sun, 123.456) };
        var result = HarmonicsCalculator.Calculate(positions, 1.0);
        Assert.That(Math.Abs(result[0].Longitude - 123.456), Is.LessThan(Delta));
    }

    [Test]
    public void TestMultipleFactorsAllCalculated()
    {
        // Sun=30°, Moon=90°, Mars=120° with harmonic 3
        // 30×3=90°, 90×3=270°, 120×3=360→0°
        var positions = new List<(Factors, double)>
        {
            (Factors.Sun, 30.0),
            (Factors.Moon, 90.0),
            (Factors.Mars, 120.0)
        };
        var result = HarmonicsCalculator.Calculate(positions, 3.0);
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(Math.Abs(result[0].Longitude - 90.0), Is.LessThan(Delta));
        Assert.That(Math.Abs(result[1].Longitude - 270.0), Is.LessThan(Delta));
        Assert.That(Math.Abs(result[2].Longitude - 0.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestFactorsArePreservedInOutput()
    {
        var positions = new List<(Factors, double)>
        {
            (Factors.Sun, 30.0),
            (Factors.Moon, 90.0),
            (Factors.Mars, 120.0)
        };
        var result = HarmonicsCalculator.Calculate(positions, 3.0);
        Assert.That(result[0].Factor, Is.EqualTo(Factors.Sun));
        Assert.That(result[1].Factor, Is.EqualTo(Factors.Moon));
        Assert.That(result[2].Factor, Is.EqualTo(Factors.Mars));
    }

    [Test]
    public void TestOrderIsPreserved()
    {
        var positions = MakePositions();
        var result = HarmonicsCalculator.Calculate(positions, 2.0);
        Assert.That(result.Count, Is.EqualTo(positions.Count));
        for (var i = 0; i < positions.Count; i++)
        {
            Assert.That(result[i].Factor, Is.EqualTo(positions[i].Factor));
        }
    }

    // MARK: - Reduction to 0–360°

    [Test]
    public void TestResultReducedBelow360()
    {
        // Sun at 200° × 2 = 400° → reduced to 40°
        var positions = new List<(Factors, double)> { (Factors.Sun, 200.0) };
        var result = HarmonicsCalculator.Calculate(positions, 2.0);
        Assert.That(Math.Abs(result[0].Longitude - 40.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestResultReducedWhenMultipleWraps()
    {
        // Moon at 270° × 4 = 1080° → 1080 % 360 = 0°
        var positions = new List<(Factors, double)> { (Factors.Moon, 270.0) };
        var result = HarmonicsCalculator.Calculate(positions, 4.0);
        Assert.That(Math.Abs(result[0].Longitude - 0.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestResultIsAlwaysInRange()
    {
        var positions = MakePositions();
        foreach (var harmonic in new[] { 2.0, 3.0, 5.0, 7.0, 12.0 })
        {
            var result = HarmonicsCalculator.Calculate(positions, harmonic);
            foreach (var (_, lon) in result)
            {
                Assert.That(lon, Is.GreaterThanOrEqualTo(0.0).And.LessThan(360.0),
                    $"Longitude {lon} out of range for harmonic {harmonic}");
            }
        }
    }

    [Test]
    public void TestExactly360ReducesToZero()
    {
        // Sun at 180° × 2 = 360° → 0°
        var positions = new List<(Factors, double)> { (Factors.Sun, 180.0) };
        var result = HarmonicsCalculator.Calculate(positions, 2.0);
        Assert.That(Math.Abs(result[0].Longitude - 0.0), Is.LessThan(Delta));
    }

    // MARK: - Non-integer harmonic

    [Test]
    public void TestNonIntegerHarmonic()
    {
        // Sun at 100° × 2.5 = 250°
        var positions = new List<(Factors, double)> { (Factors.Sun, 100.0) };
        var result = HarmonicsCalculator.Calculate(positions, 2.5);
        Assert.That(Math.Abs(result[0].Longitude - 250.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestNonIntegerHarmonicWithReduction()
    {
        // Moon at 200° × 2.5 = 500° → 500 % 360 = 140°
        var positions = new List<(Factors, double)> { (Factors.Moon, 200.0) };
        var result = HarmonicsCalculator.Calculate(positions, 2.5);
        Assert.That(Math.Abs(result[0].Longitude - 140.0), Is.LessThan(Delta));
    }

    // MARK: - Edge cases

    [Test]
    public void TestEmptyInputReturnsEmptyList()
    {
        var result = HarmonicsCalculator.Calculate([], 4.0);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestZeroLongitudeRemainsZero()
    {
        var positions = new List<(Factors, double)> { (Factors.Sun, 0.0) };
        var result = HarmonicsCalculator.Calculate(positions, 5.0);
        Assert.That(Math.Abs(result[0].Longitude - 0.0), Is.LessThan(Delta));
    }

    // MARK: - Helpers

    /// <summary>Sun=22°, Moon=178°, Mars=302°, Jupiter=42°.</summary>
    private static List<(Factors Factor, double Longitude)> MakePositions() =>
    [
        (Factors.Sun, 22.0),
        (Factors.Moon, 178.0),
        (Factors.Mars, 302.0),
        (Factors.Jupiter, 42.0)
    ];
}
