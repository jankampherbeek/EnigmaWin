// ObliqueLongitudeCalc.cs
// EnigmaWin
// Created by porting from ObliqueLongitudeCalc.swift on 27-01-2026

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;

namespace EnigmaWin.Sources.Features.AstronCalc;

/// <summary>
/// Calculate oblique longitudes for celestial points.
/// Oblique longitude is a correction for the mundane position, also called 'true location', 
/// as used by the School of Ram.
/// </summary>
public static class ObliqueLongitudeCalc
{
    /// <summary>
    /// Calculate oblique longitudes for factors.
    /// </summary>
    /// <param name="armc">Right Ascension of the Midheaven (ARMC) in degrees.</param>
    /// <param name="obliquity">Obliquity of the ecliptic in degrees.</param>
    /// <param name="geoLat">Geographic latitude in degrees.</param>
    /// <param name="factorCoordinates">Array of named ecliptic coordinates to calculate oblique longitudes for.</param>
    /// <param name="ayanamshaOffset">Ayanamsha offset in degrees.</param>
    /// <returns>Array of named ecliptic longitudes with their oblique longitude values.</returns>
    public static NamedEclipticLongitude[] ObliqueLongitudeForFactor(
        double armc,
        double obliquity,
        double geoLat,
        NamedEclipticCoordinates[] factorCoordinates,
        double ayanamshaOffset)
    {
        var results = new List<NamedEclipticLongitude>();

        // Calculate the south point
        var southPoint = CalculateSouthPoint(armc, obliquity, geoLat);
        var southPointLong = southPoint.Item1;
        var southPointLat = southPoint.Item2;

        foreach (var coord in factorCoordinates)
        {
            var eclLong = coord.Longitude;
            var eclLat = coord.Latitude;

            var obliqueLongitude = OblLongForCelPoint(
                eclLong,
                eclLat,
                southPointLong,
                southPointLat,
                ayanamshaOffset
            );

            // Apply ayanamsha offset and normalize to 0-360
            var adjustedLongitude = RangeUtil.ValueToRange(
                obliqueLongitude + ayanamshaOffset,
                0.0,
                360.0
            );

            results.Add(new NamedEclipticLongitude(
                coord.Factor,
                adjustedLongitude
            ));
        }

        return results.ToArray();
    }


    /// <summary>
    /// Calculate the south point
    /// </summary>
    private static (double, double) CalculateSouthPoint(double armc, double obliquity, double geoLat)
    {
        var declSp = -(90.0 - geoLat);
        var arsp = armc;
        if (geoLat < 0.0)
        {
            arsp = RangeUtil.ValueToRange(armc + 180.0, 0.0, 360.0);
            declSp = -90.0 - geoLat;
        }

        var sinSp = Math.Sin(MathExtra.DegToRad(arsp));
        var cosEps = Math.Cos(MathExtra.DegToRad(obliquity));
        var tanDecl = Math.Tan(MathExtra.DegToRad(declSp));
        var sinEps = Math.Sin(MathExtra.DegToRad(obliquity));
        var cosArsp = Math.Cos(MathExtra.DegToRad(arsp));
        var sinDecl = Math.Sin(MathExtra.DegToRad(declSp));
        var cosDecl = Math.Cos(MathExtra.DegToRad(declSp));

        var longSp = RangeUtil.ValueToRange(
            MathExtra.RadToDeg(Math.Atan2((sinSp * cosEps) + (tanDecl * sinEps), cosArsp)),
            0.0,
            360.0
        );

        var latSp = MathExtra.RadToDeg(
            Math.Asin((sinDecl * cosEps) - (cosDecl * sinEps * sinSp))
        );

        return (longSp, latSp);
    }


    private static double OblLongForCelPoint(
        double eclLong,
        double eclLat,
        double southPointLong,
        double southPointLat,
        double ayanamshaOffset)
    {
        var absLatSp = Math.Abs(southPointLat);
        var longSp = southPointLong;
        var longPl = eclLong + ayanamshaOffset;
        var latPl = eclLat;

        var longSouthPMinusPlanet = Math.Abs(longSp - longPl);
        var longPlanetMinusSouthP = Math.Abs(longPl - longSp);
        var latSouthPMinusPlanet = absLatSp - latPl;
        var latSouthPPlusPlanet = absLatSp + latPl;

        var s = Math.Min(longSouthPMinusPlanet, longPlanetMinusSouthP) / 2.0;
        var tanSRad = Math.Tan(MathExtra.DegToRad(s));
        var qRad = Math.Sin(MathExtra.DegToRad(latSouthPMinusPlanet)) /
                   Math.Sin(MathExtra.DegToRad(latSouthPPlusPlanet));

        var v = MathExtra.RadToDeg(Math.Atan(tanSRad * qRad)) - s;

        var absoluteV = RangeUtil.ValueToRange(Math.Abs(v), -90.0, 90.0);
        absoluteV = Math.Abs(absoluteV);

        double correctedV;
        if (IsRising(longSp, longPl))
        {
            correctedV = latPl < 0.0 ? absoluteV : -absoluteV;
        }
        else
        {
            correctedV = latPl > 0.0 ? absoluteV : -absoluteV;
        }

        return RangeUtil.ValueToRange(longPl + correctedV, 0.0, 360.0);
    }


    private static bool IsRising(double longSp, double longPl)
    {
        var diff = longPl - longSp;
        if (diff < 0.0) diff += 360.0;
        if (diff >= 360.0) diff -= 360.0;
        return diff < 180.0;
    }
}

