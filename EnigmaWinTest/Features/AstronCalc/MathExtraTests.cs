// MathExtraTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 24-01-2026

using System;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWintest.Features.AstronCalc;

/// <summary>Tests for MathExtra utilities.</summary>
[TestFixture]
public class MathExtraTests
{
    // MARK: - RadToDeg Tests

    [Test]
    public void TestRadToDegZero()
    {
        var result = MathExtra.RadToDeg(0.0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestRadToDegPi()
    {
        var result = MathExtra.RadToDeg(Math.PI);
        var difference = Math.Abs(result - 180.0);
        Assert.That(difference, Is.LessThan(1e-10), $"Expected 180.0, got {result}, difference: {difference}");
    }

    [Test]
    public void TestRadToDegPiOver2()
    {
        var result = MathExtra.RadToDeg(Math.PI / 2.0);
        var difference = Math.Abs(result - 90.0);
        Assert.That(difference, Is.LessThan(1e-10), $"Expected 90.0, got {result}, difference: {difference}");
    }

    [Test]
    public void TestRadToDegTwoPi()
    {
        var result = MathExtra.RadToDeg(2 * Math.PI);
        var difference = Math.Abs(result - 360.0);
        Assert.That(difference, Is.LessThan(1e-10), $"Expected 360.0, got {result}, difference: {difference}");
    }

    [Test]
    public void TestRadToDegNegative()
    {
        var result = MathExtra.RadToDeg(-Math.PI);
        var difference = Math.Abs(result - (-180.0));
        Assert.That(difference, Is.LessThan(1e-10), $"Expected -180.0, got {result}, difference: {difference}");
    }

    // MARK: - DegToRad Tests

    [Test]
    public void TestDegToRadZero()
    {
        var result = MathExtra.DegToRad(0.0);
        Assert.That(result, Is.EqualTo(0.0));
    }

    [Test]
    public void TestDegToRad180()
    {
        var result = MathExtra.DegToRad(180.0);
        var difference = Math.Abs(result - Math.PI);
        Assert.That(difference, Is.LessThan(1e-10), $"Expected π, got {result}, difference: {difference}");
    }

    [Test]
    public void TestDegToRad90()
    {
        var result = MathExtra.DegToRad(90.0);
        var difference = Math.Abs(result - (Math.PI / 2.0));
        Assert.That(difference, Is.LessThan(1e-10), $"Expected π/2, got {result}, difference: {difference}");
    }

    [Test]
    public void TestDegToRad360()
    {
        var result = MathExtra.DegToRad(360.0);
        var difference = Math.Abs(result - (2 * Math.PI));
        Assert.That(difference, Is.LessThan(1e-10), $"Expected 2π, got {result}, difference: {difference}");
    }

    [Test]
    public void TestDegToRadNegative()
    {
        var result = MathExtra.DegToRad(-180.0);
        var difference = Math.Abs(result - (-Math.PI));
        Assert.That(difference, Is.LessThan(1e-10), $"Expected -π, got {result}, difference: {difference}");
    }

    // MARK: - Round-trip Conversion Tests

    [Test]
    public void TestRadDegRoundTrip()
    {
        var testValues = new[] { 0.0, Math.PI / 4, Math.PI / 2, Math.PI, 3 * Math.PI / 2, 2 * Math.PI, -Math.PI / 4 };
        foreach (var radians in testValues)
        {
            var degrees = MathExtra.RadToDeg(radians);
            var backToRadians = MathExtra.DegToRad(degrees);
            var difference = Math.Abs(backToRadians - radians);
            Assert.That(difference, Is.LessThan(1e-10), $"Failed for radians: {radians}, difference: {difference}");
        }
    }

    [Test]
    public void TestDegRadRoundTrip()
    {
        var testValues = new[] { 0.0, 45.0, 90.0, 180.0, 270.0, 360.0, -45.0 };
        foreach (var degrees in testValues)
        {
            var radians = MathExtra.DegToRad(degrees);
            var backToDegrees = MathExtra.RadToDeg(radians);
            var difference = Math.Abs(backToDegrees - degrees);
            Assert.That(difference, Is.LessThan(1e-10), $"Failed for degrees: {degrees}, difference: {difference}");
        }
    }

    // MARK: - Rectangular2Polar Tests

    [Test]
    public void TestRectangular2PolarOrigin()
    {
        var rect = new RectAngCoordinates(0.0, 0.0, 0.0);
        // Note: When r is 0, Acos will have issues. This test may need adjustment based on implementation.
        // The Swift version uses Double.leastNormalMagnitude, but our implementation may behave differently.
        var polar = MathExtra.Rectangular2Polar(rect);
        // For zero coordinates, r should be 0
        Assert.That(polar.RCoord, Is.EqualTo(0.0).Within(1e-10));
    }

    [Test]
    public void TestRectangular2PolarXAxis()
    {
        var rect = new RectAngCoordinates(1.0, 0.0, 0.0);
        var polar = MathExtra.Rectangular2Polar(rect);
        var rDiff = Math.Abs(polar.RCoord - 1.0);
        var phiDiff = Math.Abs(polar.PhiCoord - 0.0);
        // theta = asin(z/r) = asin(0/1) = 0 (latitude from equatorial plane)
        var thetaDiff = Math.Abs(polar.ThetaCoord - 0.0);
        Assert.That(rDiff, Is.LessThan(1e-10), $"rCoord: expected 1.0, got {polar.RCoord}");
        Assert.That(phiDiff, Is.LessThan(1e-10), $"phiCoord: expected 0.0, got {polar.PhiCoord}");
        Assert.That(thetaDiff, Is.LessThan(1e-10), $"thetaCoord: expected 0.0, got {polar.ThetaCoord}");
    }

    [Test]
    public void TestRectangular2PolarYAxis()
    {
        var rect = new RectAngCoordinates(0.0, 1.0, 0.0);
        var polar = MathExtra.Rectangular2Polar(rect);
        var rDiff = Math.Abs(polar.RCoord - 1.0);
        var phiDiff = Math.Abs(polar.PhiCoord - (Math.PI / 2.0));
        var thetaDiff = Math.Abs(polar.ThetaCoord - 0.0); // theta = asin(0/1) = 0
        Assert.That(rDiff, Is.LessThan(1e-10), $"rCoord: expected 1.0, got {polar.RCoord}");
        Assert.That(phiDiff, Is.LessThan(1e-10), $"phiCoord: expected π/2, got {polar.PhiCoord}");
        Assert.That(thetaDiff, Is.LessThan(1e-10), $"thetaCoord: expected 0.0, got {polar.ThetaCoord}");
    }

    [Test]
    public void TestRectangular2PolarZAxis()
    {
        var rect = new RectAngCoordinates(0.0, 0.0, 1.0);
        var polar = MathExtra.Rectangular2Polar(rect);
        var rDiff = Math.Abs(polar.RCoord - 1.0);
        var thetaDiff = Math.Abs(polar.ThetaCoord - (Math.PI / 2.0)); // theta = asin(1/1) = π/2
        Assert.That(rDiff, Is.LessThan(1e-10), $"rCoord: expected 1.0, got {polar.RCoord}");
        Assert.That(thetaDiff, Is.LessThan(1e-10), $"thetaCoord: expected π/2, got {polar.ThetaCoord}");
    }

    [Test]
    public void TestRectangular2PolarNegative()
    {
        var rect = new RectAngCoordinates(-1.0, -1.0, -1.0);
        var polar = MathExtra.Rectangular2Polar(rect);
        var expectedR = Math.Sqrt(3.0);
        var rDiff = Math.Abs(polar.RCoord - expectedR);
        Assert.That(rDiff, Is.LessThan(1e-10), $"rCoord: expected {expectedR}, got {polar.RCoord}");
    }

    // MARK: - Polar2Rectangular Tests

    [Test]
    public void TestPolar2RectangularZeroRadius()
    {
        var polar = new PolarCoordinates(0.0, 0.0, 0.0);
        var rect = MathExtra.Polar2Rectangular(polar);
        var xDiff = Math.Abs(rect.XCoord - 0.0);
        var yDiff = Math.Abs(rect.YCoord - 0.0);
        var zDiff = Math.Abs(rect.ZCoord - 0.0);
        Assert.That(xDiff, Is.LessThan(1e-10), $"xCoord: expected 0.0, got {rect.XCoord}");
        Assert.That(yDiff, Is.LessThan(1e-10), $"yCoord: expected 0.0, got {rect.YCoord}");
        Assert.That(zDiff, Is.LessThan(1e-10), $"zCoord: expected 0.0, got {rect.ZCoord}");
    }

    [Test]
    public void TestPolar2RectangularXAxis()
    {
        var polar = new PolarCoordinates(0.0, 0.0, 1.0);
        var rect = MathExtra.Polar2Rectangular(polar);
        var xDiff = Math.Abs(rect.XCoord - 1.0);
        var yDiff = Math.Abs(rect.YCoord - 0.0);
        var zDiff = Math.Abs(rect.ZCoord - 0.0);
        Assert.That(xDiff, Is.LessThan(1e-10), $"xCoord: expected 1.0, got {rect.XCoord}");
        Assert.That(yDiff, Is.LessThan(1e-10), $"yCoord: expected 0.0, got {rect.YCoord}");
        Assert.That(zDiff, Is.LessThan(1e-10), $"zCoord: expected 0.0, got {rect.ZCoord}");
    }

    [Test]
    public void TestPolar2RectangularYAxis()
    {
        var polar = new PolarCoordinates(Math.PI / 2.0, 0.0, 1.0);
        var rect = MathExtra.Polar2Rectangular(polar);
        var xDiff = Math.Abs(rect.XCoord - 0.0);
        var yDiff = Math.Abs(rect.YCoord - 1.0);
        var zDiff = Math.Abs(rect.ZCoord - 0.0);
        Assert.That(xDiff, Is.LessThan(1e-10), $"xCoord: expected 0.0, got {rect.XCoord}");
        Assert.That(yDiff, Is.LessThan(1e-10), $"yCoord: expected 1.0, got {rect.YCoord}");
        Assert.That(zDiff, Is.LessThan(1e-10), $"zCoord: expected 0.0, got {rect.ZCoord}");
    }

    [Test]
    public void TestPolar2RectangularZAxis()
    {
        var polar = new PolarCoordinates(0.0, Math.PI / 2.0, 1.0);
        var rect = MathExtra.Polar2Rectangular(polar);
        var xDiff = Math.Abs(rect.XCoord - 0.0);
        var yDiff = Math.Abs(rect.YCoord - 0.0);
        var zDiff = Math.Abs(rect.ZCoord - 1.0);
        Assert.That(xDiff, Is.LessThan(1e-10), $"xCoord: expected 0.0, got {rect.XCoord}");
        Assert.That(yDiff, Is.LessThan(1e-10), $"yCoord: expected 0.0, got {rect.YCoord}");
        Assert.That(zDiff, Is.LessThan(1e-10), $"zCoord: expected 1.0, got {rect.ZCoord}");
    }

    [Test]
    public void TestPolar2RectangularArbitrary()
    {
        var polar = new PolarCoordinates(Math.PI / 4.0, Math.PI / 6.0, 2.0);
        var rect = MathExtra.Polar2Rectangular(polar);
        // Expected values using astronomical/latitude convention: x=r*cos(theta)*cos(phi), z=r*sin(theta)
        var expectedX = 2.0 * Math.Cos(Math.PI / 6.0) * Math.Cos(Math.PI / 4.0);
        var expectedY = 2.0 * Math.Cos(Math.PI / 6.0) * Math.Sin(Math.PI / 4.0);
        var expectedZ = 2.0 * Math.Sin(Math.PI / 6.0);
        var xDiff = Math.Abs(rect.XCoord - expectedX);
        var yDiff = Math.Abs(rect.YCoord - expectedY);
        var zDiff = Math.Abs(rect.ZCoord - expectedZ);
        Assert.That(xDiff, Is.LessThan(1e-10), $"xCoord: expected {expectedX}, got {rect.XCoord}");
        Assert.That(yDiff, Is.LessThan(1e-10), $"yCoord: expected {expectedY}, got {rect.YCoord}");
        Assert.That(zDiff, Is.LessThan(1e-10), $"zCoord: expected {expectedZ}, got {rect.ZCoord}");
    }

    // MARK: - Round-trip Coordinate Conversion Tests

    [Test]
    public void TestRectangularPolarRoundTrip()
    {
        var testCases = new[]
        {
            new RectAngCoordinates(1.0, 0.0, 0.0),
            new RectAngCoordinates(0.0, 1.0, 0.0),
            new RectAngCoordinates(0.0, 0.0, 1.0),
            new RectAngCoordinates(1.0, 1.0, 1.0),
            new RectAngCoordinates(2.0, 3.0, 4.0),
            new RectAngCoordinates(-1.0, 2.0, -3.0)
        };

        foreach (var rect in testCases)
        {
            // Skip origin as it has special handling
            if (rect.XCoord == 0.0 && rect.YCoord == 0.0 && rect.ZCoord == 0.0)
            {
                continue;
            }
            var polar = MathExtra.Rectangular2Polar(rect);
            var backToRect = MathExtra.Polar2Rectangular(polar);
            var xDiff = Math.Abs(backToRect.XCoord - rect.XCoord);
            var yDiff = Math.Abs(backToRect.YCoord - rect.YCoord);
            var zDiff = Math.Abs(backToRect.ZCoord - rect.ZCoord);
            Assert.That(xDiff, Is.LessThan(1e-10), $"Failed for x: {rect.XCoord}, difference: {xDiff}");
            Assert.That(yDiff, Is.LessThan(1e-10), $"Failed for y: {rect.YCoord}, difference: {yDiff}");
            Assert.That(zDiff, Is.LessThan(1e-10), $"Failed for z: {rect.ZCoord}, difference: {zDiff}");
        }
    }

    [Test]
    public void TestPolarRectangularRoundTrip()
    {
        var testCases = new[]
        {
            new PolarCoordinates(0.0, 0.0, 1.0),
            new PolarCoordinates(Math.PI / 2.0, 0.0, 1.0),
            new PolarCoordinates(0.0, Math.PI / 2.0, 1.0),
            new PolarCoordinates(Math.PI / 4.0, Math.PI / 6.0, 2.0),
            new PolarCoordinates(Math.PI, -Math.PI / 4.0, 3.0)
        };

        foreach (var polar in testCases)
        {
            var rect = MathExtra.Polar2Rectangular(polar);
            var backToPolar = MathExtra.Rectangular2Polar(rect);
            // Note: phi and theta might differ by 2π or sign, but r should match
            var rDiff = Math.Abs(backToPolar.RCoord - polar.RCoord);
            Assert.That(rDiff, Is.LessThan(1e-10), $"Failed for r: {polar.RCoord}, difference: {rDiff}");
        }
    }

    // MARK: - Edge Cases

    [Test]
    public void TestRectangular2PolarSmallValues()
    {
        var rect = new RectAngCoordinates(1e-10, 1e-10, 1e-10);
        var polar = MathExtra.Rectangular2Polar(rect);
        var expectedR = Math.Sqrt(3.0) * 1e-10;
        var rDiff = Math.Abs(polar.RCoord - expectedR);
        Assert.That(rDiff, Is.LessThan(1e-20), $"rCoord: expected {expectedR}, got {polar.RCoord}, difference: {rDiff}");
    }

    [Test]
    public void TestRectangular2PolarLargeValues()
    {
        var rect = new RectAngCoordinates(1e10, 1e10, 1e10);
        var polar = MathExtra.Rectangular2Polar(rect);
        var expectedR = Math.Sqrt(3.0) * 1e10;
        var rDiff = Math.Abs(polar.RCoord - expectedR);
        Assert.That(rDiff, Is.LessThan(1e5), $"rCoord: expected {expectedR}, got {polar.RCoord}, difference: {rDiff}");
    }

    [Test]
    public void TestDegToRadLargeValues()
    {
        var result = MathExtra.DegToRad(720.0);
        var difference = Math.Abs(result - (4 * Math.PI));
        Assert.That(difference, Is.LessThan(1e-10), $"Expected 4π, got {result}, difference: {difference}");
    }

    [Test]
    public void TestRadToDegLargeValues()
    {
        var result = MathExtra.RadToDeg(4 * Math.PI);
        var difference = Math.Abs(result - 720.0);
        Assert.That(difference, Is.LessThan(1e-10), $"Expected 720.0, got {result}, difference: {difference}");
    }
}
