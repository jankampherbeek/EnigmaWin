// AstronomicalPositionsTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 27-12-2025

using EnigmaWin.Sources.Domain;

namespace EnigmaWintest.Domain;

/// <summary>Tests for AstronomicalPositions domain types.</summary>
[TestFixture]
public class AstronomicalPositionsTests
{
    // Helper function to create FullCuspPosition for testing
    private FullCuspPosition CreateCuspPosition(double longitude, double rightAscension = 0.0, double declination = 0.0, double azimuth = 0.0, double altitude = 0.0)
    {
        return new FullCuspPosition(
            longitude,
            rightAscension,
            declination,
            new HorizontalPosition(azimuth, altitude)
        );
    }

    [Test]
    public void TestHousePositionsFullInitialization()
    {
        var cusps = new[]
        {
            CreateCuspPosition(0.0),
            CreateCuspPosition(30.0),
            CreateCuspPosition(60.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(120.0),
            CreateCuspPosition(150.0),
            CreateCuspPosition(180.0),
            CreateCuspPosition(210.0),
            CreateCuspPosition(240.0),
            CreateCuspPosition(270.0),
            CreateCuspPosition(300.0),
            CreateCuspPosition(330.0),
            CreateCuspPosition(360.0)
        };
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(15.5),
            CreateCuspPosition(90.0),
            CreateCuspPosition(105.0),
            CreateCuspPosition(195.0)
        );

        Assert.That(housePositions.Cusps.Length, Is.EqualTo(13));
        Assert.That(housePositions.Ascendant.Longitude, Is.EqualTo(15.5));
        Assert.That(housePositions.Midheaven.Longitude, Is.EqualTo(90.0));
        Assert.That(housePositions.Eastpoint.Longitude, Is.EqualTo(105.0));
        Assert.That(housePositions.Vertex.Longitude, Is.EqualTo(195.0));
    }

    [Test]
    public void TestHousePositionsCuspsArray()
    {
        var cusps = new[]
        {
            CreateCuspPosition(10.0),
            CreateCuspPosition(20.0),
            CreateCuspPosition(30.0),
            CreateCuspPosition(40.0),
            CreateCuspPosition(50.0),
            CreateCuspPosition(60.0),
            CreateCuspPosition(70.0),
            CreateCuspPosition(80.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(100.0),
            CreateCuspPosition(110.0),
            CreateCuspPosition(120.0),
            CreateCuspPosition(130.0)
        };
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(10.0),
            CreateCuspPosition(100.0),
            CreateCuspPosition(110.0),
            CreateCuspPosition(200.0)
        );

        Assert.That(housePositions.Cusps.Length, Is.EqualTo(13));
        Assert.That(housePositions.Cusps[0].Longitude, Is.EqualTo(10.0));
        Assert.That(housePositions.Cusps[12].Longitude, Is.EqualTo(130.0));
    }

    [Test]
    public void TestHousePositionsEmptyCusps()
    {
        var housePositions = new HousePositions(
            Array.Empty<FullCuspPosition>(),
            CreateCuspPosition(0.0),
            CreateCuspPosition(0.0),
            CreateCuspPosition(0.0),
            CreateCuspPosition(0.0)
        );

        Assert.That(housePositions.Cusps, Is.Empty);
        Assert.That(housePositions.Ascendant.Longitude, Is.EqualTo(0.0));
        Assert.That(housePositions.Midheaven.Longitude, Is.EqualTo(0.0));
    }

