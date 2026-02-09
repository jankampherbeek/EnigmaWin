// ElementsCalc.cs
// EnigmaWin
// Created by Jan Kampherbeek on 24-01-2026

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Conversion;

namespace EnigmaWin.Sources.Features.AstronCalc;

// MARK: - Data Structures

/// <summary>Representation of rectangular coordinates.</summary>
public record RectAngCoordinates(
    double XCoord,
    double YCoord,
    double ZCoord);

/// <summary>Representation of polar coordinates.</summary>
public record PolarCoordinates(
    double PhiCoord,
    double ThetaCoord,
    double RCoord);

/// <summary>Data for an orbit definition.</summary>
public record OrbitDefinition(
    double[] MeanAnomaly,
    double[] Eccentricity,
    double SemiMajorAxis,
    double[] ArgumentPerihelion,
    double[] AscendingNode,
    double[] Inclination);

// MARK: - Common Elements Calculator

/// <summary>Calculator for celestial positions using orbital elements.</summary>
public static class ElementsCalc
{
    /// <summary>
    /// Calculate positions for factors using orbital elements.
    /// </summary>
    /// <param name="request">The calculation request.</param>
    /// <param name="seWrapper">The Swiss Ephemeris wrapper instance.</param>
    /// <param name="ayanamshaOffset">The ayanamsha offset in degrees.</param>
    /// <returns>A dictionary mapping factors to their full positions.</returns>
    public static Dictionary<Factors, FullFactorPosition> CalculateElementsFactors(
        CalcRequest request,
        SEWrapper seWrapper,
        double ayanamshaOffset)
    {
        var julianDay = request.JulianDay;
        var julianDayPrevious = julianDay - 0.5;
        var julianDayNext = julianDay + 0.5;

        // Calculate obliquity (needed for coordinate conversion)
        const int eclipticalFlags = 258; // SEFLG_SWIEPH (2) + SEFLG_SPEED (256)
        var obliquityPosition = SEWrapper.CalculateFactorPosition(
            julianDay,
            -1,
            eclipticalFlags
        );
        var obliquity = obliquityPosition?.MainPos ?? 0.0;

        // Determine observer position from flags
        var observerPosition = request.ConfigData.ObserverPosition;
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        var calcHelioPos = new CalcHelioPos();

        foreach (var factor in request.FactorsToUse)
        {
            // Calculate position using orbital elements
            var position = CalculatePosition(
                factor,
                julianDay,
                observerPosition,
                ref calcHelioPos
            );
            // Calculate position 0.5 day before and after the calculated time, to define the speed
            var previousPosition = CalculatePosition(
                factor,
                julianDayPrevious,
                observerPosition,
                ref calcHelioPos
            );
            var nextPosition = CalculatePosition(
                factor,
                julianDayNext,
                observerPosition,
                ref calcHelioPos
            );
            var longSpeed = nextPosition.longitude - previousPosition.longitude;
            var latSpeed = nextPosition.latitude - previousPosition.latitude;
            var radvSpeed = nextPosition.distance - previousPosition.distance;

            // Convert to all coordinate systems
            var eclipticalPos = new MainAstronomicalPosition(
                MainPos: RangeUtil.ValueToRange(position.longitude - ayanamshaOffset, 0.0, 360.0),
                Deviation: position.latitude,
                Distance: position.distance,
                MainPosSpeed: longSpeed,
                DeviationSpeed: latSpeed,
                DistanceSpeed: radvSpeed
            );

            // Convert to equatorial
            var (rightAscension, declination) = seWrapper.EclipticToEquatorial(
                [position.longitude, position.latitude],
                obliquity
            );
            var (previousRightAscension, previousDeclination) = seWrapper.EclipticToEquatorial(
                [previousPosition.longitude, previousPosition.latitude],
                obliquity
            );
            var (nextRightAscension, nextDeclination) = seWrapper.EclipticToEquatorial(
                [nextPosition.longitude, nextPosition.latitude],
                obliquity
            );
            var raSpeed = nextRightAscension - previousRightAscension;
            var declinationSpeed = nextDeclination - previousDeclination;

            var equatorialPos = new MainAstronomicalPosition(
                MainPos: rightAscension,
                Deviation: declination,
                Distance: position.distance,
                MainPosSpeed: raSpeed,
                DeviationSpeed: declinationSpeed,
                DistanceSpeed: radvSpeed
            );

            // Calculate horizontal position
            var horizontalCoords = seWrapper.AzimuthAndAltitude(
                julianDay,
                rightAscension,
                declination,
                request.Latitude,
                request.Longitude,
                0.0
            );
            var horizontalPos = new HorizontalPosition(
                Azimuth: horizontalCoords[0],
                Altitude: horizontalCoords[1]
            );

            var fullPosition = new FullFactorPosition(
                Ecliptical: [eclipticalPos],
                Equatorial: [equatorialPos],
                Horizontal: [horizontalPos]
            );

            coordinates[factor] = fullPosition;
        }

        return coordinates;
    }

