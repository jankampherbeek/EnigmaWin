// CalcRequestTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 31-12-2025

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWintest.Features.AstronCalc;

/// <summary>Tests for CalcRequest structure.</summary>
[TestFixture]
public class CalcRequestTests
{
    [Test]
    public void TestSERequestInitialization()
    {
        const double julianDay = 2461034.0;
        var factors = new List<Factors> { Factors.Sun, Factors.Moon };
        const int houseSystem = 1; // Placidus
        const int seFlags = 258;
        const double latitude = 52.3676;
        const double longitude = 4.9041;

        var config = CalculationConfig.Default;
        var request = new CalcRequest(julianDay, factors, houseSystem, seFlags, latitude, longitude, 0.0, config);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(request.JulianDay, Is.EqualTo(julianDay));
            Assert.That(request.FactorsToUse, Has.Count.EqualTo(2));
            Assert.That(request.FactorsToUse[0], Is.EqualTo(Factors.Sun));
            Assert.That(request.FactorsToUse[1], Is.EqualTo(Factors.Moon));
            Assert.That(request.HouseSystem, Is.EqualTo(houseSystem));
            Assert.That(request.SEFlags, Is.EqualTo(seFlags));
            Assert.That(request.Latitude, Is.EqualTo(latitude));
            Assert.That(request.Longitude, Is.EqualTo(longitude));
        }
    }

    [Test]
    public void TestSERequestEmptyFactors()
    {
        var request = new CalcRequest(
            2461034.0,
            [],
            1,
            258,
            52.3676,
            4.9041,
            0.0,
            CalculationConfig.Default
        );

        Assert.That(request.FactorsToUse, Is.Empty);
    }

    [Test]
    public void TestSERequestMultipleFactors()
    {
        var factors = new List<Factors>
        {
            Factors.Sun,
            Factors.Moon,
            Factors.Mercury,
            Factors.Venus,
            Factors.Mars,
            Factors.Jupiter,
            Factors.Saturn
        };

        var request = new CalcRequest(
            2461034.0,
            factors,
            1,
            258,
            52.3676,
            4.9041,
            0.0,
            CalculationConfig.Default
        );

        Assert.That(request.FactorsToUse, Has.Count.EqualTo(7));
    }

    [Test]
    public void TestSERequestWithChiron()
    {
        var factors = new List<Factors> { Factors.Chiron };
        var request = new CalcRequest(
            2461034.0,
            factors,
            1,
            258,
            52.3676,
            4.9041,
            0.0,
            CalculationConfig.Default
        );

        Assert.That(request.FactorsToUse[0], Is.EqualTo(Factors.Chiron));
    }

    [Test]
    public void TestSERequestDifferentHouseSystem()
    {
        var request = new CalcRequest(
            2461034.0,
            [Factors.Sun],
            19, // Gauquelin
            258,
            52.3676,
            4.9041,
            0.0,
            CalculationConfig.Default
        );

        Assert.That(request.HouseSystem, Is.EqualTo(19));
    }

    [Test]
    public void TestSERequestDifferentLocation()
    {
        var request = new CalcRequest(
            2461034.0,
            [Factors.Sun],
            1,
            258,
            40.7128, // New York
            -74.0060,
            0.0,
            CalculationConfig.Default
        );

        using (Assert.EnterMultipleScope())
        {
            Assert.That(request.Latitude, Is.EqualTo(40.7128));
            Assert.That(request.Longitude, Is.EqualTo(-74.0060));
        }
    }

    [Test]
    public void TestSERequestZeroCoordinates()
    {
        var request = new CalcRequest(
            2461034.0,
            [Factors.Sun],
            1,
            258,
            0.0,
            0.0,
            0.0,
            CalculationConfig.Default
        );

        using (Assert.EnterMultipleScope())
        {
            Assert.That(request.Latitude, Is.EqualTo(0.0));
            Assert.That(request.Longitude, Is.EqualTo(0.0));
        }
    }
}

