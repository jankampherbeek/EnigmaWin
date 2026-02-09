// AstronCalcDomain
// EnigmaWin
// Created by Jan Kampherbeek on 27-1-2026

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.AstronCalc;


/// <summary>A fully defined astronomical position for a specific coordinate. Includes positions and speeds.</summary>
/// <remarks>
/// mainPos is the main position: longitude or right ascension
/// deviation is the deviation from the main plane: latitude or declination
/// distance is the distance in Astronomical Units (AU)
/// the postfix 'Speed' gives the speed for the item with the same name
/// </remarks>
/// <param name="MainPos">The main position: longitude or right ascension</param>
/// <param name="Deviation">The deviation from the main plane: latitude or declination</param>
/// <param name="Distance">The distance in Astronomical Units (AU)</param>
/// <param name="MainPosSpeed">The speed for the main position</param>
/// <param name="DeviationSpeed">The speed for the deviation</param>
/// <param name="DistanceSpeed">The speed for the distance</param>
public record MainAstronomicalPosition(
    double MainPos,
    double Deviation,
    double Distance,
    double MainPosSpeed = 0,
    double DeviationSpeed = 0,
    double DistanceSpeed = 0);

/// <summary>The position at the horizontal plane.</summary>
/// <remarks>
/// Azimuth: the position at the horizon, it starts at the South and runs through west, north and east, in that sequence
/// Altitude: the height above or under the horizon
/// </remarks>
/// <param name="Azimuth">The position at the horizon, starting at the South and running through west, north and east</param>
/// <param name="Altitude">The height above or under the horizon</param>
public record HorizontalPosition(
    double Azimuth,
    double Altitude);

/// <summary>The position of a cusp.</summary>
/// <param name="Longitude">The ecliptic longitude</param>
/// <param name="RightAscension">The right ascension</param>
/// <param name="Declination">The declination</param>
/// <param name="Horizontal">The horizontal position</param>
public record FullCuspPosition(
    double Longitude,
    double RightAscension,
    double Declination,
    HorizontalPosition Horizontal);

/// <summary>Combined positions for all coordinates.</summary>
/// <param name="Ecliptical">Array of ecliptical positions</param>
/// <param name="Equatorial">Array of equatorial positions</param>
/// <param name="Horizontal">Array of horizontal positions</param>
public record FullFactorPosition(
    MainAstronomicalPosition[] Ecliptical,
    MainAstronomicalPosition[] Equatorial,
    HorizontalPosition[] Horizontal);

/// <summary>Combined positions for all mundane points.</summary>
/// <param name="Cusps">Array of cusp positions</param>
/// <param name="Ascendant">The ascendant position</param>
/// <param name="Midheaven">The midheaven position</param>
/// <param name="Eastpoint">The eastpoint position</param>
/// <param name="Vertex">The vertex position</param>
public record HousePositions(
    FullCuspPosition[] Cusps,
    FullCuspPosition Ascendant,
    FullCuspPosition Midheaven,
    FullCuspPosition Eastpoint,
    FullCuspPosition Vertex);


/// <summary>Orbital elements of a celestial body.</summary>
/// <remarks>All angles are in degrees, distances in Astronomical Units (AU).</remarks>
/// <param name="SemiMajorAxis">Semi-major axis (a) in AU.</param>
/// <param name="Eccentricity">Eccentricity (e).</param>
/// <param name="Inclination">Inclination (i) in degrees.</param>
/// <param name="ArgumentOfPerihelion">Argument of perihelion (ω) in degrees.</param>
/// <param name="AscendingNode">Ascending node (Ω) in degrees.</param>
/// <param name="MeanAnomaly">Mean anomaly (M) in degrees.</param>
/// <param name="TrueAnomaly">True anomaly (v) in degrees.</param>
/// <param name="EccentricAnomaly">Eccentric anomaly (E) in degrees.</param>
/// <param name="MeanLongitude">Mean longitude (L) in degrees.</param>
/// <param name="TrueLongitude">True longitude (L') in degrees.</param>
/// <param name="Distance">Distance (r) in AU.</param>
/// <param name="Speed">Speed (v) in AU/day.</param>
public record OrbitalElements(
    double SemiMajorAxis,
    double Eccentricity,
    double Inclination,
    double ArgumentOfPerihelion,
    double AscendingNode,
    double MeanAnomaly,
    double TrueAnomaly,
    double EccentricAnomaly,
    double MeanLongitude,
    double TrueLongitude,
    double Distance,
    double Speed);


/// <summary>Result of apsides calculation containing nodes and apsides positions.</summary>
/// <remarks>Each position contains [longitude, latitude, distance].</remarks>
/// <param name="AscendingNode">Ascending node position [longitude, latitude, distance].</param>
/// <param name="DescendingNode">Descending node position [longitude, latitude, distance].</param>
/// <param name="Perihelion">Perihelion (for planets) or Perigee (for Moon) position [longitude, latitude, distance].</param>
/// <param name="Aphelion">Aphelion (for planets) or Apogee (for Moon) position [longitude, latitude, distance].</param>
public record ApsidesResult(
    double[] AscendingNode,
    double[] DescendingNode,
    double[] Perihelion,
    double[] Aphelion);


/// <summary>Represents a named celestial point with its ecliptic coordinates.</summary>
/// <param name="Factor">The factor this position belongs to.</param>
/// <param name="Longitude">The ecliptic longitude.</param>
/// <param name="Latitude">The ecliptic latitude.</param>
public record NamedEclipticCoordinates(
    Factors Factor,
    double Longitude,
    double Latitude);


/// <summary>Represents a named celestial point with its oblique longitude.</summary>
/// <param name="Factor">The factor this position belongs to.</param>
/// <param name="ObliqueLongitude">The oblique ecliptic longitude.</param>
public record NamedEclipticLongitude(
    Factors Factor,
    double ObliqueLongitude);


