using EnigmaWin.Sources.Features.Shared.Validation;

namespace EnigmaWintest.Features.Shared.Validation;

/// <summary>Tests for CoordinateValidation utilities.</summary>
[TestFixture]
public class CoordinateValidationTests
{
    // MARK: - Longitude Validation Tests

    [Test]
    public void TestLongitudeLowerBound()
    {
        Assert.That(CoordinateValidation.ValidateLongitude(-180.0), Is.True);
    }

    [Test]
    public void TestLongitudeUpperBound()
    {
        Assert.That(CoordinateValidation.ValidateLongitude(180.0), Is.True);
    }

    [Test]
    public void TestLongitudeMiddle()
    {
        Assert.That(CoordinateValidation.ValidateLongitude(0.0), Is.True);
    }

    [Test]
    public void TestLongitudeValidPositive()
    {
        Assert.That(CoordinateValidation.ValidateLongitude(45.5), Is.True);
    }

    [Test]
    public void TestLongitudeValidNegative()
    {
        Assert.That(CoordinateValidation.ValidateLongitude(-120.25), Is.True);
    }

    [Test]
    public void TestLongitudeBelowLowerBound()
    {
        Assert.That(CoordinateValidation.ValidateLongitude(-180.1), Is.False);
    }

    [Test]
    public void TestLongitudeAboveUpperBound()
    {
        Assert.That(CoordinateValidation.ValidateLongitude(180.1), Is.False);
    }

    [Test]
    public void TestLongitudeFarBelowRange()
    {
        Assert.That(CoordinateValidation.ValidateLongitude(-360.0), Is.False);
    }

    [Test]
    public void TestLongitudeFarAboveRange()
    {
        Assert.That(CoordinateValidation.ValidateLongitude(360.0), Is.False);
    }

    // MARK: - Latitude Validation Tests

    [Test]
    public void TestLatitudeJustAboveLowerBound()
    {
        Assert.That(CoordinateValidation.ValidateLatitude(-89.999), Is.True);
    }

    [Test]
    public void TestLatitudeJustBelowUpperBound()
    {
        Assert.That(CoordinateValidation.ValidateLatitude(89.999), Is.True);
    }

    [Test]
    public void TestLatitudeMiddle()
    {
        Assert.That(CoordinateValidation.ValidateLatitude(0.0), Is.True);
    }

    [Test]
    public void TestLatitudeValidPositive()
    {
        Assert.That(CoordinateValidation.ValidateLatitude(45.5), Is.True);
    }

    [Test]
    public void TestLatitudeValidNegative()
    {
        Assert.That(CoordinateValidation.ValidateLatitude(-30.25), Is.True);
    }

    [Test]
    public void TestLatitudeAtLowerBound()
    {
        Assert.That(CoordinateValidation.ValidateLatitude(-90.0), Is.False);
    }

    [Test]
    public void TestLatitudeAtUpperBound()
    {
        Assert.That(CoordinateValidation.ValidateLatitude(90.0), Is.False);
    }

    [Test]
    public void TestLatitudeBelowLowerBound()
    {
        Assert.That(CoordinateValidation.ValidateLatitude(-90.1), Is.False);
    }

    [Test]
    public void TestLatitudeAboveUpperBound()
    {
        Assert.That(CoordinateValidation.ValidateLatitude(90.1), Is.False);
    }

    [Test]
    public void TestLatitudeFarBelowRange()
    {
        Assert.That(CoordinateValidation.ValidateLatitude(-180.0), Is.False);
    }

    [Test]
    public void TestLatitudeFarAboveRange()
    {
        Assert.That(CoordinateValidation.ValidateLatitude(180.0), Is.False);
    }
}

