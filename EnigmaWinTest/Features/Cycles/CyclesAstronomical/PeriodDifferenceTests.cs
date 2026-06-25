// PeriodDifferenceTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Cycles.CyclesAstronomical;

namespace EnigmaWintest.Features.Cycles.CyclesAstronomical;

/// <summary>Tests for PeriodDifference.PerformCalculation.</summary>
[TestFixture]
public class PeriodDifferenceTests
{
    private const double BaseJd   = 2455197.5;   // 2010-01-01 0:00 UT
    private const double Interval = 10.0;
    private const double Delta    = 1e-6;

    [SetUp]
    public void SetUp()
    {
        try
        {
            var ephePath = Environment.GetEnvironmentVariable("EPHE_PATH") ?? Environment.CurrentDirectory;
            SEWrapper.SeInitializer(ephePath);
        }
        catch (Exception) { /* handled per-test via SeAvailable() */ }
    }

    [TearDown]
    public void TearDown()
    {
        try { SEWrapper.CloseEphemeris(); }
        catch (Exception) { /* ignore */ }
    }

    private static bool SeAvailable()
    {
        try { _ = new SEWrapper(); return true; }
        catch (Exception) { return false; }
    }

    private static PeriodDifferenceRequest MakeRequest(
        Factors factor1 = Factors.Sun,
        Factors factor2 = Factors.Moon,
        double endOffset = 0.0,
        Coordinates coordinate = Coordinates.Longitude,
        Ayanamshas ayanamsha = Ayanamshas.Tropical) =>
        new(factor1, factor2, Interval, BaseJd, BaseJd + endOffset,
            coordinate, ayanamsha);

    private static double GetPosition(Factors factor, double jd, Coordinates coordinate)
    {
        var config = new CalculationConfig(
            HouseSystems.NoHouses, Ayanamshas.Tropical, ObserverPositions.Geocentric,
            ProjectionTypes.TwoDimensional,
            LunarNodeTypes.MeanNode, LotsTypes.Sect);
        var request = new CalcRequest(jd, [factor], (int)HouseSystems.NoHouses.SeId(),
            0, 0.0, 0.0, 0.0, config);
        var (_, pos) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(request, coordinate);
        return pos;
    }

    // ── Result count and JD sequence ─────────────────────────────────────────

    [Test]
    public void SingleStep_ReturnsOneResult()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var results = PeriodDifference.PerformCalculation(MakeRequest());

