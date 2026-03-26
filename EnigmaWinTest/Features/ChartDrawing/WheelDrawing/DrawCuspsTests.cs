// DrawCuspsTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWintest.Features.ChartDrawing.WheelDrawing;

/// <summary>Tests for DrawCusps helper methods (non-drawing, pure logic).</summary>
[TestFixture]
public class DrawCuspsTests
{
    private const double Accuracy = 1e-10;

    // MARK: - CuspPositionText

    [Test]
    public void TestCuspPositionTextZero()
    {
        var result = DrawCusps.CuspPositionText(0.0);
        Assert.That(result, Is.EqualTo("0°00'"));
    }

    [Test]
    public void TestCuspPositionText15_30()
    {
        // 15.5° × 60 = 930 totaal minuten. 930/60 = 15 graden, 930%60 = 30 minuten
        var result = DrawCusps.CuspPositionText(15.5);
        Assert.That(result, Is.EqualTo("15°30'"));
    }

    [Test]
    public void TestCuspPositionTextSecondSign()
    {
        // 45.5 % 30 = 15.5 → zelfde als longitude 15.5
        var result = DrawCusps.CuspPositionText(45.5);
        Assert.That(result, Is.EqualTo("15°30'"));
    }

    [Test]
    public void TestCuspPositionText29()
    {
        var result = DrawCusps.CuspPositionText(29.0);
        Assert.That(result, Is.EqualTo("29°00'"));
    }

    [Test]
    public void TestCuspPositionTextOneMinute()
    {
        // 1/60 graden = 1 minuut → "0°01'"
        var result = DrawCusps.CuspPositionText(1.0 / 60.0);
        Assert.That(result, Is.EqualTo("0°01'"));
    }

    [Test]
    public void TestCuspPositionTextMinutesZeroPadded()
    {
        // 10 + 5/60 ≈ 10.0833... → inSign = 10.0833, totalMin = Int(10.0833 * 60) = 605 → "10°05'"
        var result = DrawCusps.CuspPositionText(10.0 + 5.0 / 60.0);
        Assert.That(result, Is.EqualTo("10°05'"));
    }
}
