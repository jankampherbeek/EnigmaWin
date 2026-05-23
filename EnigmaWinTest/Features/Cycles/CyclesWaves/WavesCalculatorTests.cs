// WavesCalculatorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Cycles.CyclesWaves;

namespace EnigmaWintest.Features.Cycles.CyclesWaves;

/// <summary>Tests for WavesCalculator.PerformCalculation.</summary>
[TestFixture]
public class WavesCalculatorTests
{
    private const double BaseJd   = 2455197.5;   // 2010-01-01 0:00 UT
    private const int    Interval = 10;
    private const double Delta    = 1e-6;

    [SetUp]
    public void SetUp()
    {
        try
        {
            var ephePath = Environment.GetEnvironmentVariable("EPHE_PATH") ?? Environment.CurrentDirectory;
            SEWrapper.SeInitializer(ephePath);
        }
        catch (Exception)
        {
            // Initialization failure is handled per-test via SeAvailable()
        }
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

    private static double GetLongitude(Factors factor, double jd)
    {
        var config = new CalculationConfig(
            HouseSystems.NoHouses,
            Ayanamshas.Tropical,
            ObserverPositions.Geocentric,
            ProjectionTypes.TwoDimensional,
            BlackMoonCorrectionTypes.Duval,
            LunarNodeTypes.MeanNode,
            LotsTypes.Sect);
        var request = new CalcRequest(jd, [factor], (int)HouseSystems.NoHouses.SeId(),
            0, 0.0, 0.0, 0.0, config);
        var (_, pos) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Longitude);
        return pos;
    }

    private static double ShortestArc(double p1, double p2)
    {
        var diff = Math.Abs(p1 - p2) % 360.0;
        return diff > 180.0 ? 360.0 - diff : diff;
    }

    // ── Saturn cycle ──────────────────────────────────────────────────────────

