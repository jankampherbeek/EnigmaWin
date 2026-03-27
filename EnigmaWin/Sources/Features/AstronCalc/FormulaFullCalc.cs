// FormulaFullCalc.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Conversion;

namespace EnigmaWin.Sources.Features.AstronCalc;

/// <summary>
/// Calculate full positions for factors that require full coordinate calculations
/// (South Node, Priapus, Dragon, Beast).
/// </summary>
public static class FormulaFullCalc
{
    /// <summary>
    /// Calculate full positions for factors that require full coordinate calculations.
    /// </summary>
    /// <param name="seWrapper">Wrapper for Swiss Ephemeris functions.</param>
    /// <param name="calcRequest">Request containing calculation parameters.</param>
    /// <param name="obliquity">Obliquity value needed for coordinate conversions.</param>
    /// <param name="ayanamshaOffset">Offset for ayanamsha to obtain sidereal longitude.</param>
    /// <returns>Dictionary of factor positions.</returns>
    public static Dictionary<Factors, FullFactorPosition> CalculateFormulaFullFactors(
        SEWrapper seWrapper,
        CalcRequest calcRequest,
        double obliquity,
        double ayanamshaOffset)
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>();
        var julianDay = calcRequest.JulianDay;
        var julianDayPrevious = julianDay - 0.5;
        var julianDayNext = julianDay + 0.5;

        const int flagsEcliptical = 2 + 256;       // SEFLG_SWIEPH (2) + SEFLG_SPEED (256)
        const int flagsEquatorial = flagsEcliptical + 2048; // add equatorial flag (2048)

        // Orbital elements for Moon (needed for Dragon/Beast inclination)
        var seIdMoon = Factors.Moon.SeId();
        var orbitalElements = seWrapper.CalcOrbitalElements(julianDay, seIdMoon, flagsEcliptical);
        var inclination = orbitalElements.Inclination;

