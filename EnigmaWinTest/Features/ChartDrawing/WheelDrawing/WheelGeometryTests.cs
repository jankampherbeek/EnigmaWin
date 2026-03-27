// WheelGeometryTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using Avalonia;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWintest.Features.ChartDrawing.WheelDrawing;

/// <summary>Tests for WheelGeometry calculations.</summary>
[TestFixture]
public class WheelGeometryTests
{
    private const double Accuracy = 1e-10;

    // MARK: - PointOnCircle

    [Test]
    public void TestPointOnCircleAngle0TopOfCenter()
    {
        // Hoek 0Â° (top, 12 o'clock) staat recht boven het middelpunt
        var center = new Point(100, 100);
        var p = WheelGeometry.PointOnCircle(0, 100, center);
        Assert.That(Math.Abs(p.X - 100.0), Is.LessThan(Accuracy));
        Assert.That(Math.Abs(p.Y - 0.0),   Is.LessThan(Accuracy));
    }

    [Test]
    public void TestPointOnCircleAngle90LeftOfCenter()
    {
        // Hoek 90Â° staat links van het middelpunt (wheel-conventie: kloksgewijs)
        var center = new Point(0, 0);
        var p = WheelGeometry.PointOnCircle(90, 100, center);
        Assert.That(Math.Abs(p.X - (-100.0)), Is.LessThan(Accuracy));
        Assert.That(Math.Abs(p.Y - 0.0),      Is.LessThan(1e-6));
    }

    [Test]
    public void TestPointOnCircleAngle180BelowCenter()
    {
        // Hoek 180Â° staat recht onder het middelpunt
        var center = new Point(0, 0);
        var p = WheelGeometry.PointOnCircle(180, 100, center);
        Assert.That(Math.Abs(p.X - 0.0),   Is.LessThan(1e-6));
        Assert.That(Math.Abs(p.Y - 100.0), Is.LessThan(Accuracy));
    }

    [Test]
    public void TestPointOnCircleAngle270RightOfCenter()
    {
        // Hoek 270Â° staat rechts van het middelpunt
        var center = new Point(0, 0);
        var p = WheelGeometry.PointOnCircle(270, 100, center);
        Assert.That(Math.Abs(p.X - 100.0), Is.LessThan(Accuracy));
        Assert.That(Math.Abs(p.Y - 0.0),   Is.LessThan(1e-6));
    }

    [Test]
    public void TestPointOnCircleRadiusZeroReturnsCenter()
    {
        // Straal 0 geeft altijd het middelpunt terug
        var center = new Point(50, 75);
        var p = WheelGeometry.PointOnCircle(45, 0, center);
        Assert.That(Math.Abs(p.X - 50.0), Is.LessThan(Accuracy));
        Assert.That(Math.Abs(p.Y - 75.0), Is.LessThan(Accuracy));
    }

    [Test]
    public void TestPointOnCircleNegativeAngleEquivalentTo270()
    {
        // Negatieve hoek (-90Â°) is equivalent aan 270Â°
        var center = new Point(0, 0);
        var pNeg = WheelGeometry.PointOnCircle(-90, 100, center);
        var p270 = WheelGeometry.PointOnCircle(270, 100, center);
        Assert.That(Math.Abs(pNeg.X - p270.X), Is.LessThan(Accuracy));
        Assert.That(Math.Abs(pNeg.Y - p270.Y), Is.LessThan(Accuracy));
    }

    // MARK: - MundaneAngle

    [Test]
    public void TestMundaneAngleAscendantAlwaysAt90()
    {
        // Ascendant komt altijd op 90Â°
        var asc    = 45.0;
        var result = WheelGeometry.MundaneAngle(asc, asc);
        Assert.That(result, Is.EqualTo(90.0));
    }

    [Test]
    public void TestMundaneAngleMCAtTop()
    {
        // MC (asc - 90Â°) komt op 0Â° (top)
        var asc    = 90.0;
        var mc     = 0.0;
        var result = WheelGeometry.MundaneAngle(mc, asc);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestMundaneAngleDSCAt270()
    {
        // DSC (asc + 180Â°) komt op 270Â°
        var asc    = 0.0;
        var dsc    = 180.0;
        var result = WheelGeometry.MundaneAngle(dsc, asc);
        Assert.That(result, Is.EqualTo(270.0));
    }

    [Test]
    public void TestMundaneAngleNegativeIntermediateWrapsTo270()
    {
        // longitude=0, asc=180 â†’ 0 - 180 + 90 = -90 â†’ moet 270 worden
        var result = WheelGeometry.MundaneAngle(0, 180);
        Assert.That(result, Is.EqualTo(270.0));
    }

    [Test]
    public void TestMundaneAngleAbove360WrapsDown()
    {
        // longitude=350, asc=10 â†’ 350 - 10 + 90 = 430 â†’ 70
        var result = WheelGeometry.MundaneAngle(350, 10);
        Assert.That(result, Is.EqualTo(70.0));
    }

    // MARK: - SignOffset

    [Test]
    public void TestSignOffsetAt0Returns30()
    {
        // Ascendant op 0Â° â†’ offset is 30Â°
        var result = WheelGeometry.SignOffset(0);
        Assert.That(result, Is.EqualTo(30.0));
    }

    [Test]
    public void TestSignOffsetAt15Returns15()
    {
        // Ascendant halverwege teken (15Â°) â†’ offset is 15Â°
        var result = WheelGeometry.SignOffset(15);
        Assert.That(result, Is.EqualTo(15.0));
    }

    [Test]
    public void TestSignOffsetAt30Returns30()
    {
        // Ascendant precies op tekengrens (30Â°) â†’ offset is 30Â°
        var result = WheelGeometry.SignOffset(30);
        Assert.That(result, Is.EqualTo(30.0));
    }

    [Test]
    public void TestSignOffsetAt29Returns1()
    {
        // Ascendant op 29Â° â†’ offset is 1Â°
        var result = WheelGeometry.SignOffset(29);
        Assert.That(result, Is.EqualTo(1.0));
    }

    [Test]
    public void TestSignOffsetAt359Returns1()
    {
        // Ascendant op 359Â° (359 % 30 = 29) â†’ offset is 1Â°
        var result = WheelGeometry.SignOffset(359);
        Assert.That(result, Is.EqualTo(1.0));
    }

    // MARK: - Normalise

    [Test]
    public void TestNormaliseInRangeUnchanged()
    {
        // Waarden in [0, 360) blijven ongewijzigd
        Assert.That(WheelGeometry.Normalise(90.0),  Is.EqualTo(90.0));
        Assert.That(WheelGeometry.Normalise(0.0),   Is.EqualTo(0.0));
        Assert.That(WheelGeometry.Normalise(359.0), Is.EqualTo(359.0));
    }

    [Test]
    public void TestNormalise360BecomesZero()
    {
        Assert.That(WheelGeometry.Normalise(360.0), Is.EqualTo(0.0));
    }

    [Test]
    public void TestNormaliseAbove360WrapsDown()
    {
        Assert.That(WheelGeometry.Normalise(450.0), Is.EqualTo(90.0));
        Assert.That(WheelGeometry.Normalise(720.0), Is.EqualTo(0.0));
    }

    [Test]
    public void TestNormaliseNegativeWrapsUp()
    {
        Assert.That(WheelGeometry.Normalise(-90.0),  Is.EqualTo(270.0));
        Assert.That(WheelGeometry.Normalise(-1.0),   Is.EqualTo(359.0));
        Assert.That(WheelGeometry.Normalise(-360.0), Is.EqualTo(0.0));
    }
}
