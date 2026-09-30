// AltZodiacStartOrchestratorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.AltZodiacStart;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.AltZodiacStart;

/// <summary>Tests for the conversion of longitudes into an alternative zodiac.</summary>
[TestFixture]
public class AltZodiacStartOrchestratorTests
{
    private const double Delta = 1e-9;

    [Test]
    public void TestStartFactorItselfIsZeroAries()
    {
        Assert.That(AltZodiacStartOrchestrator.ShiftedLongitude(123.456, 123.456), Is.EqualTo(0.0).Within(Delta));
    }

    [Test]
    public void TestLongitudeAfterStart()
    {
        // 30° after the start factor is 0° of the second sign (Taurus).
        Assert.That(AltZodiacStartOrchestrator.ShiftedLongitude(130.0, 100.0), Is.EqualTo(30.0).Within(Delta));
    }

    [Test]
    public void TestLongitudeBeforeStartWrapsAround()
    {
        Assert.That(AltZodiacStartOrchestrator.ShiftedLongitude(10.0, 100.0), Is.EqualTo(270.0).Within(Delta));
    }

    [Test]
    public void TestWrapAcrossZeroAries()
    {
        Assert.That(AltZodiacStartOrchestrator.ShiftedLongitude(5.0, 350.0), Is.EqualTo(15.0).Within(Delta));
    }

    [Test]
    public void TestStartAtZeroAriesKeepsLongitude()
    {
        Assert.That(AltZodiacStartOrchestrator.ShiftedLongitude(245.5, 0.0), Is.EqualTo(245.5).Within(Delta));
    }

    [Test]
    public void TestWheelPositionTextsUseAlternativeZodiac()
    {
        // Sun at 15°20' Taurus (45.333..°), zodiac starting at 10°00' Aries: new position 5°20' of the second sign.
        var sun  = new WheelPlotItem(Factors.Sun, "a", 45.0 + 20.0 / 60.0, 100.0, 101.0, "15°20'", SpeedType.Direct);
        var mars = new WheelPlotItem(Factors.Mars, "b", 200.5, 250.0, 250.0, "20°30' R", SpeedType.Retrograde);
        var data = WheelPlotData.Empty with { PlanetItems = [sun, mars] };

        var shifted = AltZodiacStartOrchestrator.WithShiftedPositionTexts(data, 10.0);

        Assert.That(shifted.PlanetItems[0].PositionText, Is.EqualTo("5°20'"));
        Assert.That(shifted.PlanetItems[1].PositionText, Is.EqualTo($"10°30' {SpeedType.Retrograde.Abbreviation()}"));
        // Placement in the wheel does not depend on the zodiac's start.
        Assert.That(shifted.PlanetItems[0].EclipticLongitude, Is.EqualTo(sun.EclipticLongitude));
        Assert.That(shifted.PlanetItems[0].PlotAngle, Is.EqualTo(sun.PlotAngle));
    }

    [TestCase(0.0, 0.0)]
    [TestCase(359.999, 0.001)]
    [TestCase(180.0, 179.5)]
    public void TestResultIsAlwaysInRange(double longitude, double start)
    {
        var result = AltZodiacStartOrchestrator.ShiftedLongitude(longitude, start);
        Assert.That(result, Is.GreaterThanOrEqualTo(0.0).And.LessThan(360.0));
    }
}
