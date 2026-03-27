// DrawSignsTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWintest.Features.ChartDrawing.WheelDrawing;

/// <summary>Tests for DrawSigns helper methods (non-drawing, pure logic).</summary>
[TestFixture]
public class DrawSignsTests
{
    private const double Accuracy = 1e-10;

    // MARK: - SignOffsetAsc

    [Test]
    public void TestSignOffsetAscAt0Returns30()
    {
        // Ascendant op 0Â° â†’ offset 30Â°
        Assert.That(DrawSigns.SignOffsetAsc(0.0), Is.EqualTo(30.0));
    }

    [Test]
    public void TestSignOffsetAscAt15Returns15()
    {
        // Ascendant halverwege teken (15Â°) â†’ offset 15Â°
        Assert.That(DrawSigns.SignOffsetAsc(15.0), Is.EqualTo(15.0));
    }

    [Test]
    public void TestSignOffsetAscAt30Returns30()
    {
        // Ascendant precies op tekengrens (30Â°) â†’ 30 % 30 = 0 â†’ 30 - 0 = 30
        Assert.That(DrawSigns.SignOffsetAsc(30.0), Is.EqualTo(30.0));
    }

    [Test]
    public void TestSignOffsetAscAt29Returns1()
    {
        // Ascendant op 29Â° â†’ offset 1Â°
        Assert.That(DrawSigns.SignOffsetAsc(29.0), Is.EqualTo(1.0));
    }

    [Test]
    public void TestSignOffsetAscAt60Returns30()
    {
        // Ascendant op tekengrens Tweeling (60Â°) â†’ 60 % 30 = 0 â†’ offset 30Â°
        Assert.That(DrawSigns.SignOffsetAsc(60.0), Is.EqualTo(30.0));
    }

    [Test]
    public void TestSignOffsetAscAt359Returns1()
    {
        // Ascendant op 359Â° (359 % 30 = 29) â†’ offset 1Â°
        Assert.That(DrawSigns.SignOffsetAsc(359.0), Is.EqualTo(1.0));
    }

    [Test]
    public void TestSignOffsetAscAt45Returns15()
    {
        // Ascendant halverwege Stier (45Â°) â†’ 45 % 30 = 15 â†’ 30 - 15 = 15Â°
        Assert.That(DrawSigns.SignOffsetAsc(45.0), Is.EqualTo(15.0));
    }
}
