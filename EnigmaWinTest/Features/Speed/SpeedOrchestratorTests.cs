// SpeedOrchestratorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Speed;

namespace EnigmaWintest.Features.Speed;

/// <summary>
/// Tests for SpeedOrchestrator.
/// Mars is used as the reference factor throughout most tests.
/// AverageSpeed.AverageFor(Mars) = 0.5242 °/day
/// Default config: stationaryPercentage=10, slowPercentage=20
///   stationaryThreshold = 0.5242 × 0.10 = 0.05242
///   slowThreshold        = 0.5242 × 0.20 = 0.10484
/// </summary>
[TestFixture]
public class SpeedOrchestratorTests
{
    // MARK: - Direct motion

    /// <summary>0.40 > slowThreshold (0.10484) → Direct.</summary>
    [Test]
    public void TestNormalDirectSpeedReturnsDirect()
    {
        var result = SpeedOrchestrator.Determine(0.40, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.Direct));
    }

    /// <summary>0.08 > stationaryThreshold and ≤ slowThreshold → Slow.</summary>
    [Test]
    public void TestSlowDirectSpeedReturnsSlow()
    {
        var result = SpeedOrchestrator.Determine(0.08, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.Slow));
    }

    /// <summary>0.02 ≤ stationaryThreshold (0.05242) → StationaryDirect.</summary>
    [Test]
    public void TestVerySlowDirectSpeedReturnsStationaryDirect()
    {
        var result = SpeedOrchestrator.Determine(0.02, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.StationaryDirect));
    }

    /// <summary>Speed exactly zero → StationaryDirect (not retrograde).</summary>
    [Test]
    public void TestZeroSpeedReturnsStationaryDirect()
    {
        var result = SpeedOrchestrator.Determine(0.0, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.StationaryDirect));
    }

    // MARK: - Retrograde motion

    /// <summary>abs(-0.40) = 0.40 > slowThreshold → Retrograde.</summary>
    [Test]
    public void TestNormalRetrogradeSpeedReturnsRetrograde()
    {
        var result = SpeedOrchestrator.Determine(-0.40, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.Retrograde));
    }

    /// <summary>abs(-0.08) > stationaryThreshold and ≤ slowThreshold → SlowRetrograde.</summary>
    [Test]
    public void TestSlowRetrogradeSpeedReturnsSlowRetrograde()
    {
        var result = SpeedOrchestrator.Determine(-0.08, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.SlowRetrograde));
    }

    /// <summary>abs(-0.02) ≤ stationaryThreshold → StationaryRetrograde.</summary>
    [Test]
    public void TestVerySlowRetrogradeSpeedReturnsStationaryRetrograde()
    {
        var result = SpeedOrchestrator.Determine(-0.02, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.StationaryRetrograde));
    }

    // MARK: - Boundaries around stationaryThreshold

    /// <summary>Speed exactly at stationaryThreshold (0.05242) → StationaryDirect.</summary>
    [Test]
    public void TestExactlyAtStationaryThresholdDirectReturnsStationaryDirect()
    {
        var result = SpeedOrchestrator.Determine(0.05242, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.StationaryDirect));
    }

    /// <summary>Speed just above stationaryThreshold (0.05243) → Slow.</summary>
    [Test]
    public void TestJustAboveStationaryThresholdDirectReturnsSlow()
    {
        var result = SpeedOrchestrator.Determine(0.05243, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.Slow));
    }

    /// <summary>Retrograde speed exactly at stationaryThreshold (-0.05242) → StationaryRetrograde.</summary>
    [Test]
    public void TestExactlyAtStationaryThresholdRetrogradeReturnsStationaryRetrograde()
    {
        var result = SpeedOrchestrator.Determine(-0.05242, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.StationaryRetrograde));
    }

    /// <summary>Retrograde speed just above stationaryThreshold (-0.05243) → SlowRetrograde.</summary>
    [Test]
    public void TestJustAboveStationaryThresholdRetrogradeReturnsSlowRetrograde()
    {
        var result = SpeedOrchestrator.Determine(-0.05243, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.SlowRetrograde));
    }

    // MARK: - Boundaries around slowThreshold

    /// <summary>Speed exactly at slowThreshold (0.10484) → Slow.</summary>
    [Test]
    public void TestExactlyAtSlowThresholdDirectReturnsSlow()
    {
        var result = SpeedOrchestrator.Determine(0.10484, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.Slow));
    }

    /// <summary>Speed just above slowThreshold (0.10485) → Direct.</summary>
    [Test]
    public void TestJustAboveSlowThresholdDirectReturnsDirect()
    {
        var result = SpeedOrchestrator.Determine(0.10485, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.Direct));
    }

    /// <summary>Retrograde speed exactly at slowThreshold (-0.10484) → SlowRetrograde.</summary>
    [Test]
    public void TestExactlyAtSlowThresholdRetrogradeReturnsSlowRetrograde()
    {
        var result = SpeedOrchestrator.Determine(-0.10484, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.SlowRetrograde));
    }

    /// <summary>Retrograde speed just above slowThreshold (-0.10485) → Retrograde.</summary>
    [Test]
    public void TestJustAboveSlowThresholdRetrogradeReturnsRetrograde()
    {
        var result = SpeedOrchestrator.Determine(-0.10485, Factors.Mars, Config());
        Assert.That(result, Is.EqualTo(SpeedType.Retrograde));
    }

    // MARK: - Factors without average speed

    /// <summary>Factor without average speed and positive speed → Direct.</summary>
    [Test]
    public void TestNoAverageSpeedPositiveReturnsDirect()
    {
        var result = SpeedOrchestrator.Determine(0.05, Factors.NorthNodeMean, Config());
        Assert.That(result, Is.EqualTo(SpeedType.Direct));
    }

    /// <summary>Factor without average speed and negative speed → Retrograde.</summary>
    [Test]
    public void TestNoAverageSpeedNegativeReturnsRetrograde()
    {
        var result = SpeedOrchestrator.Determine(-0.05, Factors.NorthNodeMean, Config());
        Assert.That(result, Is.EqualTo(SpeedType.Retrograde));
    }

    /// <summary>Factor without average speed and zero speed → Direct.</summary>
    [Test]
    public void TestNoAverageSpeedZeroReturnsDirect()
    {
        var result = SpeedOrchestrator.Determine(0.0, Factors.NorthNodeMean, Config());
        Assert.That(result, Is.EqualTo(SpeedType.Direct));
    }

    // MARK: - Custom config

    /// <summary>stationaryPct=50 widens the stationary zone; speed 0.20 that is Direct by default becomes StationaryDirect.</summary>
    [Test]
    public void TestHigherStationaryPercentageWidensStationaryZone()
    {
        // default: stationaryThreshold = 0.5242 × 0.10 = 0.05242 → 0.20 > threshold → Direct
        // custom:  stationaryThreshold = 0.5242 × 0.50 = 0.2621  → 0.20 ≤ threshold → StationaryDirect
        var resultDefault = SpeedOrchestrator.Determine(0.20, Factors.Mars, Config());
        var resultCustom  = SpeedOrchestrator.Determine(0.20, Factors.Mars, Config(stationaryPct: 50, slowPct: 80));
        Assert.That(resultDefault, Is.EqualTo(SpeedType.Direct));
        Assert.That(resultCustom,  Is.EqualTo(SpeedType.StationaryDirect));
    }

    /// <summary>slowPct=80 widens the slow zone; speed 0.30 that is Direct by default becomes Slow.</summary>
    [Test]
    public void TestHigherSlowPercentageWidensSlowZone()
    {
        // default: slowThreshold = 0.5242 × 0.20 = 0.10484 → 0.30 > threshold → Direct
        // custom:  slowThreshold = 0.5242 × 0.80 = 0.41936 → 0.30 ≤ threshold → Slow
        var resultDefault = SpeedOrchestrator.Determine(0.30, Factors.Mars, Config());
        var resultCustom  = SpeedOrchestrator.Determine(0.30, Factors.Mars, Config(slowPct: 80));
        Assert.That(resultDefault, Is.EqualTo(SpeedType.Direct));
        Assert.That(resultCustom,  Is.EqualTo(SpeedType.Slow));
    }

    // MARK: - All planets with average speed

    /// <summary>All planets return Slow when speed is 15% of their average (between stationary 10% and slow 20% threshold).</summary>
    [Test]
    public void TestAllPlanetsSlowAtFifteenPercentOfAverage()
    {
        var planets = new List<(Factors, double)>
        {
            (Factors.Sun,     0.985555  * 0.15),
            (Factors.Moon,    13.17666  * 0.15),
            (Factors.Mercury, 1.3833    * 0.15),
            (Factors.Venus,   1.2       * 0.15),
            (Factors.Mars,    0.5242    * 0.15),
            (Factors.Jupiter, 0.0831    * 0.15),
            (Factors.Saturn,  0.0336    * 0.15),
            (Factors.Uranus,  0.026666  * 0.15),
            (Factors.Neptune, 0.006668  * 0.15),
            (Factors.Pluto,   0.0041666 * 0.15)
        };
        foreach (var (factor, speed) in planets)
        {
            var result = SpeedOrchestrator.Determine(speed, factor, Config());
            Assert.That(result, Is.EqualTo(SpeedType.Slow),
                $"Expected Slow for {factor} at speed {speed}");
        }
    }

    /// <summary>All planets return StationaryDirect when speed is 5% of their average (below stationary 10% threshold).</summary>
    [Test]
    public void TestAllPlanetsStationaryDirectAtFivePercentOfAverage()
    {
        var planets = new List<(Factors, double)>
        {
            (Factors.Sun,     0.985555  * 0.05),
            (Factors.Moon,    13.17666  * 0.05),
            (Factors.Mercury, 1.3833    * 0.05),
            (Factors.Venus,   1.2       * 0.05),
            (Factors.Mars,    0.5242    * 0.05),
            (Factors.Jupiter, 0.0831    * 0.05),
            (Factors.Saturn,  0.0336    * 0.05),
            (Factors.Uranus,  0.026666  * 0.05),
            (Factors.Neptune, 0.006668  * 0.05),
            (Factors.Pluto,   0.0041666 * 0.05)
        };
        foreach (var (factor, speed) in planets)
        {
            var result = SpeedOrchestrator.Determine(speed, factor, Config());
            Assert.That(result, Is.EqualTo(SpeedType.StationaryDirect),
                $"Expected StationaryDirect for {factor} at speed {speed}");
        }
    }

    // MARK: - Helpers

    private static CalculationConfig Config(int stationaryPct = 10, int slowPct = 20) =>
        CalculationConfig.Default with { StationaryPercentage = stationaryPct, SlowPercentage = slowPct };
}
