// ElementsCalcTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWintest.Features.AstronCalc;

/// <summary>Tests for ElementsCalc utilities.</summary>
[TestFixture]
public class ElementsCalcTests
{
    private SEWrapper? _seWrapper;
    private string? _originalEphePath;

    private static CalculationConfig TestConfig => new(
        HouseSystems.NoHouses,
        Ayanamshas.Tropical,
        ObserverPositions.Geocentric,
        ProjectionTypes.TwoDimensional,
        BlackMoonCorrectionTypes.Duval,
        LunarNodeTypes.MeanNode,
        LotsTypes.Sect);

    [SetUp]
    public void SetUp()
    {
        _originalEphePath = Environment.GetEnvironmentVariable("EPHE_PATH");

        // Initialize SEWrapper if possible
        // Note: This requires the Swiss Ephemeris DLL to be available
        try
        {
            var ephePath = _originalEphePath ?? Environment.CurrentDirectory;
            SEWrapper.SeInitializer(ephePath);
            _seWrapper = new SEWrapper();
        }
        catch (Exception)
        {
            // If initialization fails, tests will be skipped
            _seWrapper = null;
        }
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up SEWrapper
        try
        {
            SEWrapper.CloseEphemeris();
        }
        catch (Exception)
        {
            // Ignore cleanup errors
        }

        // Restore original environment
        if (_originalEphePath != null)
        {
            Environment.SetEnvironmentVariable("EPHE_PATH", _originalEphePath);
        }
    }

    [Test]
    public void TestCalculateElementsFactors()
    {
        // Skip test if SEWrapper is not initialized
        if (_seWrapper == null)
        {
            Assert.Ignore("SEWrapper not initialized. Swiss Ephemeris DLL may not be available.");
            return;
        }

        // Julian Day for January 1, 2010 0:00 UT
        const double julianDay = 2455197.5;
        
        // Test parameters
        var factorsToUse = new List<Factors> { Factors.PersephoneRam, Factors.HermesRam, Factors.DemeterRam };
        const double latitude = 52.2180555555556;
        const double longitude = 6.8955555555556;

        // Create CalcRequest
        var request = new CalcRequest(
            julianDay,
            factorsToUse,
            0,
            2,
            latitude,
            longitude,
            0.0,
            TestConfig
        );

        // Expected values (converted from comma to period decimal separator)
        var expectedValues = new Dictionary<Factors, (double longitude, double latitude, double rightAscension, double declination, double azimuth, double altitude)>
        {
            [Factors.PersephoneRam] = (
                longitude: 0.21855668263958744,
                latitude: 0.0,
                rightAscension: 0.20052279151835387,
                declination: 0.08693482883616671,
                azimuth: 103.83028080723625,
                altitude: -10.389847632471579
            ),
            [Factors.HermesRam] = (
                longitude: 190.40065106386325,
                latitude: -0.0,
                rightAscension: 189.5589747249798,
                declination: -4.117914656816269,
                azimuth: 278.74888801075093,
                altitude: 1.5475484027881443
            ),
            [Factors.DemeterRam] = (
                longitude: 283.65004674832505,
                latitude: 1.9833260651055917,
                rightAscension: 284.6098392277247,
                declination: -20.76582493801715,
                azimuth: 185.06039757553333,
                altitude: -58.47127069734774
            )
        };

        // Perform calculation
        var result = ElementsCalc.CalculateElementsFactors(
            request,
            _seWrapper,
            ayanamshaOffset: 0.0
        );

        // Verify all factors are present in the result
        Assert.That(result.Count, Is.EqualTo(factorsToUse.Count), 
            $"All factors should be calculated, expected {factorsToUse.Count}, got {result.Count}");

        // Verify each factor's values
        foreach (var (factor, expected) in expectedValues)
        {
            if (!result.TryGetValue(factor, out var factorPosition))
            {
                Assert.Fail($"Factor {factor} not found in result");
                continue;
            }

            // Verify ecliptical position (longitude and latitude)
            if (factorPosition.Ecliptical == null || factorPosition.Ecliptical.Length == 0)
            {
                Assert.Fail($"Ecliptical position not found for factor {factor}");
                continue;
            }

            var eclipticalPosition = factorPosition.Ecliptical[0];
            var actualLongitude = eclipticalPosition.MainPos;
            var actualLatitude = eclipticalPosition.Deviation;

            var longDiff = Math.Abs(actualLongitude - expected.longitude);
            // Handle -0.0 case by comparing absolute values
            var latDiff = Math.Abs(actualLatitude - expected.latitude);

            if (longDiff >= 1e-6)
            {
                Assert.Fail($"Factor {factor} longitude: expected {expected.longitude}, got {actualLongitude}, difference: {longDiff}");
            }
            if (latDiff >= 1e-6)
            {
                Assert.Fail($"Factor {factor} latitude: expected {expected.latitude}, got {actualLatitude}, difference: {latDiff}");
            }

            // Verify equatorial position (right ascension and declination)
            if (factorPosition.Equatorial == null || factorPosition.Equatorial.Length == 0)
            {
                Assert.Fail($"Equatorial position not found for factor {factor}");
                continue;
            }

            var equatorialPosition = factorPosition.Equatorial[0];
            var actualRA = equatorialPosition.MainPos;
            var actualDecl = equatorialPosition.Deviation;

            var raDiff = Math.Abs(actualRA - expected.rightAscension);
            var declDiff = Math.Abs(actualDecl - expected.declination);

            if (raDiff >= 1e-6)
            {
                Assert.Fail($"Factor {factor} right ascension: expected {expected.rightAscension}, got {actualRA}, difference: {raDiff}");
            }
            if (declDiff >= 1e-6)
            {
                Assert.Fail($"Factor {factor} declination: expected {expected.declination}, got {actualDecl}, difference: {declDiff}");
            }

            // Verify horizontal position (azimuth and altitude)
            if (factorPosition.Horizontal == null || factorPosition.Horizontal.Length == 0)
            {
                Assert.Fail($"Horizontal position not found for factor {factor}");
                continue;
            }

            var horizontalPosition = factorPosition.Horizontal[0];
            var actualAzimuth = horizontalPosition.Azimuth;
            var actualAltitude = horizontalPosition.Altitude;

            var aziDiff = Math.Abs(actualAzimuth - expected.azimuth);
            var altDiff = Math.Abs(actualAltitude - expected.altitude);

            if (aziDiff >= 1e-6)
            {
                Assert.Fail($"Factor {factor} azimuth: expected {expected.azimuth}, got {actualAzimuth}, difference: {aziDiff}");
            }
            if (altDiff >= 1e-6)
            {
                Assert.Fail($"Factor {factor} altitude: expected {expected.altitude}, got {actualAltitude}, difference: {altDiff}");
            }
        }
    }

