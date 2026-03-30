// MidpointsCalculatorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.Midpoints;

/// <summary>Tests for MidpointsCalculator.</summary>
[TestFixture]
public class MidpointsCalculatorTests
{
    private const double Delta = 1e-8;

    // MARK: - Happy flow

    [Test]
    public void TestCalculatePositionsForFourFactors()
    {
        // Sun=22°, Moon=178°, Mars=302°, Jupiter=42°
        // Expected midpoints (sorted by position):
        // Sun/Jupiter:  22 + (42-22)/2   = 32°
        // Sun/Moon:     22 + (178-22)/2  = 100°
        // Moon/Jupiter: 42 + (178-42)/2  = 110°
        // Moon/Mars:   178 + (302-178)/2 = 240°
        // Sun/Mars:    shortest arc = 360-(302-22)=80°, start 302, mid = 302+40 = 342°
        // Mars/Jupiter: shortest arc = 360-(302-42)=100°, start 302, mid = 302+50 = 352°
        var positions = MakePositions();
        var result = MidpointsCalculator.Calculate(positions);
        Assert.That(result.Count, Is.EqualTo(6));
        Assert.That(Math.Abs(result[0].Position - 32.0), Is.LessThan(Delta));
        Assert.That(Math.Abs(result[1].Position - 100.0), Is.LessThan(Delta));
        Assert.That(Math.Abs(result[2].Position - 110.0), Is.LessThan(Delta));
        Assert.That(Math.Abs(result[3].Position - 240.0), Is.LessThan(Delta));
        Assert.That(Math.Abs(result[4].Position - 342.0), Is.LessThan(Delta));
        Assert.That(Math.Abs(result[5].Position - 352.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestResultIsSortedAscending()
    {
        var positions = MakePositions();
        var result = MidpointsCalculator.Calculate(positions);
        for (var i = 0; i < result.Count - 1; i++)
        {
            Assert.That(result[i].Position, Is.LessThanOrEqualTo(result[i + 1].Position));
        }
    }

    [Test]
    public void TestFactorPairs()
    {
        // Sorted order: 32 (Sun/Jupiter), 100 (Sun/Moon), 110 (Moon/Jupiter),
        //               240 (Moon/Mars), 342 (Sun/Mars), 352 (Mars/Jupiter)
        var positions = MakePositions();
        var result = MidpointsCalculator.Calculate(positions);
        Assert.That(result[0].Factor1, Is.EqualTo(Factors.Sun));
        Assert.That(result[0].Factor2, Is.EqualTo(Factors.Jupiter));
        Assert.That(result[1].Factor1, Is.EqualTo(Factors.Sun));
        Assert.That(result[1].Factor2, Is.EqualTo(Factors.Moon));
        Assert.That(result[2].Factor1, Is.EqualTo(Factors.Moon));
        Assert.That(result[2].Factor2, Is.EqualTo(Factors.Jupiter));
        Assert.That(result[3].Factor1, Is.EqualTo(Factors.Moon));
        Assert.That(result[3].Factor2, Is.EqualTo(Factors.Mars));
        Assert.That(result[4].Factor1, Is.EqualTo(Factors.Sun));
        Assert.That(result[4].Factor2, Is.EqualTo(Factors.Mars));
        Assert.That(result[5].Factor1, Is.EqualTo(Factors.Mars));
        Assert.That(result[5].Factor2, Is.EqualTo(Factors.Jupiter));
    }

    // MARK: - Shortest arc edge cases

    [Test]
    public void TestMidpointWrapsCorrectlyNearZeroBoundary()
    {
        // 350° and 10° → shortest arc = 20°, midpoint = 0°
        var positions = new List<(Factors, double)>
        {
            (Factors.Sun, 350.0),
            (Factors.Moon, 10.0)
        };
        var result = MidpointsCalculator.Calculate(positions);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].Position - 0.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestMidpointAtHalfCircle()
    {
        // 90° and 270° → two equal arcs, both 180°; result must be in [0, 360)
        var positions = new List<(Factors, double)>
        {
            (Factors.Sun, 90.0),
            (Factors.Moon, 270.0)
        };
        var result = MidpointsCalculator.Calculate(positions);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Position, Is.GreaterThanOrEqualTo(0.0).And.LessThan(360.0));
    }

    [Test]
    public void TestMidpointForIdenticalPositions()
    {
        var positions = new List<(Factors, double)>
        {
            (Factors.Sun, 120.0),
            (Factors.Moon, 120.0)
        };
        var result = MidpointsCalculator.Calculate(positions);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].Position - 120.0), Is.LessThan(Delta));
    }

    // MARK: - Count

    [Test]
    public void TestEmptyInputReturnsEmptyList()
    {
        var result = MidpointsCalculator.Calculate([]);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestSingleFactorReturnsEmptyList()
    {
        var result = MidpointsCalculator.Calculate([(Factors.Sun, 45.0)]);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestTwoFactorsProduceExactlyOneMidpoint()
    {
        var positions = new List<(Factors, double)>
        {
            (Factors.Sun, 22.0),
            (Factors.Moon, 178.0)
        };
        var result = MidpointsCalculator.Calculate(positions);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(Math.Abs(result[0].Position - 100.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestCountFormula()
    {
        // 4 factors → 4*3/2 = 6 midpoints
        var positions = MakePositions();
        var result = MidpointsCalculator.Calculate(positions);
        Assert.That(result.Count, Is.EqualTo(6));
    }

    // MARK: - Helpers

    /// <summary>Sun=22°, Moon=178°, Mars=302°, Jupiter=42° — same data as Apple MidpointsCalculatorTests.</summary>
    private static List<(Factors, double)> MakePositions() =>
    [
        (Factors.Sun, 22.0),
        (Factors.Moon, 178.0),
        (Factors.Mars, 302.0),
        (Factors.Jupiter, 42.0)
    ];
}