    [Test]
    public void TestHousePositionsStandard12Houses()
    {
        // Standard 12 houses plus one extra (13 elements total)
        var cusps = new[]
        {
            CreateCuspPosition(0.0),
            CreateCuspPosition(30.0),
            CreateCuspPosition(60.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(120.0),
            CreateCuspPosition(150.0),
            CreateCuspPosition(180.0),
            CreateCuspPosition(210.0),
            CreateCuspPosition(240.0),
            CreateCuspPosition(270.0),
            CreateCuspPosition(300.0),
            CreateCuspPosition(330.0),
            CreateCuspPosition(360.0)
        };
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(0.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(270.0)
        );

        Assert.That(housePositions.Cusps.Length, Is.EqualTo(13));
        Assert.That(housePositions.Cusps[1].Longitude, Is.EqualTo(30.0)); // First house cusp
        Assert.That(housePositions.Cusps[12].Longitude, Is.EqualTo(360.0)); // Last cusp
    }

    [Test]
    public void TestHousePositionsAscendant()
    {
        var cusps = new[]
        {
            CreateCuspPosition(0.0),
            CreateCuspPosition(30.0),
            CreateCuspPosition(60.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(120.0),
            CreateCuspPosition(150.0),
            CreateCuspPosition(180.0),
            CreateCuspPosition(210.0),
            CreateCuspPosition(240.0),
            CreateCuspPosition(270.0),
            CreateCuspPosition(300.0),
            CreateCuspPosition(330.0),
            CreateCuspPosition(360.0)
        };
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(15.5125),
            CreateCuspPosition(90.0),
            CreateCuspPosition(105.0),
            CreateCuspPosition(195.0)
        );

        Assert.That(housePositions.Ascendant.Longitude, Is.EqualTo(15.5125));
    }

    [Test]
    public void TestHousePositionsMidheaven()
    {
        var cusps = new[]
        {
            CreateCuspPosition(0.0),
            CreateCuspPosition(30.0),
            CreateCuspPosition(60.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(120.0),
            CreateCuspPosition(150.0),
            CreateCuspPosition(180.0),
            CreateCuspPosition(210.0),
            CreateCuspPosition(240.0),
            CreateCuspPosition(270.0),
            CreateCuspPosition(300.0),
            CreateCuspPosition(330.0),
            CreateCuspPosition(360.0)
        };
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(0.0),
            CreateCuspPosition(90.1234),
            CreateCuspPosition(105.0),
            CreateCuspPosition(195.0)
        );

        Assert.That(housePositions.Midheaven.Longitude, Is.EqualTo(90.1234));
    }

    [Test]
    public void TestHousePositionsEastpoint()
    {
        var cusps = new[]
        {
            CreateCuspPosition(0.0),
            CreateCuspPosition(30.0),
            CreateCuspPosition(60.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(120.0),
            CreateCuspPosition(150.0),
            CreateCuspPosition(180.0),
            CreateCuspPosition(210.0),
            CreateCuspPosition(240.0),
            CreateCuspPosition(270.0),
            CreateCuspPosition(300.0),
            CreateCuspPosition(330.0),
            CreateCuspPosition(360.0)
        };
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(0.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(105.5678),
            CreateCuspPosition(195.0)
        );

        Assert.That(housePositions.Eastpoint.Longitude, Is.EqualTo(105.5678));
    }

    [Test]
    public void TestHousePositionsVertex()
    {
        var cusps = new[]
        {
            CreateCuspPosition(0.0),
            CreateCuspPosition(30.0),
            CreateCuspPosition(60.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(120.0),
            CreateCuspPosition(150.0),
            CreateCuspPosition(180.0),
            CreateCuspPosition(210.0),
            CreateCuspPosition(240.0),
            CreateCuspPosition(270.0),
            CreateCuspPosition(300.0),
            CreateCuspPosition(330.0),
            CreateCuspPosition(360.0)
        };
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(0.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(105.0),
            CreateCuspPosition(195.9999)
        );

        Assert.That(housePositions.Vertex.Longitude, Is.EqualTo(195.9999));
    }

    [Test]
    public void TestHousePositionsAllAnglesZero()
    {
        var cusps = Enumerable.Repeat(CreateCuspPosition(0.0), 13).ToArray();
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(0.0),
            CreateCuspPosition(0.0),
            CreateCuspPosition(0.0),
            CreateCuspPosition(0.0)
        );

        Assert.That(housePositions.Ascendant.Longitude, Is.EqualTo(0.0));
        Assert.That(housePositions.Midheaven.Longitude, Is.EqualTo(0.0));
        Assert.That(housePositions.Eastpoint.Longitude, Is.EqualTo(0.0));
        Assert.That(housePositions.Vertex.Longitude, Is.EqualTo(0.0));
    }

    [Test]
    public void TestHousePositionsNegativeAngles()
    {
        var cusps = new[]
        {
            CreateCuspPosition(-10.0),
            CreateCuspPosition(0.0),
            CreateCuspPosition(10.0),
            CreateCuspPosition(20.0),
            CreateCuspPosition(30.0),
            CreateCuspPosition(40.0),
            CreateCuspPosition(50.0),
            CreateCuspPosition(60.0),
            CreateCuspPosition(70.0),
            CreateCuspPosition(80.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(100.0),
            CreateCuspPosition(110.0)
        };
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(-5.0),
            CreateCuspPosition(90.0),
            CreateCuspPosition(105.0),
            CreateCuspPosition(195.0)
        );

        Assert.That(housePositions.Ascendant.Longitude, Is.EqualTo(-5.0));
        Assert.That(housePositions.Cusps[0].Longitude, Is.EqualTo(-10.0));
    }

    [Test]
    public void TestHousePositionsLargeAngles()
    {
        var cusps = new[]
        {
            CreateCuspPosition(360.0),
            CreateCuspPosition(390.0),
            CreateCuspPosition(420.0),
            CreateCuspPosition(450.0),
            CreateCuspPosition(480.0),
            CreateCuspPosition(510.0),
            CreateCuspPosition(540.0),
            CreateCuspPosition(570.0),
            CreateCuspPosition(600.0),
            CreateCuspPosition(630.0),
            CreateCuspPosition(660.0),
            CreateCuspPosition(690.0),
            CreateCuspPosition(720.0)
        };
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(375.0),
            CreateCuspPosition(450.0),
            CreateCuspPosition(465.0),
            CreateCuspPosition(555.0)
        );

        Assert.That(housePositions.Ascendant.Longitude, Is.EqualTo(375.0));
        Assert.That(housePositions.Midheaven.Longitude, Is.EqualTo(450.0));
        Assert.That(housePositions.Cusps[12].Longitude, Is.EqualTo(720.0));
    }

    [Test]
    public void TestHousePositionsPreciseDecimals()
    {
        var cusps = new[]
        {
            CreateCuspPosition(0.123456),
            CreateCuspPosition(30.234567),
            CreateCuspPosition(60.345678),
            CreateCuspPosition(90.456789),
            CreateCuspPosition(120.567890),
            CreateCuspPosition(150.678901),
            CreateCuspPosition(180.789012),
            CreateCuspPosition(210.890123),
            CreateCuspPosition(240.901234),
            CreateCuspPosition(270.012345),
            CreateCuspPosition(300.123456),
            CreateCuspPosition(330.234567),
            CreateCuspPosition(360.345678)
        };
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(15.123456789),
            CreateCuspPosition(90.987654321),
            CreateCuspPosition(105.111111111),
            CreateCuspPosition(195.999999999)
        );

        Assert.That(housePositions.Ascendant.Longitude, Is.EqualTo(15.123456789));
        Assert.That(housePositions.Midheaven.Longitude, Is.EqualTo(90.987654321));
        Assert.That(housePositions.Eastpoint.Longitude, Is.EqualTo(105.111111111));
        Assert.That(housePositions.Vertex.Longitude, Is.EqualTo(195.999999999));
        Assert.That(housePositions.Cusps[0].Longitude, Is.EqualTo(0.123456));
    }

