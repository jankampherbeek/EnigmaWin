// RangeUtilTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 24-01-2026

using System;
using EnigmaWin.Sources.Features.Shared.Conversion;

namespace EnigmaWintest.Features.Shared.Conversion;

/// <summary>Tests for RangeUtil utilities.</summary>
[TestFixture]
public class RangeUtilTests
{
    // MARK: - 0-360 Range Tests (Common for longitude normalization)

    [Test]
    public void TestValueToRangeAlreadyInRange()
    {
        var result = RangeUtil.ValueToRange(180.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(180.0));
    }

    [Test]
    public void TestValueToRangeAtLowerBoundary()
    {
        var result = RangeUtil.ValueToRange(0.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestValueToRangeJustBelowUpperBoundary()
    {
        var result = RangeUtil.ValueToRange(359.9, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(359.9));
    }

    [Test]
    public void TestValueToRangeAtUpperBoundary()
    {
        var result = RangeUtil.ValueToRange(360.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestValueToRangeAboveUpperBoundary()
    {
        var result = RangeUtil.ValueToRange(370.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(10.0));
    }

    [Test]
    public void TestValueToRangeWellAboveUpperBoundary()
    {
        var result = RangeUtil.ValueToRange(720.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestValueToRangeVeryAboveUpperBoundary()
    {
        var result = RangeUtil.ValueToRange(1080.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestValueToRangeBelowLowerBoundary()
    {
        var result = RangeUtil.ValueToRange(-10.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(350.0));
    }

    [Test]
    public void TestValueToRangeWellBelowLowerBoundary()
    {
        var result = RangeUtil.ValueToRange(-370.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(350.0));
    }

    [Test]
    public void TestValueToRangeVeryBelowLowerBoundary()
    {
        var result = RangeUtil.ValueToRange(-720.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    // MARK: - -180 to 180 Range Tests (Common for angle differences)

    [Test]
    public void TestValueToRangeNegativeRangeAlreadyInRange()
    {
        var result = RangeUtil.ValueToRange(90.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(90.0));
    }

    [Test]
    public void TestValueToRangeNegativeValueInRange()
    {
        var result = RangeUtil.ValueToRange(-90.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(-90.0));
    }

    [Test]
    public void TestValueToRangeAtNegativeLowerBoundary()
    {
        var result = RangeUtil.ValueToRange(-180.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(-180.0));
    }

    [Test]
    public void TestValueToRangeJustBelowNegativeUpperBoundary()
    {
        var result = RangeUtil.ValueToRange(179.9, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(179.9).Within(0.0001));
    }

    [Test]
    public void TestValueToRangeAtNegativeUpperBoundary()
    {
        var result = RangeUtil.ValueToRange(180.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(-180.0));
    }

    [Test]
    public void TestValueToRangeAboveNegativeUpperBoundary()
    {
        var result = RangeUtil.ValueToRange(190.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(-170.0));
    }

    [Test]
    public void TestValueToRangeWellAboveNegativeUpperBoundary()
    {
        var result = RangeUtil.ValueToRange(360.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestValueToRangeBelowNegativeLowerBoundary()
    {
        var result = RangeUtil.ValueToRange(-190.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(170.0));
    }

    [Test]
    public void TestValueToRangeWellBelowNegativeLowerBoundary()
    {
        var result = RangeUtil.ValueToRange(-360.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    // MARK: - Other Range Tests

    [Test]
    public void TestValueToRangeSmallRange()
    {
        var result = RangeUtil.ValueToRange(15.0, 0.0, 10.0);
        Assert.That(result, Is.EqualTo(5.0));
    }

    [Test]
    public void TestValueToRangeInSmallRange()
    {
        var result = RangeUtil.ValueToRange(5.0, 0.0, 10.0);
        Assert.That(result, Is.EqualTo(5.0));
    }

    [Test]
    public void TestValueToRangeNegativeSmallRange()
    {
        var result = RangeUtil.ValueToRange(15.0, -10.0, 10.0);
        Assert.That(result, Is.EqualTo(-5.0));
    }

    [Test]
    public void TestValueToRangeInNegativeSmallRange()
    {
        var result = RangeUtil.ValueToRange(-5.0, -10.0, 10.0);
        Assert.That(result, Is.EqualTo(-5.0));
    }

    [Test]
    public void TestValueToRangeFractionalRange()
    {
        var result = RangeUtil.ValueToRange(1.5, 0.0, 1.0);
        Assert.That(result, Is.EqualTo(0.5));
    }

    [Test]
    public void TestValueToRangeInFractionalRange()
    {
        var result = RangeUtil.ValueToRange(0.5, 0.0, 1.0);
        Assert.That(result, Is.EqualTo(0.5));
    }

    // MARK: - Edge Cases

    [Test]
    public void TestValueToRangeZero()
    {
        var result = RangeUtil.ValueToRange(0.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestValueToRangeVerySmallPositive()
    {
        var result = RangeUtil.ValueToRange(0.0001, 0.0, 360.0);
        var difference = Math.Abs(result - 0.0001);
        Assert.That(difference, Is.LessThan(1e-10), $"Expected 0.0001, got {result}, difference: {difference}");
    }

    [Test]
    public void TestValueToRangeVerySmallNegative()
    {
        var result = RangeUtil.ValueToRange(-0.0001, 0.0, 360.0);
        var difference = Math.Abs(result - 359.9999);
        Assert.That(difference, Is.LessThan(1e-10), $"Expected ~359.9999, got {result}, difference: {difference}");
    }

    [Test]
    public void TestValueToRangeLargePositive()
    {
        var result = RangeUtil.ValueToRange(1000.0, 0.0, 360.0);
        // 1000 % 360 = 280
        Assert.That(result, Is.EqualTo(280.0));
    }

    [Test]
    public void TestValueToRangeLargeNegative()
    {
        var result = RangeUtil.ValueToRange(-1000.0, 0.0, 360.0);
        // -1000 + 3*360 = -1000 + 1080 = 80
        Assert.That(result, Is.EqualTo(80.0));
    }

    // MARK: - Real-world Use Cases

    [Test]
    public void TestValueToRangeLongitude450()
    {
        var result = RangeUtil.ValueToRange(450.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(90.0));
    }

    [Test]
    public void TestValueToRangeLongitudeNegative45()
    {
        var result = RangeUtil.ValueToRange(-45.0, 0.0, 360.0);
        Assert.That(result, Is.EqualTo(315.0));
    }

    [Test]
    public void TestValueToRangeAngleDifference200()
    {
        var result = RangeUtil.ValueToRange(200.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(-160.0));
    }

    [Test]
    public void TestValueToRangeAngleDifferenceNegative200()
    {
        var result = RangeUtil.ValueToRange(-200.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(160.0));
    }

    [Test]
    public void TestValueToRangeAngleDifferenceZero()
    {
        var result = RangeUtil.ValueToRange(0.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestValueToRangeAngleDifference180()
    {
        var result = RangeUtil.ValueToRange(180.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(-180.0));
    }

    [Test]
    public void TestValueToRangeAngleDifferenceNegative180()
    {
        var result = RangeUtil.ValueToRange(-180.0, -180.0, 180.0);
        Assert.That(result, Is.EqualTo(-180.0));
    }
}
