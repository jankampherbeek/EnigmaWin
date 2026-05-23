// WheelGeometry.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Windows;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>Geometry calculations for wheel drawing: points on circles, angle conversions.</summary>
public static class WheelGeometry
{
    public static Point PointOnCircle(double angleDeg, double radius, Point center)
    {
        var rad = angleDeg * Math.PI / 180.0;
        var x = center.X - Math.Sin(rad) * radius;
        var y = center.Y - Math.Cos(rad) * radius;
        return new Point(x, y);
    }

    public static double MundaneAngle(double longitude, double ascendantLongitude)
    {
        var angle = longitude - ascendantLongitude + 90.0;
        angle = angle % 360.0;
        if (angle < 0) angle += 360.0;
        return angle;
    }

    public static double SignOffset(double ascendantLongitude) =>
        30.0 - ascendantLongitude % 30.0;

    public static double Normalise(double angle)
    {
        var a = angle % 360.0;
        if (a < 0) a += 360.0;
        return a;
    }
}
