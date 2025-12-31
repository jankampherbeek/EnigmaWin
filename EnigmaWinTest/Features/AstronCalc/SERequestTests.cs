// SERequestTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 31-12-2025

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;

namespace EnigmaWintest.Features.AstronCalc;

/// <summary>Tests for SERequest structure.</summary>
[TestFixture]
public class SERequestTests
{
    [Test]
    public void TestSERequestInitialization()
    {
        var julianDay = 2461034.0;
        var factors = new List<Factors> { Factors.Sun, Factors.Moon };
        var houseSystem = 1; // Placidus
        var seFlags = 258;
        var latitude = 52.3676;
        var longitude = 4.9041;

        var request = new SERequest(julianDay, factors, houseSystem, seFlags, latitude, longitude);

        Assert.That(request.JulianDay, Is.EqualTo(julianDay));
        Assert.That(request.FactorsToUse.Count, Is.EqualTo(2));
        Assert.That(request.FactorsToUse[0], Is.EqualTo(Factors.Sun));
        Assert.That(request.FactorsToUse[1], Is.EqualTo(Factors.Moon));
        Assert.That(request.HouseSystem, Is.EqualTo(houseSystem));
        Assert.That(request.SEFlags, Is.EqualTo(seFlags));
        Assert.That(request.Latitude, Is.EqualTo(latitude));
        Assert.That(request.Longitude, Is.EqualTo(longitude));
    }

    [Test]
    public void TestSERequestEmptyFactors()
    {
        var request = new SERequest(
            2461034.0,
            new List<Factors>(),
            1,
            258,
            52.3676,
            4.9041
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

        var request = new SERequest(
            2461034.0,
            factors,
            1,
            258,
            52.3676,
            4.9041
        );

        Assert.That(request.FactorsToUse.Count, Is.EqualTo(7));
    }

    [Test]
    public void TestSERequestWithChiron()
    {
        var factors = new List<Factors> { Factors.Chiron };
        var request = new SERequest(
            2461034.0,
            factors,
            1,
            258,
            52.3676,
            4.9041
        );

        Assert.That(request.FactorsToUse[0], Is.EqualTo(Factors.Chiron));
    }

    [Test]
    public void TestSERequestDifferentHouseSystem()
    {
        var request = new SERequest(
            2461034.0,
            new List<Factors> { Factors.Sun },
            19, // Gauquelin
            258,
            52.3676,
            4.9041
        );

        Assert.That(request.HouseSystem, Is.EqualTo(19));
    }

    [Test]
    public void TestSERequestDifferentLocation()
    {
        var request = new SERequest(
            2461034.0,
            new List<Factors> { Factors.Sun },
            1,
            258,
            40.7128, // New York
            -74.0060
        );

        Assert.That(request.Latitude, Is.EqualTo(40.7128));
        Assert.That(request.Longitude, Is.EqualTo(-74.0060));
    }

    [Test]
    public void TestSERequestZeroCoordinates()
    {
        var request = new SERequest(
            2461034.0,
            new List<Factors> { Factors.Sun },
            1,
            258,
            0.0,
            0.0
        );

        Assert.That(request.Latitude, Is.EqualTo(0.0));
        Assert.That(request.Longitude, Is.EqualTo(0.0));
    }
}

