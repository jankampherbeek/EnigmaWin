// DrawSignsTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

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
        // Ascendant op 0° → offset 30°
        Assert.That(DrawSigns.SignOffsetAsc(0.0), Is.EqualTo(30.0));
    }

    [Test]
    public void TestSignOffsetAscAt15Returns15()
    {
        // Ascendant halverwege teken (15°) → offset 15°
        Assert.That(DrawSigns.SignOffsetAsc(15.0), Is.EqualTo(15.0));
    }

    [Test]
    public void TestSignOffsetAscAt30Returns30()
    {
        // Ascendant precies op tekengrens (30°) → 30 % 30 = 0 → 30 - 0 = 30
        Assert.That(DrawSigns.SignOffsetAsc(30.0), Is.EqualTo(30.0));
    }

    [Test]
    public void TestSignOffsetAscAt29Returns1()
    {
        // Ascendant op 29° → offset 1°
        Assert.That(DrawSigns.SignOffsetAsc(29.0), Is.EqualTo(1.0));
    }

    [Test]
    public void TestSignOffsetAscAt60Returns30()
    {
        // Ascendant op tekengrens Tweeling (60°) → 60 % 30 = 0 → offset 30°
        Assert.That(DrawSigns.SignOffsetAsc(60.0), Is.EqualTo(30.0));
    }

    [Test]
    public void TestSignOffsetAscAt359Returns1()
    {
        // Ascendant op 359° (359 % 30 = 29) → offset 1°
        Assert.That(DrawSigns.SignOffsetAsc(359.0), Is.EqualTo(1.0));
    }

    [Test]
    public void TestSignOffsetAscAt45Returns15()
    {
        // Ascendant halverwege Stier (45°) → 45 % 30 = 15 → 30 - 15 = 15°
        Assert.That(DrawSigns.SignOffsetAsc(45.0), Is.EqualTo(15.0));
    }
}
