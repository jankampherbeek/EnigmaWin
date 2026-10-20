// HarmonicExactAspectsOrchestratorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicExactAspects;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.HarmonicExactAspects;

/// <summary>Tests for HarmonicExactAspectsOrchestrator.</summary>
[TestFixture]
public class HarmonicExactAspectsOrchestratorTests
{
    private const double Delta = 1e-8;

    private static readonly List<Aspects> ConjunctionOnly = [Aspects.Conjunction];

    [Test]
    public void TestOneResultPerHarmonicUpToMaximum()
    {
        var positions = new List<(Factors, double)> { (Factors.Sun, 10.0), (Factors.Moon, 47.0), (Factors.Mars, 200.0) };
        var result = HarmonicExactAspectsOrchestrator.Find(positions, HarmonicExactAspectsOrchestrator.MajorAspects, 12);
        // Ties can give more than one row for a harmonic, so count the distinct harmonics.
        Assert.That(result.Select(r => r.Harmonic).Distinct().Count(), Is.EqualTo(11));
        Assert.That(result[0].Harmonic, Is.EqualTo(2));
        Assert.That(result[^1].Harmonic, Is.EqualTo(12));
    }

    [Test]
    public void TestMaximumHarmonicIsRespected()
    {
        var positions = new List<(Factors, double)> { (Factors.Sun, 10.0), (Factors.Moon, 47.0) };
        var result = HarmonicExactAspectsOrchestrator.Find(positions, ConjunctionOnly, 20);
        Assert.That(result.Count, Is.EqualTo(19));
        Assert.That(result[^1].Harmonic, Is.EqualTo(20));
    }

    [Test]
    public void TestSelectsSmallestOrb()
    {
        // Sun-Moon 90° apart: exact conjunction in H4 (0°). Sun-Mars 91°: 4° off in H4.
        var positions = new List<(Factors, double)> { (Factors.Sun, 0.0), (Factors.Moon, 90.0), (Factors.Mars, 271.0) };
        var result = HarmonicExactAspectsOrchestrator.Find(positions, ConjunctionOnly, 4);
        var h4 = result.Find(r => r.Harmonic == 4)!;
        Assert.That(h4.Factor1, Is.EqualTo(Factors.Sun));
        Assert.That(h4.Factor2, Is.EqualTo(Factors.Moon));
        Assert.That(h4.Aspect, Is.EqualTo(Aspects.Conjunction));
        Assert.That(h4.Orb, Is.EqualTo(0.0).Within(Delta));
    }

    [Test]
    public void TestAllAspectsWithSameSmallestOrbAreReturned()
    {
        // Three factors 120° apart: in H3 all three pairs form an exact conjunction.
        var positions = new List<(Factors, double)> { (Factors.Sun, 0.0), (Factors.Moon, 120.0), (Factors.Mars, 240.0) };
        var result = HarmonicExactAspectsOrchestrator.Find(positions, ConjunctionOnly, 3);
        var h3 = result.FindAll(r => r.Harmonic == 3);
        Assert.That(h3.Count, Is.EqualTo(3));
        Assert.That(h3.TrueForAll(r => r.Orb < Delta), Is.True);
    }

    [Test]
    public void TestOrbIsMeasuredInHarmonicChart()
    {
        // Distance 61° → in H2 the distance is 122°; nearest of major aspects is trine (120°), orb 2°.
        var positions = new List<(Factors, double)> { (Factors.Sun, 0.0), (Factors.Moon, 61.0) };
        var result = HarmonicExactAspectsOrchestrator.Find(positions, HarmonicExactAspectsOrchestrator.MajorAspects, 2);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Aspect, Is.EqualTo(Aspects.Trine));
        Assert.That(result[0].Orb, Is.EqualTo(2.0).Within(Delta));
    }

    [Test]
    public void TestImplicitPairsAreSkipped()
    {
        // Mean north and south node are always in opposition; this pair should never be reported.
        var positions = new List<(Factors, double)> { (Factors.NorthNodeMean, 10.0), (Factors.SouthNodeMean, 190.0) };
        var result = HarmonicExactAspectsOrchestrator.Find(positions, HarmonicExactAspectsOrchestrator.MajorAspects, 12);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestFewerThanTwoFactorsGivesEmptyResult()
    {
        var positions = new List<(Factors, double)> { (Factors.Sun, 10.0) };
        var result = HarmonicExactAspectsOrchestrator.Find(positions, HarmonicExactAspectsOrchestrator.MajorAspects, 12);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestNoAspectsGivesEmptyResult()
    {
        var positions = new List<(Factors, double)> { (Factors.Sun, 10.0), (Factors.Moon, 47.0) };
        var result = HarmonicExactAspectsOrchestrator.Find(positions, [], 12);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestMajorAspectsContainsFiveAspects()
    {
        Assert.That(HarmonicExactAspectsOrchestrator.MajorAspects, Is.EquivalentTo(new[]
        {
            Aspects.Conjunction, Aspects.Opposition, Aspects.Square, Aspects.Trine, Aspects.Sextile
        }));
    }
}
