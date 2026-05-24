// AstronCalcOrchestratorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWintest.Features.AstronCalc;

/// <summary>Tests for AstronCalcOrchestrator.PerformSingleCoordinateCalculation.</summary>
[TestFixture]
public class AstronCalcOrchestratorTests
{
    // Julian Day for 2010-01-01 0:00 UT — matches the Apple reference tests
    private const double JulianDay = 2455197.5;
    private const double GeoLat    = 52.21805555555556;
    private const double GeoLon    = 6.895555555555555;
    private const int    HouseSystemId = 82;   // Regiomontanus

    private static CalculationConfig TestConfig => new(
        HouseSystems.NoHouses,
        Ayanamshas.Tropical,
        ObserverPositions.Geocentric,
        ProjectionTypes.TwoDimensional,
        BlackMoonCorrectionTypes.Duval,
        LunarNodeTypes.MeanNode,
        LotsTypes.Sect);

    private CalcRequest MakeRequest(List<Factors> factors) => new(
        JulianDay,
        factors,
        HouseSystemId,
        2,          // SEFlags
        GeoLat,
        GeoLon,
        0.0,
        TestConfig);

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
            // Initialization failure is handled per-test via _seAvailable
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

    // ── CommonSe ─────────────────────────────────────────────────────────────

    [Test]
    public void SingleCoord_CommonSe_Longitude_MatchesKnownValuesForMainPlanets()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var expectedLongitudes = new Dictionary<Factors, double>
        {
            [Factors.Sun]     = 280.4511630990,
            [Factors.Moon]    = 103.2443853269,
            [Factors.Mercury] = 288.9958888996,
            [Factors.Venus]   = 277.8499668252,
            [Factors.Mars]    = 138.8175005128,
            [Factors.Jupiter] = 326.3588628480,
            [Factors.Saturn]  = 184.5065798899,
            [Factors.Uranus]  = 353.0900907615,
            [Factors.Neptune] = 324.5820935220,
            [Factors.Pluto]   = 273.3088210717
        };

        foreach (var (factor, expected) in expectedLongitudes)
        {
            var request = MakeRequest([factor]);
            var (jd, longitude) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
                request, Coordinates.Longitude);

