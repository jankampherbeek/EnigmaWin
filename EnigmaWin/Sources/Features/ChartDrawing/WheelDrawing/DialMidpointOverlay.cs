// DialMidpointOverlay.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Draws the interactive midpoint overlay on a dial wheel:
/// a red opposition line through the selected factor, and semi-transparent red
/// lines connecting each pair (B, C) whose midpoint coincides with the selected factor.
/// </summary>
public static class DialMidpointOverlay
{
    private const double GlyphFraction = 0.78;   // must match FPlanetGlyph in all Draw* classes
    private const double HitRadius     = 20.0;   // pixels for glyph hit testing
    private const double OrbDeg        = 1.6;    // dial-degree orb for midpoint matching

    // MARK: - Hit testing

    /// <summary>
    /// Returns the factor whose glyph is closest to <paramref name="location"/>,
    /// or <c>null</c> when no glyph is within <see cref="HitRadius"/> pixels.
    /// </summary>
    public static Factors? NearestFactor(Point location, Point center, double outerR,
                                          WheelPlotData data)
    {
        var glyphR = outerR * GlyphFraction;
        Factors? best      = null;
        var      bestDist  = double.MaxValue;

        foreach (var item in data.PlanetItems)
        {
            var pt   = WheelGeometry.PointOnCircle(item.PlotAngle, glyphR, center);
            var dx   = location.X - pt.X;
            var dy   = location.Y - pt.Y;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < HitRadius && dist < bestDist)
            {
                best     = item.Factor;
                bestDist = dist;
            }
        }
        return best;
    }

    // MARK: - Drawing

    /// <summary>
    /// Draws the opposition line and midpoint lines for <paramref name="selectedFactor"/>.
    /// Does nothing when the factor is not found in <paramref name="data"/>.
    /// </summary>
    public static void Draw(DrawingContext ctx, Point center, double outerR,
                             WheelPlotData data, Factors selectedFactor)
    {
        WheelPlotItem? selected = null;
        foreach (var item in data.PlanetItems)
        {
            if (item.Factor == selectedFactor) { selected = item; break; }
        }
        if (selected is null) return;

        var glyphR    = outerR * GlyphFraction;
        var strokeW   = Math.Max(1.0, outerR * 0.006);
        var redPen    = new Pen(Brushes.Red, strokeW);
        var redFaded  = new Pen(new SolidColorBrush(Color.FromArgb(191, 255, 0, 0)), strokeW); // 75 % opacity

        // Opposition line: selected glyph → opposite point
        var selAngle = selected.PlotAngle;
        var oppAngle = WheelGeometry.Normalise(selAngle + 180.0);
        var p1 = WheelGeometry.PointOnCircle(selAngle, glyphR, center);
        var p2 = WheelGeometry.PointOnCircle(oppAngle, glyphR, center);
        ctx.DrawLine(redPen, p1, p2);

        // Midpoint lines: pairs (B, C) whose midpoint coincides with the selected factor
        foreach (var (b, c) in MidpointPairs(selected, data))
        {
            var bp = WheelGeometry.PointOnCircle(b.PlotAngle, glyphR, center);
            var cp = WheelGeometry.PointOnCircle(c.PlotAngle, glyphR, center);
            ctx.DrawLine(redFaded, bp, cp);
        }
    }

    // MARK: - Midpoint pairs

    private static List<(WheelPlotItem, WheelPlotItem)> MidpointPairs(
        WheelPlotItem selected, WheelPlotData data)
    {
        var result   = new List<(WheelPlotItem, WheelPlotItem)>();
        var selAngle = selected.PlotAngle;
        var items    = new List<WheelPlotItem>();
        foreach (var item in data.PlanetItems)
            if (item.Factor != selected.Factor) items.Add(item);

        for (var i = 0; i < items.Count; i++)
        for (var j = i + 1; j < items.Count; j++)
        {
            var b    = items[i];
            var c    = items[j];
            var diff = WheelGeometry.Normalise(c.PlotAngle - b.PlotAngle);
            var mid1 = WheelGeometry.Normalise(b.PlotAngle + diff / 2.0);
            var mid2 = WheelGeometry.Normalise(mid1 + 180.0);

            if (AngularDistance(selAngle, mid1) < OrbDeg ||
                AngularDistance(selAngle, mid2) < OrbDeg)
                result.Add((b, c));
        }
        return result;
    }

    private static double AngularDistance(double a, double b)
    {
        var d = Math.Abs(WheelGeometry.Normalise(a - b));
        return Math.Min(d, 360.0 - d);
    }
}