    [Test]
    public void Saturn_SingleStep_ReturnsOneResult()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Saturn, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results, Has.Count.EqualTo(1));
    }

    [Test]
    public void Saturn_ThreeSteps_ReturnsThreeResults()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var endJd = BaseJd + 2 * Interval;
        var request = new WavesRequest(Factors.Saturn, Interval, BaseJd, endJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results, Has.Count.EqualTo(3));
    }

    [Test]
    public void Saturn_JdSequence_IsCorrect()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var endJd = BaseJd + 2 * Interval;
        var request = new WavesRequest(Factors.Saturn, Interval, BaseJd, endJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results[0].JulianDay, Is.EqualTo(BaseJd).Within(Delta));
        Assert.That(results[1].JulianDay, Is.EqualTo(BaseJd + Interval).Within(Delta));
        Assert.That(results[2].JulianDay, Is.EqualTo(BaseJd + 2 * Interval).Within(Delta));
    }

    [Test]
    public void Saturn_EndOnStep_IsIncluded()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var endJd = BaseJd + Interval;
        var request = new WavesRequest(Factors.Saturn, Interval, BaseJd, endJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results[1].JulianDay, Is.EqualTo(endJd).Within(Delta));
    }

    [Test]
    public void Saturn_EndBetweenSteps_StopsBeforeExceeding()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var endJd = BaseJd + Interval - 1.0;
        var request = new WavesRequest(Factors.Saturn, Interval, BaseJd, endJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].JulianDay, Is.LessThanOrEqualTo(endJd));
    }

    [Test]
    public void Saturn_StartAfterEnd_ReturnsEmpty()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Saturn, Interval, BaseJd + 1.0, BaseJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Saturn_FirstJd_EqualsStart()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Saturn, Interval, BaseJd, BaseJd + Interval,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results[0].JulianDay, Is.EqualTo(BaseJd).Within(Delta));
    }

    [Test]
    public void Saturn_WaveValue_IsNonNegative()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Saturn, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results[0].WaveValue, Is.GreaterThanOrEqualTo(0.0));
    }

    [Test]
    public void Saturn_WaveValue_IsAtMost1080()
    {
        // Saturn cycle has 4 planets → 6 pairs; each pair ≤ 180° → max 1080°
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Saturn, Interval, BaseJd, BaseJd + 2 * Interval,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        foreach (var (_, waveValue) in results)
            Assert.That(waveValue, Is.LessThanOrEqualTo(1080.0 + Delta));
    }

    [Test]
    public void Saturn_WaveValue_MatchesManualComputation()
    {
        // Verify against manually summed pairwise arcs for Saturn, Uranus, Neptune, Pluto
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var satLon = GetLongitude(Factors.Saturn,  BaseJd);
        var uraLon = GetLongitude(Factors.Uranus,  BaseJd);
        var nepLon = GetLongitude(Factors.Neptune, BaseJd);
        var pluLon = GetLongitude(Factors.Pluto,   BaseJd);

        var expected = ShortestArc(satLon, uraLon)
                     + ShortestArc(satLon, nepLon)
                     + ShortestArc(satLon, pluLon)
                     + ShortestArc(uraLon, nepLon)
                     + ShortestArc(uraLon, pluLon)
                     + ShortestArc(nepLon, pluLon);

        var request = new WavesRequest(Factors.Saturn, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results[0].WaveValue, Is.EqualTo(expected).Within(Delta));
    }

    [Test]
    public void Saturn_Determinism_SameInputSameOutput()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Saturn, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var first  = WavesCalculator.PerformCalculation(request);
        var second = WavesCalculator.PerformCalculation(request);

        Assert.That(first[0].WaveValue, Is.EqualTo(second[0].WaveValue).Within(Delta));
    }

    // ── Jupiter cycle ─────────────────────────────────────────────────────────

    [Test]
    public void Jupiter_SingleStep_ReturnsOneResult()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Jupiter, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results, Has.Count.EqualTo(1));
    }

    [Test]
    public void Jupiter_ThreeSteps_ReturnsThreeResults()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var endJd = BaseJd + 2 * Interval;
        var request = new WavesRequest(Factors.Jupiter, Interval, BaseJd, endJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results, Has.Count.EqualTo(3));
    }

    [Test]
    public void Jupiter_WaveValue_IsNonNegative()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Jupiter, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results[0].WaveValue, Is.GreaterThanOrEqualTo(0.0));
    }

    [Test]
    public void Jupiter_WaveValue_IsAtMost1800()
    {
        // Jupiter cycle has 5 planets → 10 pairs; each pair ≤ 180° → max 1800°
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Jupiter, Interval, BaseJd, BaseJd + 2 * Interval,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        foreach (var (_, waveValue) in results)
            Assert.That(waveValue, Is.LessThanOrEqualTo(1800.0 + Delta));
    }

    [Test]
    public void Jupiter_WaveValue_MatchesManualComputation()
    {
        // Verify against manually summed pairwise arcs for Jupiter, Saturn, Uranus, Neptune, Pluto
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var jupLon = GetLongitude(Factors.Jupiter, BaseJd);
        var satLon = GetLongitude(Factors.Saturn,  BaseJd);
        var uraLon = GetLongitude(Factors.Uranus,  BaseJd);
        var nepLon = GetLongitude(Factors.Neptune, BaseJd);
        var pluLon = GetLongitude(Factors.Pluto,   BaseJd);

        var expected = ShortestArc(jupLon, satLon)
                     + ShortestArc(jupLon, uraLon)
                     + ShortestArc(jupLon, nepLon)
                     + ShortestArc(jupLon, pluLon)
                     + ShortestArc(satLon, uraLon)
                     + ShortestArc(satLon, nepLon)
                     + ShortestArc(satLon, pluLon)
                     + ShortestArc(uraLon, nepLon)
                     + ShortestArc(uraLon, pluLon)
                     + ShortestArc(nepLon, pluLon);

        var request = new WavesRequest(Factors.Jupiter, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results[0].WaveValue, Is.EqualTo(expected).Within(Delta));
    }

    [Test]
    public void Jupiter_WaveValue_DiffersFromSaturn()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var satRequest = new WavesRequest(Factors.Saturn,  Interval, BaseJd, BaseJd, Coordinates.Longitude);
        var jupRequest = new WavesRequest(Factors.Jupiter, Interval, BaseJd, BaseJd, Coordinates.Longitude);

        var satValue = WavesCalculator.PerformCalculation(satRequest)[0].WaveValue;
        var jupValue = WavesCalculator.PerformCalculation(jupRequest)[0].WaveValue;

        Assert.That(jupValue, Is.Not.EqualTo(satValue).Within(Delta));
    }

    [Test]
    public void Jupiter_Determinism_SameInputSameOutput()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Jupiter, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var first  = WavesCalculator.PerformCalculation(request);
        var second = WavesCalculator.PerformCalculation(request);

        Assert.That(first[0].WaveValue, Is.EqualTo(second[0].WaveValue).Within(Delta));
    }

    // ── Uranus cycle ──────────────────────────────────────────────────────────

    [Test]
    public void Uranus_SingleStep_ReturnsOneResult()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Uranus, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results, Has.Count.EqualTo(1));
    }

    [Test]
    public void Uranus_ThreeSteps_ReturnsThreeResults()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var endJd = BaseJd + 2 * Interval;
        var request = new WavesRequest(Factors.Uranus, Interval, BaseJd, endJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results, Has.Count.EqualTo(3));
    }

    [Test]
    public void Uranus_WaveValue_IsNonNegative()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Uranus, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results[0].WaveValue, Is.GreaterThanOrEqualTo(0.0));
    }

    [Test]
    public void Uranus_WaveValue_IsAtMost540()
    {
        // Uranus cycle has 3 planets → 3 pairs; each pair ≤ 180° → max 540°
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Uranus, Interval, BaseJd, BaseJd + 2 * Interval,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        foreach (var (_, waveValue) in results)
            Assert.That(waveValue, Is.LessThanOrEqualTo(540.0 + Delta));
    }

    [Test]
    public void Uranus_WaveValue_MatchesManualComputation()
    {
        // Verify against manually summed pairwise arcs for Uranus, Neptune, Pluto
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var uraLon = GetLongitude(Factors.Uranus,  BaseJd);
        var nepLon = GetLongitude(Factors.Neptune, BaseJd);
        var pluLon = GetLongitude(Factors.Pluto,   BaseJd);

        var expected = ShortestArc(uraLon, nepLon)
                     + ShortestArc(uraLon, pluLon)
                     + ShortestArc(nepLon, pluLon);

        var request = new WavesRequest(Factors.Uranus, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var results = WavesCalculator.PerformCalculation(request);

        Assert.That(results[0].WaveValue, Is.EqualTo(expected).Within(Delta));
    }

    [Test]
    public void Uranus_WaveValue_DiffersFromJupiter()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var jupRequest = new WavesRequest(Factors.Jupiter, Interval, BaseJd, BaseJd, Coordinates.Longitude);
        var uraRequest = new WavesRequest(Factors.Uranus,  Interval, BaseJd, BaseJd, Coordinates.Longitude);

        var jupValue = WavesCalculator.PerformCalculation(jupRequest)[0].WaveValue;
        var uraValue = WavesCalculator.PerformCalculation(uraRequest)[0].WaveValue;

        Assert.That(uraValue, Is.Not.EqualTo(jupValue).Within(Delta));
    }

    [Test]
    public void Uranus_Determinism_SameInputSameOutput()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = new WavesRequest(Factors.Uranus, Interval, BaseJd, BaseJd,
            Coordinates.Longitude);
        var first  = WavesCalculator.PerformCalculation(request);
        var second = WavesCalculator.PerformCalculation(request);

        Assert.That(first[0].WaveValue, Is.EqualTo(second[0].WaveValue).Within(Delta));
    }
}