        foreach (var factor in calcRequest.FactorsToUse)
        {
            switch (factor)
            {
                case Factors.SouthNodeMean or Factors.SouthNodeTrue:
                {
                    // Calculate South Node from North Node (mean or true)
                    var nodeSeId = calcRequest.ConfigData.LunarNodeType == LunarNodeTypes.TrueNode ? 11 : 10;

                    var nodePos = CalculateFullPositionForSePointWithId(
                        seWrapper,
                        nodeSeId,
                        julianDay,
                        calcRequest.Latitude,
                        calcRequest.Longitude,
                        flagsEcliptical,
                        flagsEquatorial);

                    if (nodePos is null) continue;

                    var eclFirst = nodePos.Ecliptical[0];
                    var eqFirst = nodePos.Equatorial[0];
                    var horFirst = nodePos.Horizontal[0];

                    var nodeDistancePos = eclFirst.Distance;
                    var nodeDistanceSpeed = eclFirst.DistanceSpeed;

                    var nodeLongPos = eclFirst.MainPos + 180.0;
                    if (nodeLongPos >= 360.0) nodeLongPos -= 360.0;
                    var nodeLongSpeed = eclFirst.MainPosSpeed;

                    var eclLongPosSpeed = new MainAstronomicalPosition(
                        MainPos: RangeUtil.ValueToRange(nodeLongPos - ayanamshaOffset, 0.0, 360.0),
                        Deviation: 0.0,
                        Distance: nodeDistancePos,
                        MainPosSpeed: nodeLongSpeed,
                        DeviationSpeed: 0.0,
                        DistanceSpeed: nodeDistanceSpeed
                    );

                    var nodeRaPos = eqFirst.MainPos + 180.0;
                    if (nodeRaPos >= 360.0) nodeRaPos -= 360.0;
                    var nodeRaSpeed = eqFirst.MainPosSpeed;
                    var nodeDeclPos = -eqFirst.Deviation;
                    var nodeDeclSpeed = eqFirst.DeviationSpeed;

                    var raPosSpeed = new MainAstronomicalPosition(
                        MainPos: nodeRaPos,
                        Deviation: nodeDeclPos,
                        Distance: nodeDistancePos,
                        MainPosSpeed: nodeRaSpeed,
                        DeviationSpeed: nodeDeclSpeed,
                        DistanceSpeed: nodeDistanceSpeed
                    );

                    var nodeAzimuth = horFirst.Azimuth + 180.0;
                    if (nodeAzimuth >= 360.0) nodeAzimuth -= 360.0;
                    var nodeAltitude = -horFirst.Altitude;

                    var azimPosSpeed = new HorizontalPosition(
                        Azimuth: nodeAzimuth,
                        Altitude: nodeAltitude
                    );

                    var southNodeFpPos = new FullFactorPosition(
                        Ecliptical: [eclLongPosSpeed],
                        Equatorial: [raPosSpeed],
                        Horizontal: [azimPosSpeed]
                    );

                    coordinates[factor] = southNodeFpPos;
                    break;
                }

                case Factors.Priapus or Factors.PriapusCorrected:
                {
                    // Calculate Priapus from apogee (opposite of apogee)
                    Factors apogeeFactor;
                    if (factor == Factors.Priapus)
                    {
                        apogeeFactor = Factors.ApogeeMean;
                    }
                    else
                    {
                        apogeeFactor = calcRequest.ConfigData.BlackMoonCorrectionType switch
                        {
                            BlackMoonCorrectionTypes.Duval or BlackMoonCorrectionTypes.Swisseph => Factors.ApogeeCorrected,
                            BlackMoonCorrectionTypes.Interpolated => Factors.ApogeeInterpolated,
                            _ => Factors.ApogeeCorrected
                        };
                    }

                    FullFactorPosition? fullPointPosApogee;

                    // If using Duval correction, calculate via formula
                    if (apogeeFactor == Factors.ApogeeCorrected &&
                        calcRequest.ConfigData.BlackMoonCorrectionType == BlackMoonCorrectionTypes.Duval)
                    {
                        var apogeeCalc = new ApogeeDuvalCalc();
                        var longitude = apogeeCalc.CalcApogeeDuval(julianDay, seWrapper);
                        var longitudePrevious = apogeeCalc.CalcApogeeDuval(julianDayPrevious, seWrapper);
                        var longitudeNext = apogeeCalc.CalcApogeeDuval(julianDayNext, seWrapper);
                        var longitudeSpeed = longitudeNext - longitudePrevious;

                        var zeroPos = new MainAstronomicalPosition(0.0, 0.0, 0.0);
                        var zeroHor = new HorizontalPosition(0.0, 0.0);

                        var eclPos = new MainAstronomicalPosition(
                            MainPos: RangeUtil.ValueToRange(longitude - ayanamshaOffset, 0.0, 360.0),
                            Deviation: 0.0,
                            Distance: 0.0,
                            MainPosSpeed: longitudeSpeed,
                            DeviationSpeed: 0.0,
                            DistanceSpeed: 0.0
                        );

                        fullPointPosApogee = new FullFactorPosition(
                            Ecliptical: [eclPos],
                            Equatorial: [zeroPos],
                            Horizontal: [zeroHor]
                        );
                    }
                    else
                    {
                        fullPointPosApogee = CalculateFullPositionForSePoint(
                            seWrapper,
                            apogeeFactor,
                            julianDay,
                            calcRequest.Latitude,
                            calcRequest.Longitude,
                            flagsEcliptical,
                            flagsEquatorial
                        );
                    }

                    if (fullPointPosApogee is null) continue;

                    var apogeePos = fullPointPosApogee;
                    var eclFirst = apogeePos.Ecliptical[0];
                    var eqFirst = apogeePos.Equatorial[0];
                    var horFirst = apogeePos.Horizontal[0];

                    var eclLong = eclFirst.MainPos + 180.0;
                    if (eclLong >= 360.0) eclLong -= 360.0;

                    var eclipticPositions = new MainAstronomicalPosition(
                        MainPos: RangeUtil.ValueToRange(eclLong - ayanamshaOffset, 0.0, 360.0),
                        Deviation: -eclFirst.Deviation,
                        Distance: 0.0,
                        MainPosSpeed: eclFirst.MainPosSpeed,
                        DeviationSpeed: eclFirst.DeviationSpeed,
                        DistanceSpeed: 0.0
                    );

                    var ra = eqFirst.MainPos + 180.0;
                    if (ra >= 360.0) ra -= 360.0;

                    var equatorialPositions = new MainAstronomicalPosition(
                        MainPos: ra,
                        Deviation: -eqFirst.Deviation,
                        Distance: 0.0,
                        MainPosSpeed: eqFirst.MainPosSpeed,
                        DeviationSpeed: eqFirst.DeviationSpeed,
                        DistanceSpeed: 0.0
                    );

                    var azimuth = horFirst.Azimuth + 180.0;
                    if (azimuth >= 360.0) azimuth -= 360.0;

                    var horizontalPositions = new HorizontalPosition(
                        Azimuth: azimuth,
                        Altitude: -horFirst.Altitude
                    );

                    var priapusFullPos = new FullFactorPosition(
                        Ecliptical: [eclipticPositions],
                        Equatorial: [equatorialPositions],
                        Horizontal: [horizontalPositions]
                    );

                    coordinates[factor] = priapusFullPos;
                    break;
                }

                case Factors.Dragon or Factors.Beast:
                {
                    // Calculate Dragon/Beast from node with 90/-90 degree offset and inclination for RA etc.
                    var nodeSeId = calcRequest.ConfigData.LunarNodeType == LunarNodeTypes.TrueNode ? 11 : 10;

                    var fullPointPosNode = CalculateFullPositionForSePointWithId(
                        seWrapper,
                        nodeSeId,
                        julianDay,
                        calcRequest.Latitude,
                        calcRequest.Longitude,
                        flagsEcliptical,
                        flagsEquatorial
                    );

                    if (fullPointPosNode is null) continue;

                    var nodePos = fullPointPosNode;
                    var eclFirst = nodePos.Ecliptical[0];

                    var lunarOrbDistance = eclFirst.Distance;
                    var eclLongNode = eclFirst.MainPos;
                    var longSpeedNode = eclFirst.MainPosSpeed;

                    const double latitudeSpeed = 0.0; // latitude defined by inclination, relatively constant

                    var deltaNode = factor == Factors.Dragon ? 90.0 : -90.0;
                    var latitude = factor == Factors.Dragon ? inclination : -inclination;
                    var longitude = eclLongNode + deltaNode;
                    if (longitude >= 360.0) longitude -= 360.0;
                    if (longitude < 0.0) longitude += 360.0;

                    var previousLong = eclLongNode - longSpeedNode / 2.0;
                    var nextLong = eclLongNode + longSpeedNode / 2.0;

                    var (rightAscension, declination) = seWrapper.EclipticToEquatorial(
                        [eclLongNode, latitude],
                        obliquity);
                    var (previousRightAscension, previousDeclination) = seWrapper.EclipticToEquatorial(
                        [previousLong, latitude],
                        obliquity);
                    var (nextRightAscension, nextDeclination) = seWrapper.EclipticToEquatorial(
                        [nextLong, latitude],
                        obliquity);

                    var raSpeed = nextRightAscension - previousRightAscension;
                    var declinationSpeed = nextDeclination - previousDeclination;

                    var distanceSpeed = eclFirst.DistanceSpeed;

                    var horiz = seWrapper.AzimuthAndAltitude(
                        julianDay,
                        rightAscension,
                        declination,
                        calcRequest.Latitude,
                        calcRequest.Longitude,
                        0.0
                    );

                    var eclipticalPos = new MainAstronomicalPosition(
                        MainPos: RangeUtil.ValueToRange(longitude - ayanamshaOffset, 0.0, 360.0),
                        Deviation: latitude,
                        Distance: lunarOrbDistance,
                        MainPosSpeed: longSpeedNode,
                        DeviationSpeed: latitudeSpeed,
                        DistanceSpeed: distanceSpeed
                    );

                    var equatorialPos = new MainAstronomicalPosition(
                        MainPos: rightAscension,
                        Deviation: declination,
                        Distance: lunarOrbDistance,
                        MainPosSpeed: raSpeed,
                        DeviationSpeed: declinationSpeed,
                        DistanceSpeed: distanceSpeed
                    );

                    var horizontalPos = new HorizontalPosition(
                        Azimuth: horiz[0],
                        Altitude: horiz[1]
                    );

                    var fullPointPos = new FullFactorPosition(
                        Ecliptical: [eclipticalPos],
                        Equatorial: [equatorialPos],
                        Horizontal: [horizontalPos]
                    );

                    coordinates[factor] = fullPointPos;
                    break;
                }
            }
        }

