// WheelGeometry.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using System;
using Avalonia;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>Geometry calculations for wheel drawing: points on circles, angle conversions.</summary>
public static class WheelGeometry
{
    /// <summary>
    /// Returns the Point on a circle at the given angle and radius, relative to center.
    /// Wheel convention: 0° is at the top (12 o'clock), angles increase clockwise.
    /// </summary>
    public static Point PointOnCircle(double angleDeg, double radius, Point center)
    {
        var rad = angleDeg * Math.PI / 180.0;
        var x = center.X - Math.Sin(rad) * radius;
        var y = center.Y - Math.Cos(rad) * radius;
        return new Point(x, y);
    }

    /// <summary>
    /// Converts an ecliptic longitude to a wheel angle,
    /// placing the ascendant at 9 o'clock (270° → 90°).
    /// </summary>
    public static double MundaneAngle(double longitude, double ascendantLongitude)
    {
        var angle = longitude - ascendantLongitude + 90.0;
        angle = angle % 360.0;
        if (angle < 0) angle += 360.0;
        return angle;
    }

    /// <summary>Offset in degrees for the first sign boundary after the ascendant.</summary>
    public static double SignOffset(double ascendantLongitude)
    {
        return 30.0 - ascendantLongitude % 30.0;
    }

    /// <summary>Normalises an angle to the range [0, 360).</summary>
    public static double Normalise(double angle)
    {
        var a = angle % 360.0;
        if (a < 0) a += 360.0;
        return a;
    }
}