            Assert.That(jd, Is.EqualTo(JulianDay),
                $"Julian day must be preserved for {factor}");
            Assert.That(Math.Abs(longitude - expected), Is.LessThan(1e-3),
                $"Longitude for {factor}: expected {expected}, got {longitude}");
        }
    }

    [Test]
    public void SingleCoord_CommonSe_Latitude_ConsistentWithPerformCalculation()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.Sun]);
        var (_, latitude) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Latitude);

        var full = AstronCalcOrchestrator.PerformCalculation(request);
        var expected = full.Coordinates.TryGetValue(Factors.Sun, out var pos) &&
                       pos.Ecliptical.Length > 0
            ? pos.Ecliptical[0].Deviation
            : 0.0;

        Assert.That(Math.Abs(latitude - expected), Is.LessThan(1e-6),
            $"Latitude for Sun: expected {expected}, got {latitude}");
    }

    [Test]
    public void SingleCoord_CommonSe_RightAscension_ConsistentWithPerformCalculation()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.Sun]);
        var (_, ra) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.RightAscension);

        var full = AstronCalcOrchestrator.PerformCalculation(request);
        var expected = full.Coordinates.TryGetValue(Factors.Sun, out var pos) &&
                       pos.Equatorial.Length > 0
            ? pos.Equatorial[0].MainPos
            : 0.0;

        Assert.That(Math.Abs(ra - expected), Is.LessThan(1e-6),
            $"Right ascension for Sun: expected {expected}, got {ra}");
    }

    [Test]
    public void SingleCoord_CommonSe_Declination_ConsistentWithPerformCalculation()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.Sun]);
        var (_, decl) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Declination);

        var full = AstronCalcOrchestrator.PerformCalculation(request);
        var expected = full.Coordinates.TryGetValue(Factors.Sun, out var pos) &&
                       pos.Equatorial.Length > 0
            ? pos.Equatorial[0].Deviation
            : 0.0;

        Assert.That(Math.Abs(decl - expected), Is.LessThan(1e-6),
            $"Declination for Sun: expected {expected}, got {decl}");
    }

    [Test]
    public void SingleCoord_CommonSe_Distance_ConsistentWithPerformCalculation()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.Sun]);
        var (_, distance) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Distance);

        var full = AstronCalcOrchestrator.PerformCalculation(request);
        var expected = full.Coordinates.TryGetValue(Factors.Sun, out var pos) &&
                       pos.Ecliptical.Length > 0
            ? pos.Ecliptical[0].Distance
            : 0.0;

        Assert.That(Math.Abs(distance - expected), Is.LessThan(1e-9),
            $"Distance for Sun: expected {expected}, got {distance}");
    }

    // ── CommonElements ────────────────────────────────────────────────────────

    [Test]
    public void SingleCoord_CommonElements_Longitude_ConsistentWithPerformCalculation()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.PersephoneRam]);
        var (_, longitude) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Longitude);

        var full = AstronCalcOrchestrator.PerformCalculation(request);
        var expected = full.Coordinates.TryGetValue(Factors.PersephoneRam, out var pos) &&
                       pos.Ecliptical.Length > 0
            ? pos.Ecliptical[0].MainPos
            : 0.0;

        Assert.That(Math.Abs(longitude - expected), Is.LessThan(1e-6),
            $"Longitude for PersephoneRam: expected {expected}, got {longitude}");
    }

    [Test]
    public void SingleCoord_CommonElements_Declination_ConsistentWithPerformCalculation()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.PersephoneRam]);
        var (_, decl) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Declination);

        var full = AstronCalcOrchestrator.PerformCalculation(request);
        var expected = full.Coordinates.TryGetValue(Factors.PersephoneRam, out var pos) &&
                       pos.Equatorial.Length > 0
            ? pos.Equatorial[0].Deviation
            : 0.0;

        Assert.That(Math.Abs(decl - expected), Is.LessThan(1e-6),
            $"Declination for PersephoneRam: expected {expected}, got {decl}");
    }

    // ── CommonFormulaLongitude ────────────────────────────────────────────────

    [Test]
    public void SingleCoord_CommonFormulaLongitude_Longitude_ConsistentWithPerformCalculation()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.PersephoneCarteret]);
        var (_, longitude) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Longitude);

        var full = AstronCalcOrchestrator.PerformCalculation(request);
        full.Coordinates.TryGetValue(Factors.PersephoneCarteret, out var pos);
        var expected = pos is { Ecliptical.Length: > 0 } ? pos.Ecliptical[0].MainPos : 0.0;

        Assert.That(Math.Abs(longitude - expected), Is.LessThan(1e-6),
            $"Longitude for PersephoneCarteret: expected {expected}, got {longitude}");
    }

    // ── CommonFormulaFull ─────────────────────────────────────────────────────

    [Test]
    public void SingleCoord_CommonFormulaFull_Longitude_ConsistentWithPerformCalculation()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.SouthNodeMean]);
        var (_, longitude) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Longitude);

        var full = AstronCalcOrchestrator.PerformCalculation(request);
        var expected = full.Coordinates.TryGetValue(Factors.SouthNodeMean, out var pos) &&
                       pos.Ecliptical.Length > 0
            ? pos.Ecliptical[0].MainPos
            : 0.0;

        Assert.That(Math.Abs(longitude - expected), Is.LessThan(1e-6),
            $"Longitude for SouthNode: expected {expected}, got {longitude}");
    }

    [Test]
    public void SingleCoord_CommonFormulaFull_RightAscension_ConsistentWithPerformCalculation()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.SouthNodeMean]);
        var (_, ra) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.RightAscension);

        var full = AstronCalcOrchestrator.PerformCalculation(request);
        var expected = full.Coordinates.TryGetValue(Factors.SouthNodeMean, out var pos) &&
                       pos.Equatorial.Length > 0
            ? pos.Equatorial[0].MainPos
            : 0.0;

        Assert.That(Math.Abs(ra - expected), Is.LessThan(1e-6),
            $"Right ascension for SouthNode: expected {expected}, got {ra}");
    }

    // ── ZodiacFixed ───────────────────────────────────────────────────────────

    [Test]
    public void SingleCoord_ZodiacFixed_Longitude_IsZeroForTropicalZodiac()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.ZeroAries]);
        var (_, longitude) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Longitude);

        Assert.That(longitude, Is.EqualTo(0.0),
            $"Zero Aries longitude should be 0.0 for tropical zodiac, got {longitude}");
    }

    // ── Apsides ───────────────────────────────────────────────────────────────

    [Test]
    public void SingleCoord_Apsides_Longitude_ConsistentWithPerformCalculation()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.BlackSun]);
        var (_, longitude) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Longitude);

        var full = AstronCalcOrchestrator.PerformCalculation(request);
        var expected = full.Coordinates.TryGetValue(Factors.BlackSun, out var pos) &&
                       pos.Ecliptical.Length > 0
            ? pos.Ecliptical[0].MainPos
            : 0.0;

        Assert.That(Math.Abs(longitude - expected), Is.LessThan(1e-6),
            $"Longitude for BlackSun: expected {expected}, got {longitude}");
    }

    // ── Unsupported calculation types ─────────────────────────────────────────

    [Test]
    public void SingleCoord_LotsFactorReturnsZero()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.FortunaSect]);
        var (jd, position) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Longitude);

        Assert.That(jd, Is.EqualTo(JulianDay), "Julian day must be preserved for Lots factor");
        Assert.That(position, Is.EqualTo(0.0),
            $"FortunaSect (Lots) should return 0.0, got {position}");
    }

    [Test]
    public void SingleCoord_MundaneFactorReturnsZero()
    {
        if (!SeAvailable()) Assert.Ignore("Swiss Ephemeris DLL not available.");

        var request = MakeRequest([Factors.Ascendant]);
        var (jd, position) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Longitude);

        Assert.That(jd, Is.EqualTo(JulianDay), "Julian day must be preserved for Mundane factor");
        Assert.That(position, Is.EqualTo(0.0),
            $"Mundane factor should return 0.0, got {position}");
    }

    [Test]
    public void SingleCoord_EmptyFactorsReturnsZero()
    {
        var request = MakeRequest([]);
        var (jd, position) = AstronCalcOrchestrator.PerformSingleCoordinateCalculation(
            request, Coordinates.Longitude);

        Assert.That(jd, Is.EqualTo(JulianDay), "Julian day must be preserved for empty factors");
        Assert.That(position, Is.EqualTo(0.0),
            $"Empty factors should return 0.0, got {position}");
    }
}