    // MARK: - Helper Methods

    private static (double longitude, double latitude, double distance) CalculatePosition(
        Factors factor,
        double julianDay,
        ObserverPositions observerPosition,
        ref CalcHelioPos calcHelioPos)
    {
        var factorT = FactorT(julianDay);

        if (observerPosition == ObserverPositions.Geocentric || observerPosition == ObserverPositions.Topocentric)
        {
            // Calculate geocentric position
            var earthOrbit = DefineOrbitDefinition(Factors.Earth);
            var rectAngEarthHelio = calcHelioPos.CalcEclipticPosition(factorT, earthOrbit);

            var planetOrbit = DefineOrbitDefinition(factor);
            var rectAngPlanetHelio = calcHelioPos.CalcEclipticPosition(factorT, planetOrbit);

            // Calculate geocentric position by subtracting Earth's position
            var rectAngPlanetGeo = new RectAngCoordinates(
                XCoord: rectAngPlanetHelio.XCoord - rectAngEarthHelio.XCoord,
                YCoord: rectAngPlanetHelio.YCoord - rectAngEarthHelio.YCoord,
                ZCoord: rectAngPlanetHelio.ZCoord - rectAngEarthHelio.ZCoord
            );

            var polarPlanetGeo = MathExtra.Rectangular2Polar(rectAngPlanetGeo);
            return DefinePosition(polarPlanetGeo);
        }
        else
        {
            // Calculate heliocentric position
            var planetOrbit = DefineOrbitDefinition(factor);
            var rectAngPlanetHelio = calcHelioPos.CalcEclipticPosition(factorT, planetOrbit);
            var polarPlanetHelio = MathExtra.Rectangular2Polar(rectAngPlanetHelio);
            return DefinePosition(polarPlanetHelio);
        }
    }

    private static (double longitude, double latitude, double distance) DefinePosition(PolarCoordinates polarPlanet)
    {
        var posLong = MathExtra.RadToDeg(polarPlanet.PhiCoord);
        if (posLong < 0.0) posLong += 360.0;
        var posLat = MathExtra.RadToDeg(polarPlanet.ThetaCoord);
        var posDist = MathExtra.RadToDeg(polarPlanet.RCoord);
        return (posLong, posLat, posDist);
    }

    private static double FactorT(double julianDay)
    {
        return (julianDay - 2415020.5) / 36525.0;
    }

    private static OrbitDefinition DefineOrbitDefinition(Factors factor)
    {
        double[] meanAnomaly;
        var eccentricity = new double[] { 0, 0, 0 };
        var argumentPerihelion = new double[] { 0, 0, 0 };
        var ascendingNode = new double[] { 0, 0, 0 };
        var inclination = new double[] { 0, 0, 0 };
        double semiMajorAxis;

        switch (factor)
        {
            case Factors.Earth:
                meanAnomaly = [358.47584, 35999.0498, -0.00015];
                eccentricity = [0.016751, -0.41e-4, 0];
                semiMajorAxis = 1.00000013;
                argumentPerihelion = [101.22083, 1.71918, 0.00045];
                break;

            case Factors.PersephoneRam:
                meanAnomaly = [295.0, 60, 0];
                semiMajorAxis = 71.137866;
                break;

            case Factors.HermesRam:
                meanAnomaly = [134.7, 50.0, 0];
                semiMajorAxis = 80.331954;
                break;

            case Factors.DemeterRam:
                meanAnomaly = [114.6, 40, 0];
                semiMajorAxis = 93.216975;
                ascendingNode = [125, 0, 0];
                inclination = [5.5, 0, 0];
                break;

            default:
                // Fallback for unsupported factors
                meanAnomaly = [0, 0, 0];
                semiMajorAxis = 1.0;
                break;
        }

        return new OrbitDefinition(
            MeanAnomaly: meanAnomaly,
            Eccentricity: eccentricity,
            SemiMajorAxis: semiMajorAxis,
            ArgumentPerihelion: argumentPerihelion,
            AscendingNode: ascendingNode,
            Inclination: inclination
        );
    }
}

// MARK: - Heliocentric Position Calculator