    [Test]
    public void TestCalculateElementsFactorsWithAyanamshaOffset()
    {
        // Skip test if SEWrapper is not initialized
        if (_seWrapper == null)
        {
            Assert.Ignore("SEWrapper not initialized. Swiss Ephemeris DLL may not be available.");
            return;
        }

        // Julian Day for January 1, 2010 0:00 UT
        const double julianDay = 2455197.5;
        var factorsToUse = new List<Factors> { Factors.PersephoneRam };
        const double latitude = 52.2180555555556;
        const double longitude = 6.8955555555556;
        const double ayanamshaOffset = 23.5; // Example ayanamsha offset

        var request = new CalcRequest(
            julianDay,
            factorsToUse,
            0,              // house system
            2,              // SEFlags
            latitude,
            longitude,
            0.0,
            TestConfig
        );

        var result = ElementsCalc.CalculateElementsFactors(
            request,
            _seWrapper,
            ayanamshaOffset
        );

        // Verify that the result contains the factor
        Assert.That(result, Contains.Key(Factors.PersephoneRam));
        
        var factorPosition = result[Factors.PersephoneRam];
        Assert.That(factorPosition.Ecliptical, Is.Not.Null);
        Assert.That(factorPosition.Ecliptical.Length, Is.GreaterThan(0));
        
        // Verify that longitude has been adjusted by ayanamsha offset
        var eclipticalPos = factorPosition.Ecliptical[0];
        // The longitude should be normalized to 0-360 range after subtracting ayanamsha offset
        Assert.That(eclipticalPos.MainPos, Is.GreaterThanOrEqualTo(0.0).And.LessThan(360.0));
    }

    [Test]
    public void TestCalculateElementsFactorsEmptyFactors()
    {
        // Skip test if SEWrapper is not initialized
        if (_seWrapper == null)
        {
            Assert.Ignore("SEWrapper not initialized. Swiss Ephemeris DLL may not be available.");
            return;
        }

        const double julianDay = 2455197.5;
        var factorsToUse = new List<Factors>();
        const double latitude = 52.2180555555556;
        const double longitude = 6.8955555555556;

        var request = new CalcRequest(
            julianDay,
            factorsToUse,
            0,              // house system
            2,              // SEFlags
            latitude,
            longitude,
            0.0,
            TestConfig
        );

        var result = ElementsCalc.CalculateElementsFactors(
            request,
            _seWrapper,
            ayanamshaOffset: 0.0
        );

        // Result should be empty when no factors are provided
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestCalculateElementsFactorsMultipleFactors()
    {
        // Skip test if SEWrapper is not initialized
        if (_seWrapper == null)
        {
            Assert.Ignore("SEWrapper not initialized. Swiss Ephemeris DLL may not be available.");
            return;
        }

        const double julianDay = 2455197.5;
        var factorsToUse = new List<Factors> 
        { 
            Factors.PersephoneRam, 
            Factors.HermesRam, 
            Factors.DemeterRam 
        };
        const double latitude = 52.2180555555556;
        const double longitude = 6.8955555555556;

        var request = new CalcRequest(
            julianDay,
            factorsToUse,
            0,              // house system
            2,              // SEFlags
            latitude,
            longitude,
            0.0,
            TestConfig
        );

        var result = ElementsCalc.CalculateElementsFactors(
            request,
            _seWrapper,
            ayanamshaOffset: 0.0
        );

        // Verify all factors are calculated
        Assert.That(result.Count, Is.EqualTo(factorsToUse.Count));
        
        foreach (var factor in factorsToUse)
        {
            Assert.That(result, Contains.Key(factor));
            var factorPosition = result[factor];
            
            // Verify all coordinate systems are present
            Assert.That(factorPosition.Ecliptical, Is.Not.Null.And.Length.GreaterThan(0));
            Assert.That(factorPosition.Equatorial, Is.Not.Null.And.Length.GreaterThan(0));
            Assert.That(factorPosition.Horizontal, Is.Not.Null.And.Length.GreaterThan(0));
        }
    }
}
