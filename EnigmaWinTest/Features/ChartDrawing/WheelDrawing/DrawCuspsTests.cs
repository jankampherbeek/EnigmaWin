// DrawCuspsTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

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
        Assert.That(result, Is.EqualTo("0Â°00'"));
    }

    [Test]
    public void TestCuspPositionText15_30()
    {
        // 15.5Â° Ã— 60 = 930 totaal minuten. 930/60 = 15 graden, 930%60 = 30 minuten
        var result = DrawCusps.CuspPositionText(15.5);
        Assert.That(result, Is.EqualTo("15Â°30'"));
    }

    [Test]
    public void TestCuspPositionTextSecondSign()
    {
        // 45.5 % 30 = 15.5 â†’ zelfde als longitude 15.5
        var result = DrawCusps.CuspPositionText(45.5);
        Assert.That(result, Is.EqualTo("15Â°30'"));
    }

    [Test]
    public void TestCuspPositionText29()
    {
        var result = DrawCusps.CuspPositionText(29.0);
        Assert.That(result, Is.EqualTo("29Â°00'"));
    }

    [Test]
    public void TestCuspPositionTextOneMinute()
    {
        // 1/60 graden = 1 minuut â†’ "0Â°01'"
        var result = DrawCusps.CuspPositionText(1.0 / 60.0);
        Assert.That(result, Is.EqualTo("0Â°01'"));
    }

    [Test]
    public void TestCuspPositionTextMinutesZeroPadded()
    {
        // 10 + 5/60 â‰ˆ 10.0833... â†’ inSign = 10.0833, totalMin = Int(10.0833 * 60) = 605 â†’ "10Â°05'"
        var result = DrawCusps.CuspPositionText(10.0 + 5.0 / 60.0);
        Assert.That(result, Is.EqualTo("10Â°05'"));
    }
}
