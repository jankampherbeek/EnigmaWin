// MidpointMatchFinderTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.Midpoints;

/// <summary>
/// Tests for MidpointMatchFinder.
/// Base positions: Sun=22°, Moon=178°, Mars=302°, Jupiter=42°
/// Midpoints (360°):
///   Sun/Jupiter  =  32°
///   Sun/Moon     = 100°
///   Moon/Jupiter = 110°
///   Moon/Mars    = 240°
///   Sun/Mars     = 342°
///   Mars/Jupiter = 352°
/// </summary>
[TestFixture]
public class MidpointMatchFinderTests
{
    private const double Delta = 1e-8;
    private const double Orb   = 1.6;

    private static readonly List<(Factors, double)> BasePositions =
    [
        (Factors.Sun,     22.0),
        (Factors.Moon,   178.0),
        (Factors.Mars,   302.0),
        (Factors.Jupiter, 42.0)
    ];

    private static readonly List<BaseMidpoint> BaseMidpoints =
        MidpointsCalculator.Calculate(BasePositions);

    // MARK: - Dial360: happy flow

    /// <summary>Saturn exactly on midpoint Sun/Moon (100°) → orb 0°.</summary>
    [Test]
    public void TestDial360ExactConjunction()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 100.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial360, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 0.0), Is.LessThan(Delta));
    }

    /// <summary>Saturn 1° past midpoint Sun/Moon (101°) → orb 1°, within 1.6°.</summary>
    [Test]
    public void TestDial360ConjunctionWithinOrb()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 101.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial360, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 1.0), Is.LessThan(Delta));
    }

    /// <summary>Saturn 2° past midpoint Sun/Moon (102°) → outside orb of 1.6°.</summary>
    [Test]
    public void TestDial360ConjunctionOutsideOrb()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 102.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial360, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Null);
    }

    /// <summary>Saturn 1° before midpoint Sun/Moon (99°) → match, orb 1°.</summary>
    [Test]
    public void TestDial360MatchJustBeforeMidpoint()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 99.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial360, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 1.0), Is.LessThan(Delta));
    }

    /// <summary>Midpoint Sun/Mars = 342°. Saturn at 341° → orb 1°.</summary>
    [Test]
    public void TestDial360MatchNear360()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 341.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial360, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Mars);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 1.0), Is.LessThan(Delta));
    }

    /// <summary>Midpoint Mars/Jupiter = 352°. Saturn at 353° → orb 1°.</summary>
    [Test]
    public void TestDial360MatchMarsJupiterMidpoint()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 353.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial360, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Mars
                                   && m.Factor2 == Factors.Jupiter);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 1.0), Is.LessThan(Delta));
    }

    // MARK: - Dial360: result properties

    /// <summary>Result list must be sorted by ActualOrb ascending.</summary>
    [Test]
    public void TestDial360ResultSortedByOrb()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 100.3), (Factors.Uranus, 342.9) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial360, Orb);
        for (var i = 0; i < results.Count - 1; i++)
            Assert.That(results[i].ActualOrb, Is.LessThanOrEqualTo(results[i + 1].ActualOrb));
    }

    /// <summary>MaxOrb in every result equals the configured orb value.</summary>
    [Test]
    public void TestDial360MaxOrbEqualsConfigured()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 100.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial360, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.MaxOrb - Orb), Is.LessThan(Delta));
    }

    /// <summary>Stored positions are the original 360° longitudes, not reduced dial values.</summary>
    [Test]
    public void TestDial360StoredPositionsAreOriginal()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 101.2) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial360, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.MidpointPosition - 100.0), Is.LessThan(Delta));
        Assert.That(Math.Abs(match.MatchingPosition  - 101.2), Is.LessThan(Delta));
    }

    // MARK: - Dial360: edge cases

    [Test]
    public void TestDial360EmptyPositionsReturnsEmpty()
    {
        var results = MidpointMatchFinder.Find(BaseMidpoints, [], MidpointDialType.Dial360, Orb);
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void TestDial360EmptyMidpointsReturnsEmpty()
    {
        var results = MidpointMatchFinder.Find([], BasePositions, MidpointDialType.Dial360, Orb);
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void TestDial360ZeroOrbOnlyFindsExactMatches()
    {
        // Saturn at 100.0001° — just past exact; with orb=0 there should be no match.
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 100.0001) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial360, 0.0);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Null);
    }

    // MARK: - Dial90: matches

    /// <summary>
    /// In Dial90 every multiple of 90° folds to the same position.
    /// Midpoint Sun/Moon = 100°; 100 mod 90 = 10°.
    /// Saturn at 190° → 190 mod 90 = 10° → deviation = 0° → exact match.
    /// </summary>
    [Test]
    public void TestDial90NinetyDegreeExactMatch()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 190.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial90, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 0.0), Is.LessThan(Delta));
    }

    /// <summary>Saturn at 191° → 191 mod 90 = 11°; midpoint mod 90 = 10°; deviation = 1° ≤ 1.6°.</summary>
    [Test]
    public void TestDial90NinetyDegreeWithinOrb()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 191.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial90, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 1.0), Is.LessThan(Delta));
    }

    /// <summary>
    /// Pure 45° separation in Dial90: midpoint mod 90 = 10°; planet at 55° → 55 mod 90 = 55°;
    /// deviation = 45° — on the boundary, not within 1.6°.
    /// </summary>
    [Test]
    public void TestDial90PureFortyFiveDegreeIsNotMatch()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 55.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial90, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Null);
    }

    /// <summary>Stored positions in Dial90 result are the original 360° longitudes.</summary>
    [Test]
    public void TestDial90StoredPositionsAreOriginal()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 190.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial90, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.MatchingPosition  - 190.0), Is.LessThan(Delta));
        Assert.That(Math.Abs(match.MidpointPosition   - 100.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestDial90EmptyPositionsReturnsEmpty()
    {
        var results = MidpointMatchFinder.Find(BaseMidpoints, [], MidpointDialType.Dial90, Orb);
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void TestDial90ResultSortedByOrb()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 190.3), (Factors.Uranus, 191.2) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial90, Orb);
        for (var i = 0; i < results.Count - 1; i++)
            Assert.That(results[i].ActualOrb, Is.LessThanOrEqualTo(results[i + 1].ActualOrb));
    }

    // MARK: - Dial45: matches

    /// <summary>
    /// In Dial45 every multiple of 45° folds to the same position.
    /// Midpoint Sun/Moon = 100°; 100 mod 45 = 10°.
    /// Saturn at 55° → 55 mod 45 = 10° → deviation = 0° → exact match.
    /// </summary>
    [Test]
    public void TestDial45FortyFiveDegreeExactMatch()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 55.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial45, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 0.0), Is.LessThan(Delta));
    }

    /// <summary>Saturn at 56° → 56 mod 45 = 11°; midpoint mod 45 = 10°; deviation = 1° ≤ 1.6°.</summary>
    [Test]
    public void TestDial45FortyFiveDegreeWithinOrb()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 56.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial45, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 1.0), Is.LessThan(Delta));
    }

    /// <summary>
    /// A 22.5° separation sits at the fold boundary of Dial45 (dialSize/2 = 22.5°).
    /// With a wide orb of 23° it should be found with deviation = 22.5°.
    /// </summary>
    [Test]
    public void TestDial45TwentyTwoPointFiveWithWideOrb()
    {
        // midpoint Sun/Moon mod 45 = 10°; planet at 32.5° mod 45 = 32.5°; deviation = 22.5°
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 32.5) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial45, 23.0);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 22.5), Is.LessThan(Delta));
    }

    /// <summary>
    /// 90° separation in Dial45: 145 mod 45 = 10°; same as midpoint → deviation = 0°.
    /// </summary>
    [Test]
    public void TestDial45NinetyDegreeExactMatch()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 145.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial45, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn
                                   && m.Factor1 == Factors.Sun
                                   && m.Factor2 == Factors.Moon);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.ActualOrb - 0.0), Is.LessThan(Delta));
    }

    /// <summary>Stored positions in Dial45 result are the original 360° longitudes.</summary>
    [Test]
    public void TestDial45StoredPositionsAreOriginal()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 55.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial45, Orb);
        var match = results.Find(m => m.MatchingFactor == Factors.Saturn);
        Assert.That(match, Is.Not.Null);
        Assert.That(Math.Abs(match!.MatchingPosition  - 55.0),  Is.LessThan(Delta));
        Assert.That(Math.Abs(match.MidpointPosition   - 100.0), Is.LessThan(Delta));
    }

    [Test]
    public void TestDial45EmptyMidpointsReturnsEmpty()
    {
        var results = MidpointMatchFinder.Find([], BasePositions, MidpointDialType.Dial45, Orb);
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void TestDial45ResultSortedByOrb()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 55.0), (Factors.Uranus, 146.0) };
        var mids = MidpointsCalculator.Calculate(positions);
        var results = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial45, Orb);
        for (var i = 0; i < results.Count - 1; i++)
            Assert.That(results[i].ActualOrb, Is.LessThanOrEqualTo(results[i + 1].ActualOrb));
    }

    // MARK: - Cross-dial comparison

    /// <summary>
    /// A planet 45° from the midpoint matches in Dial45 but NOT in Dial360.
    /// Sun/Moon midpoint = 100°; Saturn at 55° = 45° away.
    /// Dial360: deviation = |55-100| = 45° → no match (orb 1.6°).
    /// Dial45: both reduce to 10°; deviation = 0° → match.
    /// </summary>
    [Test]
    public void TestDial45MatchNotInDial360()
    {
        var positions = new List<(Factors, double)>(BasePositions) { (Factors.Saturn, 55.0) };
        var mids = MidpointsCalculator.Calculate(positions);

        var resultsDial360 = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial360, Orb);
        var matchDial360 = resultsDial360.Find(m => m.MatchingFactor == Factors.Saturn
                                                 && m.Factor1 == Factors.Sun
                                                 && m.Factor2 == Factors.Moon);

        var resultsDial45 = MidpointMatchFinder.Find(mids, positions, MidpointDialType.Dial45, Orb);
        var matchDial45 = resultsDial45.Find(m => m.MatchingFactor == Factors.Saturn
                                               && m.Factor1 == Factors.Sun
                                               && m.Factor2 == Factors.Moon);

        Assert.That(matchDial360, Is.Null);
        Assert.That(matchDial45, Is.Not.Null);
    }
}
