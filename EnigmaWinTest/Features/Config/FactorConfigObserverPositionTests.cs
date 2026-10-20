// FactorConfigObserverPositionTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWintest.Features.Config;

/// <summary>Tests for FactorConfig.ForObserverPosition and UserConfiguration.ForCalculation.</summary>
[TestFixture]
public class FactorConfigObserverPositionTests
{
    private static FactorSettings SettingsFor(FactorConfig config, Factors factor) =>
        config.Settings.Single(s => s.Factor == factor);

    [Test]
    public void TestHeliocentricEarthTakesSettingsOfSun()
    {
        var result = FactorConfig.Default.ForObserverPosition(ObserverPositions.Heliocentric);
        var earth  = SettingsFor(result, Factors.Earth);
        var sun    = SettingsFor(FactorConfig.Default, Factors.Sun);
        Assert.That(earth.IsUsed, Is.EqualTo(sun.IsUsed));
        Assert.That(earth.IsDrawn, Is.EqualTo(sun.IsDrawn));
        Assert.That(earth.OrbPercentage, Is.EqualTo(sun.OrbPercentage));
        Assert.That(result.Settings.Count(s => s.Factor == Factors.Earth), Is.EqualTo(1));
    }

    [Test]
    public void TestGeocentricIsUnchanged()
    {
        var config = FactorConfig.Default;
        Assert.That(config.ForObserverPosition(ObserverPositions.Geocentric), Is.EqualTo(config));
        Assert.That(config.ForObserverPosition(ObserverPositions.Topocentric), Is.EqualTo(config));
    }

    [Test]
    public void TestHeliocentricKeepsEarthSettingsWhenEarthIsUsed()
    {
        var settings = FactorConfig.Default.Settings
            .Select(s => s.Factor == Factors.Earth ? s with { IsUsed = true, IsDrawn = false, OrbPercentage = 40 } : s)
            .ToList();
        var config = new FactorConfig(settings);
        var earth  = SettingsFor(config.ForObserverPosition(ObserverPositions.Heliocentric), Factors.Earth);
        Assert.That(earth.IsDrawn, Is.False);
        Assert.That(earth.OrbPercentage, Is.EqualTo(40));
    }

    [Test]
    public void TestHeliocentricWithoutSunLeavesEarthOff()
    {
        var settings = FactorConfig.Default.Settings
            .Select(s => s.Factor == Factors.Sun ? s with { IsUsed = false, IsDrawn = false } : s)
            .ToList();
        var earth = SettingsFor(new FactorConfig(settings).ForObserverPosition(ObserverPositions.Heliocentric),
                                Factors.Earth);
        Assert.That(earth.IsUsed, Is.False);
    }

    [Test]
    public void TestForCalculationDoesNotChangeOriginal()
    {
        var config = UserConfiguration.Default;
        config.CalculationConfig = config.CalculationConfig with { ObserverPosition = ObserverPositions.Heliocentric };
        var forCalc = config.ForCalculation();
        Assert.That(SettingsFor(forCalc.FactorConfig, Factors.Earth).IsUsed, Is.True);
        Assert.That(SettingsFor(config.FactorConfig, Factors.Earth).IsUsed, Is.False);
        Assert.That(forCalc.Id, Is.EqualTo(config.Id));
    }
}