        return coordinates;
    }
    

    private static FullFactorPosition? CalculateFullPositionForSePoint(
        SEWrapper seWrapper,
        Factors factor,
        double julianDay,
        double latitude,
        double longitude,
        int flagsEcliptical,
        int flagsEquatorial)
    {
        var factorId = factor.SeId();
        return CalculateFullPositionForSePointWithId(
            seWrapper,
            factorId,
            julianDay,
            latitude,
            longitude,
            flagsEcliptical,
            flagsEquatorial
        );
    }

    private static FullFactorPosition? CalculateFullPositionForSePointWithId(
        SEWrapper seWrapper,
        int seId,
        double julianDay,
        double latitude,
        double longitude,
        int flagsEcliptical,
        int flagsEquatorial)
    {
        // Calculate ecliptical position
        var eclipticalPos = SEWrapper.CalculateFactorPosition(
            julianDay,
            seId,
            flagsEcliptical
        );
        if (eclipticalPos is null) return null;

        // Calculate equatorial position
        var equatorialPos = SEWrapper.CalculateFactorPosition(
            julianDay,
            seId,
            flagsEquatorial
        );
        if (equatorialPos is null) return null;

        // Calculate horizontal position using equatorial coordinates
        var horiz = seWrapper.AzimuthAndAltitude(
            julianDay,
            equatorialPos.MainPos,
            equatorialPos.Deviation,
            latitude,
            longitude,
            0.0
        );

        var horizontalPos = new HorizontalPosition(
            Azimuth: horiz[0],
            Altitude: horiz[1]
        );

        return new FullFactorPosition(
            Ecliptical: [eclipticalPos],
            Equatorial: [equatorialPos],
            Horizontal: [horizontalPos]
        );
    }
}