        Assert.That(results, Has.Count.EqualTo(1));
    }

    [Test]
    public void ThreeSteps_ReturnsThreeResults()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var results = PeriodDifference.PerformCalculation(MakeRequest(endOffset: Interval * 2));

        Assert.That(results, Has.Count.EqualTo(3));
    }

    [Test]
    public void MultipleSteps_JulianDaySequenceIsCorrect()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var results = PeriodDifference.PerformCalculation(MakeRequest(endOffset: Interval * 2));

        for (var i = 0; i < results.Count; i++)
            Assert.That(results[i].JulianDay, Is.EqualTo(BaseJd + Interval * i).Within(Delta),
                $"Step {i}: JD mismatch");
    }

    [Test]
    public void EndJdExactlyOnStep_IsIncluded()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var results = PeriodDifference.PerformCalculation(MakeRequest(endOffset: Interval * 2));

        Assert.That(results, Has.Count.EqualTo(3));
        Assert.That(results[^1].JulianDay, Is.EqualTo(BaseJd + Interval * 2).Within(Delta));
    }

    [Test]
    public void EndJdBetweenSteps_StopsBeforeExceeding()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var endOffset = Interval * 2 + Interval / 2;
        var results = PeriodDifference.PerformCalculation(MakeRequest(endOffset: endOffset));

        Assert.That(results, Has.Count.EqualTo(3));
        Assert.That(results[^1].JulianDay, Is.LessThanOrEqualTo(BaseJd + endOffset));
    }

    [Test]
    public void StartAfterEnd_ReturnsEmpty()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new PeriodDifferenceRequest(
            Factors.Sun, Factors.Moon, Interval,
            BaseJd + 1.0, BaseJd,
            Coordinates.Longitude, Ayanamshas.Tropical);
        var results = PeriodDifference.PerformCalculation(request);

        Assert.That(results, Is.Empty);
    }

    // ── Difference correctness: same factor ───────────────────────────────────

    [Test]
    public void SameFactor_LongitudeDifferenceIsZero()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var results = PeriodDifference.PerformCalculation(
            MakeRequest(factor1: Factors.Sun, factor2: Factors.Sun,
                coordinate: Coordinates.Longitude));

        Assert.That(results[0].Difference, Is.EqualTo(0.0).Within(Delta),
            $"Sun vs Sun longitude difference should be 0, got {results[0].Difference}");
    }

    [Test]
    public void SameFactor_DeclinationDifferenceIsZero()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var results = PeriodDifference.PerformCalculation(
            MakeRequest(factor1: Factors.Moon, factor2: Factors.Moon,
                coordinate: Coordinates.Declination));

        Assert.That(results[0].Difference, Is.EqualTo(0.0).Within(Delta),
            $"Moon vs Moon declination difference should be 0, got {results[0].Difference}");
    }

    // ── Difference correctness: manual computation ────────────────────────────

    [Test]
    public void LongitudeDifference_MatchesManualShortestArc()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var results = PeriodDifference.PerformCalculation(
            MakeRequest(factor1: Factors.Sun, factor2: Factors.Moon,
                coordinate: Coordinates.Longitude));

        var lon1 = GetPosition(Factors.Sun,  BaseJd, Coordinates.Longitude);
        var lon2 = GetPosition(Factors.Moon, BaseJd, Coordinates.Longitude);
        var diff = Math.Abs(lon1 - lon2) % 360.0;
        var expected = diff > 180.0 ? 360.0 - diff : diff;

        Assert.That(results[0].Difference, Is.EqualTo(expected).Within(Delta),
            $"Expected longitude difference {expected}, got {results[0].Difference}");
    }

    [Test]
    public void DeclinationDifference_MatchesManualAbsoluteComputation()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var results = PeriodDifference.PerformCalculation(
            MakeRequest(factor1: Factors.Sun, factor2: Factors.Moon,
                coordinate: Coordinates.Declination));

        var decl1 = GetPosition(Factors.Sun,  BaseJd, Coordinates.Declination);
        var decl2 = GetPosition(Factors.Moon, BaseJd, Coordinates.Declination);
        var expected = Math.Abs(decl1 - decl2);

        Assert.That(results[0].Difference, Is.EqualTo(expected).Within(Delta),
            $"Expected declination difference {expected}, got {results[0].Difference}");
    }

    // ── Symmetry ──────────────────────────────────────────────────────────────

    [Test]
    public void SwappedFactors_GiveSameLongitudeDifference()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var forward = PeriodDifference.PerformCalculation(
            MakeRequest(factor1: Factors.Sun, factor2: Factors.Saturn,
                coordinate: Coordinates.Longitude));
        var swapped = PeriodDifference.PerformCalculation(
            MakeRequest(factor1: Factors.Saturn, factor2: Factors.Sun,
                coordinate: Coordinates.Longitude));

        Assert.That(forward[0].Difference, Is.EqualTo(swapped[0].Difference).Within(Delta),
            "Sun-Saturn and Saturn-Sun should give identical difference");
    }

    // ── Range constraints ─────────────────────────────────────────────────────

    [Test]
    public void LongitudeDifference_IsInZeroTo180()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var results = PeriodDifference.PerformCalculation(
            MakeRequest(factor1: Factors.Sun, factor2: Factors.Moon,
                endOffset: Interval * 2, coordinate: Coordinates.Longitude));

        foreach (var (jd, diff) in results)
        {
            Assert.That(diff, Is.GreaterThanOrEqualTo(0.0),
                $"Longitude difference {diff} at JD {jd} is negative");
            Assert.That(diff, Is.LessThanOrEqualTo(180.0 + Delta),
                $"Longitude difference {diff} at JD {jd} exceeds 180°");
        }
    }

    [Test]
    public void DeclinationDifference_IsNonNegative()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var results = PeriodDifference.PerformCalculation(
            MakeRequest(factor1: Factors.Sun, factor2: Factors.Moon,
                endOffset: Interval * 2, coordinate: Coordinates.Declination));

        foreach (var (jd, diff) in results)
            Assert.That(diff, Is.GreaterThanOrEqualTo(0.0),
                $"Declination difference {diff} at JD {jd} is negative");
    }

    [Test]
    public void DistanceDifference_IsNonNegative()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var results = PeriodDifference.PerformCalculation(
            MakeRequest(factor1: Factors.Sun, factor2: Factors.Moon,
                endOffset: Interval * 2, coordinate: Coordinates.Distance));

        foreach (var (jd, diff) in results)
            Assert.That(diff, Is.GreaterThanOrEqualTo(0.0),
                $"Distance difference {diff} at JD {jd} is negative");
    }

    // ── Determinism ───────────────────────────────────────────────────────────

    [Test]
    public void Determinism_SameInputSameOutput()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest(factor1: Factors.Sun, factor2: Factors.Moon,
            coordinate: Coordinates.Longitude);
        var first  = PeriodDifference.PerformCalculation(request);
        var second = PeriodDifference.PerformCalculation(request);

        Assert.That(first.Count, Is.EqualTo(second.Count));
        Assert.That(first[0].Difference, Is.EqualTo(second[0].Difference).Within(Delta));
    }
}
