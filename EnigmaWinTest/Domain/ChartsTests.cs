// ChartsTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 27-12-2025

using EnigmaWin.Sources.Domain;

namespace EnigmaWintest.Domain;

/// <summary>Tests for Charts domain types.</summary>
[TestFixture]
public class ChartsTests
{
    // MARK: - Helper Functions

    /// <summary>Helper function to create FullCuspPosition for testing.</summary>
    private FullCuspPosition CreateCuspPosition(double longitude, double rightAscension = 0.0, double declination = 0.0, double azimuth = 0.0, double altitude = 0.0)
    {
        return new FullCuspPosition(
            longitude,
            rightAscension,
            declination,
            new HorizontalPosition(azimuth, altitude)
        );
    }

    /// <summary>Helper function to create HousePositions for testing.</summary>
    private HousePositions CreateHousePositions(double ascendant = 0.0, double midheaven = 0.0, double eastpoint = 0.0, double vertex = 0.0)
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
        return new HousePositions(
            cusps,
            CreateCuspPosition(ascendant),
            CreateCuspPosition(midheaven),
            CreateCuspPosition(eastpoint),
            CreateCuspPosition(vertex)
        );
    }

    /// <summary>Helper function to create FullFactorPosition for testing.</summary>
    private FullFactorPosition CreateFactorPosition(double eclipticalLongitude = 0.0, double equatorialRa = 0.0, double azimuth = 0.0, double altitude = 0.0)
    {
        var ecliptical = new MainAstronomicalPosition(
            eclipticalLongitude,
            0.0,
            1.0
        );
        var equatorial = new MainAstronomicalPosition(
            equatorialRa,
            0.0,
            1.0
        );
        var horizontal = new HorizontalPosition(azimuth, altitude);

        return new FullFactorPosition(
            [ecliptical],
            [equatorial],
            [horizontal]
        );
    }

    // MARK: - Initialization Tests

    [Test]
    public void TestFullChartInitialization()
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>
        {
            { Factors.Sun, CreateFactorPosition(eclipticalLongitude: 45.5) },
            { Factors.Moon, CreateFactorPosition(eclipticalLongitude: 120.25) }
        };
        var housePositions = CreateHousePositions(ascendant: 15.5, midheaven: 90.0);
        var siderealTime = 12.345678;
        var julianDay = 2461034.1666666665;
        var obliquity = 23.4375;

        var chart = new FullChart(
            coordinates,
            housePositions,
            siderealTime,
            julianDay,
            obliquity
        );

        Assert.That(chart.Coordinates.Count, Is.EqualTo(2));
        Assert.That(chart.HousePositions.Ascendant.Longitude, Is.EqualTo(15.5));
        Assert.That(chart.SiderealTime, Is.EqualTo(siderealTime));
        Assert.That(chart.JulianDay, Is.EqualTo(julianDay));
        Assert.That(chart.Obliquity, Is.EqualTo(obliquity));
    }

    [Test]
    public void TestFullChartEmptyCoordinates()
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        var housePositions = CreateHousePositions();
        var chart = new FullChart(
            coordinates,
            housePositions,
            0.0,
            0.0,
            0.0
        );

        Assert.That(chart.Coordinates, Is.Empty);
        Assert.That(chart.HousePositions.Cusps.Length, Is.EqualTo(13));
    }

    [Test]
    public void TestFullChartSingleFactor()
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>
        {
            { Factors.Sun, CreateFactorPosition(eclipticalLongitude: 180.0) }
        };
        var housePositions = CreateHousePositions();
        var chart = new FullChart(
            coordinates,
            housePositions,
            6.0,
            2451545.0,
            23.44
        );

        Assert.That(chart.Coordinates.Count, Is.EqualTo(1));
        Assert.That(chart.Coordinates[Factors.Sun].Ecliptical[0].MainPos, Is.EqualTo(180.0));
    }

    [Test]
    public void TestFullChartMultipleFactors()
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>
        {
            { Factors.Sun, CreateFactorPosition(eclipticalLongitude: 0.0) },
            { Factors.Moon, CreateFactorPosition(eclipticalLongitude: 30.0) },
            { Factors.Mercury, CreateFactorPosition(eclipticalLongitude: 60.0) },
            { Factors.Venus, CreateFactorPosition(eclipticalLongitude: 90.0) },
            { Factors.Mars, CreateFactorPosition(eclipticalLongitude: 120.0) },
            { Factors.Jupiter, CreateFactorPosition(eclipticalLongitude: 150.0) },
            { Factors.Saturn, CreateFactorPosition(eclipticalLongitude: 180.0) }
        };
        var housePositions = CreateHousePositions();
        var chart = new FullChart(
            coordinates,
            housePositions,
            12.0,
            2451545.5,
            23.5
        );
        Assert.Multiple(() =>
        {
            Assert.That(chart.Coordinates, Has.Count.EqualTo(7));
            Assert.That(chart.Coordinates[Factors.Sun].Ecliptical[0].MainPos, Is.EqualTo(0.0));
            Assert.That(chart.Coordinates[Factors.Moon].Ecliptical[0].MainPos, Is.EqualTo(30.0));
            Assert.That(chart.Coordinates[Factors.Saturn].Ecliptical[0].MainPos, Is.EqualTo(180.0));
        });
    }

    // MARK: - Property Access Tests

    [Test]
    public void TestFullChartCoordinatesAccess()
    {
        var sunPosition = CreateFactorPosition(eclipticalLongitude: 45.123, equatorialRa: 3.0, azimuth: 180.0, altitude: 30.0);
        var coordinates = new Dictionary<Factors, FullFactorPosition>
        {
            { Factors.Sun, sunPosition }
        };
        var chart = new FullChart(
            coordinates,
            CreateHousePositions(),
            0.0,
            0.0,
            0.0
        );

        var retrievedPosition = chart.Coordinates[Factors.Sun];
        Assert.That(retrievedPosition, Is.Not.Null);
        Assert.That(retrievedPosition.Ecliptical[0].MainPos, Is.EqualTo(45.123));
        Assert.That(retrievedPosition.Equatorial[0].MainPos, Is.EqualTo(3.0));
        Assert.That(retrievedPosition.Horizontal[0].Azimuth, Is.EqualTo(180.0));
        Assert.That(retrievedPosition.Horizontal[0].Altitude, Is.EqualTo(30.0));
    }

    [Test]
    public void TestFullChartHousePositionsAccess()
    {
        var housePositions = CreateHousePositions(
            ascendant: 15.5,
            midheaven: 90.0,
            eastpoint: 105.0,
            vertex: 195.0
        );
        var chart = new FullChart(
            new Dictionary<Factors, FullFactorPosition>(),
            housePositions,
            0.0,
            0.0,
            0.0
        );

        Assert.That(chart.HousePositions.Ascendant.Longitude, Is.EqualTo(15.5));
        Assert.That(chart.HousePositions.Midheaven.Longitude, Is.EqualTo(90.0));
        Assert.That(chart.HousePositions.Eastpoint.Longitude, Is.EqualTo(105.0));
        Assert.That(chart.HousePositions.Vertex.Longitude, Is.EqualTo(195.0));
        Assert.That(chart.HousePositions.Cusps.Length, Is.EqualTo(13));
    }

    [Test]
    public void TestFullChartSiderealTimeAccess()
    {
        var siderealTime = 12.3456789;
        var chart = new FullChart(
            new Dictionary<Factors, FullFactorPosition>(),
            CreateHousePositions(),
            siderealTime,
            0.0,
            0.0
        );

        Assert.That(chart.SiderealTime, Is.EqualTo(siderealTime));
    }

    [Test]
    public void TestFullChartJulianDayAccess()
    {
        var julianDay = 2461034.1666666665;
        var chart = new FullChart(
            new Dictionary<Factors, FullFactorPosition>(),
            CreateHousePositions(),
            0.0,
            julianDay,
            0.0
        );

        Assert.That(chart.JulianDay, Is.EqualTo(julianDay));
    }

    [Test]
    public void TestFullChartObliquityAccess()
    {
        var obliquity = 23.4375;
        var chart = new FullChart(
            new Dictionary<Factors, FullFactorPosition>(),
            CreateHousePositions(),
            0.0,
            0.0,
            obliquity
        );

        Assert.That(chart.Obliquity, Is.EqualTo(obliquity));
    }

    // MARK: - Edge Cases

    [Test]
    public void TestFullChartZeroValues()
    {
        var chart = new FullChart(
            new Dictionary<Factors, FullFactorPosition>(),
            CreateHousePositions(),
            0.0,
            0.0,
            0.0
        );

        Assert.That(chart.SiderealTime, Is.EqualTo(0.0));
        Assert.That(chart.JulianDay, Is.EqualTo(0.0));
        Assert.That(chart.Obliquity, Is.EqualTo(0.0));
    }

    [Test]
    public void TestFullChartNegativeSiderealTime()
    {
        var chart = new FullChart(
            new Dictionary<Factors, FullFactorPosition>(),
            CreateHousePositions(),
            -12.0,
            2451545.0,
            23.44
        );

        Assert.That(chart.SiderealTime, Is.EqualTo(-12.0));
    }

    [Test]
    public void TestFullChartLargeJulianDay()
    {
        var julianDay = 2500000.0;
        var chart = new FullChart(
            new Dictionary<Factors, FullFactorPosition>(),
            CreateHousePositions(),
            0.0,
            julianDay,
            23.44
        );

        Assert.That(chart.JulianDay, Is.EqualTo(julianDay));
    }

    [Test]
    public void TestFullChartNegativeObliquity()
    {
        var chart = new FullChart(
            new Dictionary<Factors, FullFactorPosition>(),
            CreateHousePositions(),
            0.0,
            2451545.0,
            -23.44
        );

        Assert.That(chart.Obliquity, Is.EqualTo(-23.44));
    }

    [Test]
    public void TestFullChartPreciseDecimals()
    {
        var siderealTime = 12.345678901234567;
        var julianDay = 2461034.166666666666666;
        var obliquity = 23.437500000000001;

        var chart = new FullChart(
            new Dictionary<Factors, FullFactorPosition>(),
            CreateHousePositions(),
            siderealTime,
            julianDay,
            obliquity
        );

        Assert.That(chart.SiderealTime, Is.EqualTo(siderealTime));
        Assert.That(chart.JulianDay, Is.EqualTo(julianDay));
        Assert.That(chart.Obliquity, Is.EqualTo(obliquity));
    }

    // MARK: - Typical Astrological Values

    [Test]
    public void TestFullChartTypicalAstrologicalValues()
    {
        // Typical values for a natal chart
        var coordinates = new Dictionary<Factors, FullFactorPosition>
        {
            { Factors.Sun, CreateFactorPosition(eclipticalLongitude: 15.5) },      // Aries 15°30'
            { Factors.Moon, CreateFactorPosition(eclipticalLongitude: 105.25) },  // Cancer 15°15'
            { Factors.Mercury, CreateFactorPosition(eclipticalLongitude: 30.75) }, // Taurus 0°45'
            { Factors.Venus, CreateFactorPosition(eclipticalLongitude: 60.5) },   // Gemini 0°30'
            { Factors.Mars, CreateFactorPosition(eclipticalLongitude: 180.0) },    // Libra 0°
            { Factors.Jupiter, CreateFactorPosition(eclipticalLongitude: 240.0) },  // Sagittarius 0°
            { Factors.Saturn, CreateFactorPosition(eclipticalLongitude: 270.0) }    // Capricorn 0°
        };
        var housePositions = CreateHousePositions(
            ascendant: 15.5,   // Aries 15°30'
            midheaven: 105.0,  // Cancer 15°
            eastpoint: 105.0,
            vertex: 195.0      // Libra 15°
        );
        var siderealTime = 12.345678;
        var julianDay = 2461034.1666666665;  // 2025-01-01 16:00:00 UT
        var obliquity = 23.4375;  // Approximate obliquity for 2025

        var chart = new FullChart(
            coordinates,
            housePositions,
            siderealTime,
            julianDay,
            obliquity
        );

        Assert.That(chart.Coordinates.Count, Is.EqualTo(7));
        Assert.That(chart.Coordinates[Factors.Sun].Ecliptical[0].MainPos, Is.EqualTo(15.5));
        Assert.That(chart.Coordinates[Factors.Moon].Ecliptical[0].MainPos, Is.EqualTo(105.25));
        Assert.That(chart.HousePositions.Ascendant.Longitude, Is.EqualTo(15.5));
        Assert.That(chart.HousePositions.Midheaven.Longitude, Is.EqualTo(105.0));
        Assert.That(chart.SiderealTime, Is.EqualTo(siderealTime));
        Assert.That(chart.JulianDay, Is.EqualTo(julianDay));
        Assert.That(chart.Obliquity, Is.EqualTo(obliquity));
    }

    // MARK: - Immutability Tests



    // MARK: - Complex Scenarios

    [Test]
    public void TestFullChartAllMajorPlanets()
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>
        {
            { Factors.Sun, CreateFactorPosition(eclipticalLongitude: 0.0) },
            { Factors.Moon, CreateFactorPosition(eclipticalLongitude: 30.0) },
            { Factors.Mercury, CreateFactorPosition(eclipticalLongitude: 60.0) },
            { Factors.Venus, CreateFactorPosition(eclipticalLongitude: 90.0) },
            { Factors.Mars, CreateFactorPosition(eclipticalLongitude: 120.0) },
            { Factors.Jupiter, CreateFactorPosition(eclipticalLongitude: 150.0) },
            { Factors.Saturn, CreateFactorPosition(eclipticalLongitude: 180.0) },
            { Factors.Uranus, CreateFactorPosition(eclipticalLongitude: 210.0) },
            { Factors.Neptune, CreateFactorPosition(eclipticalLongitude: 240.0) },
            { Factors.Pluto, CreateFactorPosition(eclipticalLongitude: 270.0) }
        };
        var chart = new FullChart(
            coordinates,
            CreateHousePositions(),
            0.0,
            0.0,
            0.0
        );

        Assert.That(chart.Coordinates.Count, Is.EqualTo(10));
        Assert.Multiple(() =>
        {
            Assert.That(chart.Coordinates.ContainsKey(Factors.Sun), Is.True);
            Assert.That(chart.Coordinates.ContainsKey(Factors.Moon), Is.True);
            Assert.That(chart.Coordinates.ContainsKey(Factors.Mercury), Is.True);
            Assert.That(chart.Coordinates.ContainsKey(Factors.Venus), Is.True);
            Assert.That(chart.Coordinates.ContainsKey(Factors.Mars), Is.True);
            Assert.That(chart.Coordinates.ContainsKey(Factors.Jupiter), Is.True);
            Assert.That(chart.Coordinates.ContainsKey(Factors.Saturn), Is.True);
            Assert.That(chart.Coordinates.ContainsKey(Factors.Uranus), Is.True);
            Assert.That(chart.Coordinates.ContainsKey(Factors.Neptune), Is.True);
            Assert.That(chart.Coordinates.ContainsKey(Factors.Pluto), Is.True);
        });
    }

    [Test]
    public void TestFullChartDifferentCoordinateSystems()
    {
        var sunPosition = CreateFactorPosition(
            eclipticalLongitude: 45.0,
            equatorialRa: 3.0,
            azimuth: 180.0,
            altitude: 30.0
        );
        var moonPosition = CreateFactorPosition(
            eclipticalLongitude: 120.0,
            equatorialRa: 8.0,
            azimuth: 270.0,
            altitude: 45.0
        );
        var coordinates = new Dictionary<Factors, FullFactorPosition>
        {
            { Factors.Sun, sunPosition },
            { Factors.Moon, moonPosition }
        };
        var chart = new FullChart(
            coordinates,
            CreateHousePositions(),
            0.0,
            0.0,
            0.0
        );

        var sun = chart.Coordinates[Factors.Sun];
        var moon = chart.Coordinates[Factors.Moon];

        Assert.That(sun.Ecliptical[0].MainPos, Is.EqualTo(45.0));
        Assert.That(sun.Equatorial[0].MainPos, Is.EqualTo(3.0));
        Assert.That(sun.Horizontal[0].Azimuth, Is.EqualTo(180.0));
        Assert.That(sun.Horizontal[0].Altitude, Is.EqualTo(30.0));

        Assert.That(moon.Ecliptical[0].MainPos, Is.EqualTo(120.0));
        Assert.That(moon.Equatorial[0].MainPos, Is.EqualTo(8.0));
        Assert.That(moon.Horizontal[0].Azimuth, Is.EqualTo(270.0));
        Assert.That(moon.Horizontal[0].Altitude, Is.EqualTo(45.0));
    }
}

