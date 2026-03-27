// MathExtra.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Features.AstronCalc;

/// <summary>Mathematical utility functions for coordinate conversions.</summary>
public static class MathExtra
{
    /// <summary>Convert rectangular coordinates to polar coordinates.</summary>
    /// <param name="rect">The rectangular coordinates.</param>
    /// <returns>The polar coordinates.</returns>
    public static PolarCoordinates Rectangular2Polar(RectAngCoordinates rect)
    {
        var r = Math.Sqrt(rect.XCoord * rect.XCoord + rect.YCoord * rect.YCoord + rect.ZCoord * rect.ZCoord);
        var phi = Math.Atan2(rect.YCoord, rect.XCoord);
        var theta = Math.Asin(rect.ZCoord / r);
        return new PolarCoordinates(phi, theta, r);
    }

    /// <summary>Convert polar coordinates to rectangular coordinates.</summary>
    /// <param name="polar">The polar coordinates.</param>
    /// <returns>The rectangular coordinates.</returns>
    public static RectAngCoordinates Polar2Rectangular(PolarCoordinates polar)
    {
        var x = polar.RCoord * Math.Cos(polar.ThetaCoord) * Math.Cos(polar.PhiCoord);
        var y = polar.RCoord * Math.Cos(polar.ThetaCoord) * Math.Sin(polar.PhiCoord);
        var z = polar.RCoord * Math.Sin(polar.ThetaCoord);
        return new RectAngCoordinates(x, y, z);
    }

    /// <summary>Convert radians to degrees.</summary>
    /// <param name="radians">The value in radians.</param>
    /// <returns>The value in degrees.</returns>
    public static double RadToDeg(double radians)
    {
        return radians * 180.0 / Math.PI;
    }

    /// <summary>Convert degrees to radians.</summary>
    /// <param name="degrees">The value in degrees.</param>
    /// <returns>The value in radians.</returns>
    public static double DegToRad(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