/// <summary>Calculate heliocentric rectangular positions for celestial points.</summary>
public struct CalcHelioPos
{
    private double _meanAnomaly2;
    private double _semiAxis;
    private double _inclination;
    private double _eccentricity;
    private double _eccAnomaly;
    private double _factorT;

    /// <summary>Calculate ecliptic position using orbital elements.</summary>
    public RectAngCoordinates CalcEclipticPosition(double factorT, OrbitDefinition orbitDefinition)
    {
        _factorT = factorT;

        var meanAnomaly1 = MathExtra.DegToRad(ProcessTermsForFractionT(factorT, orbitDefinition.MeanAnomaly));
        if (meanAnomaly1 < 0.0) meanAnomaly1 += 2 * Math.PI;

        _eccentricity = ProcessTermsForFractionT(factorT, orbitDefinition.Eccentricity);
        _eccAnomaly = EccAnomalyFromKeplerEquation(meanAnomaly1, _eccentricity);

        var trueAnomalyPol = CalcPolarTrueAnomaly(orbitDefinition);
        ReduceToEcliptic(trueAnomalyPol, orbitDefinition);

        return CalcRectAngHelioCoordinates(
            _semiAxis,
            _inclination,
            _eccAnomaly,
            _eccentricity,
            _meanAnomaly2,
            orbitDefinition
        );
    }

    private double EccAnomalyFromKeplerEquation(double meanAnomaly, double eccentricity)
    {
        var eccAnomaly = meanAnomaly;
        for (var i = 1; i < 6; i++)
        {
            eccAnomaly = meanAnomaly + (eccentricity * Math.Sin(eccAnomaly));
        }
        return eccAnomaly;
    }

    private PolarCoordinates CalcPolarTrueAnomaly(OrbitDefinition orbitDefinition)
    {
        var xCoord = orbitDefinition.SemiMajorAxis * (Math.Cos(_eccAnomaly) - _eccentricity);
        var yCoord = orbitDefinition.SemiMajorAxis * Math.Sin(_eccAnomaly) * Math.Sqrt(1 - (_eccentricity * _eccentricity));
        var zCoord = 0.0;
        var anomalyVec = new RectAngCoordinates(xCoord, yCoord, zCoord);
        return MathExtra.Rectangular2Polar(anomalyVec);
    }

    private void ReduceToEcliptic(PolarCoordinates trueAnomalyPol, OrbitDefinition orbitDefinition)
    {
        _semiAxis = MathExtra.RadToDeg(trueAnomalyPol.PhiCoord) + ProcessTermsForFractionT(_factorT, orbitDefinition.ArgumentPerihelion);
        _meanAnomaly2 = MathExtra.DegToRad(ProcessTermsForFractionT(_factorT, orbitDefinition.AscendingNode));

        var factorVDeg = _semiAxis + MathExtra.RadToDeg(_meanAnomaly2);
        if (factorVDeg < 0.0) factorVDeg += 360.0;
        var factorVRad = MathExtra.DegToRad(factorVDeg);

        _inclination = MathExtra.DegToRad(ProcessTermsForFractionT(_factorT, orbitDefinition.Inclination));
        _semiAxis = Math.Atan(Math.Cos(_inclination) * Math.Tan(factorVRad - _meanAnomaly2));
        if (_semiAxis < Math.PI) _semiAxis += Math.PI;
        _semiAxis = MathExtra.RadToDeg(_semiAxis + _meanAnomaly2);
        if (Math.Abs(factorVDeg - _semiAxis) > 10.0) _semiAxis -= 180.0;
    }

    private RectAngCoordinates CalcRectAngHelioCoordinates(
        double semiAxis,
        double inclination,
        double eccAnomaly,
        double eccentricity,
        double meanAnomaly,
        OrbitDefinition orbitDefinition)
    {
        var phiCoord = MathExtra.DegToRad(semiAxis);
        if (phiCoord < 0.0) phiCoord += 2 * Math.PI;
        var thetaCoord = Math.Atan(Math.Sin(phiCoord - meanAnomaly) * Math.Tan(inclination));
        var rCoord = MathExtra.DegToRad(orbitDefinition.SemiMajorAxis) * (1 - (eccentricity * Math.Cos(eccAnomaly)));
        var helioPol = new PolarCoordinates(phiCoord, thetaCoord, rCoord);
        return MathExtra.Polar2Rectangular(helioPol);
    }

    private double ProcessTermsForFractionT(double fractionT, double[] elements)
    {
        return elements[0] + (elements[1] * fractionT) + (elements[2] * fractionT * fractionT);
    }
}