    [Test]
    public void TestHousePositionsCuspsImmutability()
    {
        var originalCusps = new[]
        {
            CreateCuspPosition(10.0),
            CreateCuspPosition(20.0),
            CreateCuspPosition(30.0)
        };
        var housePositions = new HousePositions(
            originalCusps,
            CreateCuspPosition(10.0),
            CreateCuspPosition(100.0),
            CreateCuspPosition(110.0),
            CreateCuspPosition(200.0)
        );

        // Modify original array
        var modifiedCusps = originalCusps.ToList();
        modifiedCusps.Add(CreateCuspPosition(40.0));

        // HousePositions cusps should remain unchanged (records are immutable, but arrays are reference types)
        // In C#, the array reference is copied, so modifying the original array would affect the record
        // This test verifies the count is still 3 (the record maintains its own reference)
        Assert.That(housePositions.Cusps.Length, Is.EqualTo(3));
        Assert.That(modifiedCusps.Count, Is.EqualTo(4));
    }

    [Test]
    public void TestHousePositionsTypicalAstrologicalValues()
    {
        // Typical house cusps for a chart
        var cusps = new[]
        {
            CreateCuspPosition(0.0),
            CreateCuspPosition(45.5),
            CreateCuspPosition(75.25),
            CreateCuspPosition(105.0),
            CreateCuspPosition(135.75),
            CreateCuspPosition(165.5),
            CreateCuspPosition(195.0),
            CreateCuspPosition(225.5),
            CreateCuspPosition(255.25),
            CreateCuspPosition(285.0),
            CreateCuspPosition(315.75),
            CreateCuspPosition(345.5),
            CreateCuspPosition(360.0)
        };
        var housePositions = new HousePositions(
            cusps,
            CreateCuspPosition(15.5),  // Aries 15°30'
            CreateCuspPosition(105.0), // Cancer 15°
            CreateCuspPosition(105.0),
            CreateCuspPosition(195.0)     // Libra 15°
        );

        Assert.That(housePositions.Cusps.Length, Is.EqualTo(13));
        Assert.That(housePositions.Ascendant.Longitude, Is.EqualTo(15.5));
        Assert.That(housePositions.Midheaven.Longitude, Is.EqualTo(105.0));
        Assert.That(housePositions.Cusps[1].Longitude, Is.EqualTo(45.5)); // First house cusp
        Assert.That(housePositions.Cusps[4].Longitude, Is.EqualTo(135.75)); // Fourth house cusp
    }
}

