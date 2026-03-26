// WheelMetricsTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using System;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWintest.Features.ChartDrawing.WheelDrawing;

/// <summary>Tests for WheelMetrics computed values.</summary>
[TestFixture]
public class WheelMetricsTests
{
    private const double Accuracy = 1e-10;

    // MARK: - Radius

    [Test]
    public void TestRadiusHalfFraction()
    {
        // Fractie 0.5 van 200 geeft 100
        var result = WheelMetrics.Radius(0.5, 200);
        Assert.That(Math.Abs(result - 100.0), Is.LessThan(Accuracy));
    }

    [Test]
    public void TestRadiusFullFraction()
    {
        // Fractie 1.0 geeft de volledige outerRadius terug
        var result = WheelMetrics.Radius(1.0, 350);
        Assert.That(Math.Abs(result - 350.0), Is.LessThan(Accuracy));
    }

    [Test]
    public void TestRadiusZeroFractionGivesZero()
    {
        // Fractie 0.0 geeft altijd 0
        var result = WheelMetrics.Radius(0.0, 350);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestRadiusZeroOuterGivesZero()
    {
        // outerRadius 0 geeft altijd 0
        var result = WheelMetrics.Radius(0.98, 0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestRadiusOuterCircleFractionAt350()
    {
        // OuterCircle = 0.98, bij 350px → 343
        var result = WheelMetrics.Radius(WheelMetrics.OuterCircle, 350);
        Assert.That(Math.Abs(result - 343.0), Is.LessThan(Accuracy));
    }

    // MARK: - FontSize

    [Test]
    public void TestFontSizeSignGlyphAt350()
    {
        // SignGlyphFontFraction = 0.080, 0.080 * 350 = 28.0
        var result = WheelMetrics.FontSize(WheelMetrics.SignGlyphFontFraction, 350);
        Assert.That(Math.Abs(result - 28.0), Is.LessThan(Accuracy));
    }

    [Test]
    public void TestFontSizeZeroFractionGivesZero()
    {
        var result = WheelMetrics.FontSize(0.0, 350);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestFontSizeHalfFraction()
    {
        // Fractie 0.5 van 200 geeft 100
        var result = WheelMetrics.FontSize(0.5, 200);
        Assert.That(Math.Abs(result - 100.0), Is.LessThan(Accuracy));
    }

    // MARK: - StrokeWidth

    [Test]
    public void TestStrokeWidthStrokeFractionAt350()
    {
        // StrokeFraction = 0.0057, 0.0057 * 350 = 1.995
        var result = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, 350);
        Assert.That(Math.Abs(result - 1.995), Is.LessThan(Accuracy));
    }

    [Test]
    public void TestStrokeWidthZeroFractionGivesZero()
    {
        var result = WheelMetrics.StrokeWidth(0.0, 350);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestStrokeWidthFullFraction()
    {
        // Fractie 1.0 geeft de volledige outerRadius als breedte
        var result = WheelMetrics.StrokeWidth(1.0, 100);
        Assert.That(Math.Abs(result - 100.0), Is.LessThan(Accuracy));
    }
}
