// HarmonicsMatchFinderTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.Harmonics;

/// <summary>
/// Tests for HarmonicsMatchFinder.
/// Radix positions: Sun=30°, Moon=90°, Mars=150°, Jupiter=200°
/// Harmonic 3 positions: Sun=90°, Moon=270°, Mars=90°, Jupiter=240°
/// (Sun×3=90, Moon×3=270, Mars×3=450→90, Jupiter×3=600→240)
/// </summary>
[TestFixture]
public class HarmonicsMatchFinderTests
{
    private const double Delta = 1e-8;
    private const double Orb   = 2.0;

    private static readonly List<(Factors Factor, double Longitude)> RadixPositions =
    [
        (Factors.Sun,     30.0),
        (Factors.Moon,    90.0),
        (Factors.Mars,   150.0),
        (Factors.Jupiter, 200.0)
    ];

    // Harmonic 3: Sun=90, Moon=270, Mars=90, Jupiter=240
    private static readonly List<(Factors Factor, double Longitude)> HarmonicPositions =
        HarmonicsCalculator.Calculate(RadixPositions, 3.0);

    // MARK: - Happy flow

    /// <summary>
    /// Sun harmonic (90°) coincides exactly with radix Moon (90°).
    /// Self-match Sun→Sun is excluded; Sun→Moon must be found with orb 0°.
    /// </summary>
    [Test]
    public void TestExactMatch()
    {
        var results = HarmonicsMatchFinder.Find(HarmonicPositions, RadixPositions, Orb);
        var match = results.Find(m => m.HarmonicFactor == Factors.Sun
                                   && m.RadixFactor    == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 0.0), Is.LessThan(Delta));
    }

    /// <summary>
    /// Mars harmonic (90°) coincides exactly with radix Moon (90°) → orb 0°.
    /// </summary>
    [Test]
    public void TestExactMatchMarsHarmonicWithMoon()
    {
        var results = HarmonicsMatchFinder.Find(HarmonicPositions, RadixPositions, Orb);
        var match = results.Find(m => m.HarmonicFactor == Factors.Mars
                                   && m.RadixFactor    == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 0.0), Is.LessThan(Delta));
    }

    /// <summary>Jupiter harmonic (240°) is 1° away from a radix factor at 241° → within orb.</summary>
    [Test]
    public void TestMatchWithinOrb()
    {
        var radix = new List<(Factors, double)>(RadixPositions) { (Factors.Saturn, 241.0) };
        var harmonic = HarmonicsCalculator.Calculate(radix, 3.0);
        var results = HarmonicsMatchFinder.Find(harmonic, radix, Orb);
        var match = results.Find(m => m.HarmonicFactor == Factors.Jupiter
                                   && m.RadixFactor    == Factors.Saturn);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 1.0), Is.LessThan(Delta));
    }

    /// <summary>Jupiter harmonic (240°) is 3° away from Saturn at 243° → outside orb of 2°.</summary>
    [Test]
    public void TestNoMatchOutsideOrb()
    {
        var radix = new List<(Factors, double)>(RadixPositions) { (Factors.Saturn, 243.0) };
        var harmonic = HarmonicsCalculator.Calculate(radix, 3.0);
        var results = HarmonicsMatchFinder.Find(harmonic, radix, Orb);
        var match = results.Find(m => m.HarmonicFactor == Factors.Jupiter
                                   && m.RadixFactor    == Factors.Saturn);
        Assert.That(match, Is.Null);
    }

    /// <summary>Match 1° before the harmonic position → same orb as 1° after.</summary>
    [Test]
    public void TestMatchJustBeforeHarmonicPosition()
    {
        var radix = new List<(Factors, double)>(RadixPositions) { (Factors.Saturn, 239.0) };
        var harmonic = HarmonicsCalculator.Calculate(radix, 3.0);
        var results = HarmonicsMatchFinder.Find(harmonic, radix, Orb);
        var match = results.Find(m => m.HarmonicFactor == Factors.Jupiter
                                   && m.RadixFactor    == Factors.Saturn);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 1.0), Is.LessThan(Delta));
    }

    // MARK: - Self-match exclusion

    /// <summary>A factor must not be matched against its own harmonic position.</summary>
    [Test]
    public void TestSelfMatchIsExcluded()
    {
        var results = HarmonicsMatchFinder.Find(HarmonicPositions, RadixPositions, Orb);
        Assert.That(results.Find(m => m.HarmonicFactor == Factors.Sun
                                   && m.RadixFactor    == Factors.Sun), Is.Null);
        Assert.That(results.Find(m => m.HarmonicFactor == Factors.Moon
                                   && m.RadixFactor    == Factors.Moon), Is.Null);
        Assert.That(results.Find(m => m.HarmonicFactor == Factors.Mars
                                   && m.RadixFactor    == Factors.Mars), Is.Null);
        Assert.That(results.Find(m => m.HarmonicFactor == Factors.Jupiter
                                   && m.RadixFactor    == Factors.Jupiter), Is.Null);
    }

    // MARK: - Wrap-around at 0°/360°

    /// <summary>
    /// Harmonic at 1° and radix factor at 359° → shortest distance = 2° → within orb.
    /// </summary>
    [Test]
    public void TestWrapAroundNearZero()
    {
        var radix    = new List<(Factors, double)> { (Factors.Sun, 120.0), (Factors.Moon, 359.0) };
        var harmonic = new List<(Factors, double)> { (Factors.Sun, 1.0),   (Factors.Moon, 200.0) };
        var results = HarmonicsMatchFinder.Find(harmonic, radix, Orb);
        var match = results.Find(m => m.HarmonicFactor == Factors.Sun
                                   && m.RadixFactor    == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 2.0), Is.LessThan(Delta));
    }

    /// <summary>
    /// Harmonic at 359° and radix factor at 1° → shortest distance = 2° → within orb.
    /// </summary>
    [Test]
    public void TestWrapAroundNear360()
    {
        var radix    = new List<(Factors, double)> { (Factors.Sun, 200.0), (Factors.Moon, 1.0) };
        var harmonic = new List<(Factors, double)> { (Factors.Sun, 359.0), (Factors.Moon, 100.0) };
        var results = HarmonicsMatchFinder.Find(harmonic, radix, Orb);
        var match = results.Find(m => m.HarmonicFactor == Factors.Sun
                                   && m.RadixFactor    == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 2.0), Is.LessThan(Delta));
    }

    /// <summary>Harmonic at 358° and radix at 1° → distance = 3° → outside orb of 2°.</summary>
    [Test]
    public void TestWrapAroundOutsideOrb()
    {
        var radix    = new List<(Factors, double)> { (Factors.Sun, 200.0), (Factors.Moon, 1.0) };
        var harmonic = new List<(Factors, double)> { (Factors.Sun, 358.0), (Factors.Moon, 100.0) };
        var results = HarmonicsMatchFinder.Find(harmonic, radix, Orb);
        var match = results.Find(m => m.HarmonicFactor == Factors.Sun
                                   && m.RadixFactor    == Factors.Moon);
        Assert.That(match, Is.Null);
    }

    // MARK: - Result properties

    /// <summary>Results must be sorted by ActualOrb ascending (most exact first).</summary>
    [Test]
    public void TestResultSortedByOrb()
    {
        var radix    = new List<(Factors, double)>(RadixPositions) { (Factors.Saturn, 91.0), (Factors.Uranus, 91.5) };
        var harmonic = HarmonicsCalculator.Calculate(radix, 3.0);
        var results  = HarmonicsMatchFinder.Find(harmonic, radix, Orb);
        for (var i = 0; i < results.Count - 1; i++)
            Assert.That(results[i].ActualOrb, Is.LessThanOrEqualTo(results[i + 1].ActualOrb));
    }

    /// <summary>MaxOrb stored in every match equals the configured orb value.</summary>
    [Test]
    public void TestMaxOrbEqualsConfigured()
    {
        var results = HarmonicsMatchFinder.Find(HarmonicPositions, RadixPositions, Orb);
        foreach (var match in results)
            Assert.That(Math.Abs(match.MaxOrb - Orb), Is.LessThan(Delta));
    }

    // MARK: - Edge cases

    [Test]
    public void TestEmptyHarmonicPositionsReturnsEmpty()
    {
        var results = HarmonicsMatchFinder.Find([], RadixPositions, Orb);
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void TestEmptyRadixPositionsReturnsEmpty()
    {
        var results = HarmonicsMatchFinder.Find(HarmonicPositions, [], Orb);
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void TestZeroOrbOnlyFindsExactMatches()
    {
        // Sun harmonic at 90° exactly matches radix Moon at 90°; any non-exact pair is excluded.
        var results = HarmonicsMatchFinder.Find(HarmonicPositions, RadixPositions, 0.0);
        foreach (var match in results)
            Assert.That(Math.Abs(match.ActualOrb - 0.0), Is.LessThan(Delta));
    }
}
